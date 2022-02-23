using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class Ranks
    {
        private readonly MumeiContext _context;
        
        //reads the ranks
        public Ranks(MumeiContext context)
        {
            _context = context;
        }

        public async Task<List<Rank>> GetRanksAsync(ulong id)
        {
            var ranks = await _context.Ranks
                .Where(x => x.ServerId == id)
                .ToListAsync();

            return await Task.FromResult(ranks);
        }

        //adding ranks
        public async Task AddRankAsync(ulong id, ulong roleId)
        {
            var server = await _context.Servers
                //search all servers for id
                .FindAsync(id);

            //if server doesn't exist in database
            if (server == null)
                _context.Add(new Server { Id = id });

            _context.Add(new Rank { RoleId = roleId, ServerId = id });
            await _context.SaveChangesAsync();
        }


        //removing ranks by reading through the rank database table
        public async Task RemoveRankAsync(ulong id, ulong roleId)
        {
            var rank = await _context.Ranks
                .Where(x => x.RoleId == roleId)
                .FirstOrDefaultAsync();

            _context.Remove(rank);
            await _context.SaveChangesAsync();
        }

        //clear multiple ranks
        public async Task ClearRanksAsync(List<Rank> ranks)
        {
            _context.RemoveRange(ranks);
            await _context.SaveChangesAsync();
        }
    }
}
