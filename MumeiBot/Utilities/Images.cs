using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MumeiBot.Utilities
{
    public class Images
    {
        public async Task<string> CreateImageAsync(SocketGuildUser user, string url = null)
        {
            var avatar = await FetchImageAsync(user.GetAvatarUrl(size: 2048, format: Discord.ImageFormat.Png) ?? user.GetDefaultAvatarUrl());

            var background = await FetchImageAsync("https://images.unsplash.com/photo-1528360983277-13d401cdc186?ixid=MnwxMjA3fDB8MHxzZWFyY2h8MjR8fGFuaW1lfGVufDB8MHwwfHw%3D&ixlib=rb-1.2.1&auto=format&fit=crop&w=400&q=60");
            
            if (url != null)
                background = await FetchImageAsync(url);

            background = CropToBanner(background);
            avatar = ClipImageToCircle(avatar);

            //convert avatar to bitmap
            var bitmap = avatar as Bitmap;
            //? checks if null, so if its not null, makes bitmap transparent
            bitmap?.MakeTransparent();

            var banner = CopyRegionIntoImage(bitmap, background);
            banner = DrawTextToImage(banner, $"サーバーへようこそ！", $"{user.Username}#{user.Discriminator}");

            string path = $"{Guid.NewGuid()}.png";
            banner.Save(path);
            return await Task.FromResult(path);
        }

        //crops to banner size
        private static Bitmap CropToBanner(Image image)
        {
            var originalWidth = image.Width;
            var originalHeight = image.Height;
            var destinationSize = new Size(1100, 450); //1100 x 450

            var heightRatio = (float)originalHeight / destinationSize.Height;
            var widthRatio = (float)originalWidth / destinationSize.Width;

            var ratio = Math.Min(heightRatio, widthRatio); //returns smaller one of the two

            var heightScale = Convert.ToInt32(destinationSize.Height * ratio);
            var widthScale = Convert.ToInt32(destinationSize.Width * ratio);

            var startX = (originalWidth - widthScale) / 2;
            var startY = (originalHeight - heightScale) / 2;

            //sourceRectangle: new rectangle based upon original height and width
            var sourceRectangle = new Rectangle(startX, startY, widthScale, heightScale);
            //bitmap based upon destination size
            var bitmap = new Bitmap(destinationSize.Width, destinationSize.Height);
            //based upon destination size
            var destinationRectangle = new Rectangle(0, 0, bitmap.Width, bitmap.Height);

            using var g = Graphics.FromImage(bitmap);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.DrawImage(image, destinationRectangle, sourceRectangle, GraphicsUnit.Pixel);

            return bitmap;
        }

        private Image ClipImageToCircle(Image image)
        {

            //create new image, which is a bitmap of those arguments
            //create 3 variables for the center of the image
            Image destination = new Bitmap(image.Width, image.Height, image.PixelFormat);
            var radius = image.Width / 2;
            var x = image.Width / 2;
            var y = image.Height / 2;

            //get the graphics from the destination
            using Graphics g = Graphics.FromImage(destination);
            //rectangle 
            var r = new Rectangle(x - radius, y - radius, radius * 2, radius * 2);

            //highest quality image
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.SmoothingMode = SmoothingMode.AntiAlias;
           
            //transparent brush to make rectangle blank
            using(Brush brush = new SolidBrush(Color.Transparent))
            {
                g.FillRectangle(brush, 0, 0, destination.Width, destination.Height);
            }

            //put on the image at the transparent rectangle
            var path = new GraphicsPath();
            //which is at the end a circle
            path.AddEllipse(r);
            g.SetClip(path);
            g.DrawImage(image, 0, 0);
            //return the circle in the image
            return destination;
        }

        private Image CopyRegionIntoImage(Image source, Image destination)
        {
            //avatar ON background
            using var grD = Graphics.FromImage(destination);
            //puts them at the center
            var X = (destination.Width / 2) - 110;
            var Y = (destination.Height / 2) - 155;

            grD.DrawImage(source, X, Y, 220, 220);
            return destination;
        }

        private Image DrawTextToImage (Image image, string header, string subheader)
        {
            var rampartOne = new Font("Rampart One", 40, FontStyle.Bold);
            var rampartOneSmall = new Font("Rampart One", 23, FontStyle.Regular);

            var brushWhite = new SolidBrush(ColorTranslator.FromHtml("#ff6961"));
            var brushTaro = new SolidBrush(ColorTranslator.FromHtml("#FFFF00"));

            var headerX = image.Width / 2;
            var headerY = (image.Height / 2) + 115;

            var subheaderX = image.Width / 2;
            var subheaderY = (image.Height / 2) + 160;

            var drawFormat = new StringFormat
            {
                //auto aligns to center instead of manual calculation
                LineAlignment = StringAlignment.Center,
                Alignment = StringAlignment.Center
            };

            using var GrD = Graphics.FromImage(image);
            GrD.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            GrD.DrawString(header, rampartOne, brushWhite, headerX, headerY, drawFormat);
            GrD.DrawString(subheader, rampartOneSmall, brushTaro, subheaderX, subheaderY, drawFormat);

            var img = new Bitmap(image);
            return img;
        }
        private async Task<Image> FetchImageAsync(string url)
        {
            //new http client, then sends a get request to the url
            var client = new HttpClient();
            var response = await client.GetAsync(url);

            //if response fails, runs the default bg
            if (!response.IsSuccessStatusCode)
            {
                var backupResponse = await client.GetAsync("https://images.unsplash.com/photo-1528360983277-13d401cdc186?ixid=MnwxMjA3fDB8MHxzZWFyY2h8MjR8fGFuaW1lfGVufDB8MHwwfHw%3D&ixlib=rb-1.2.1&auto=format&fit=crop&w=400&q=60");
                var backupStream = await backupResponse.Content.ReadAsStreamAsync();
                return Image.FromStream(backupStream);
            }

            //if nothing goes wrong runs this
            var stream = await response.Content.ReadAsStreamAsync();
            return Image.FromStream(stream);
        }
    }
}
