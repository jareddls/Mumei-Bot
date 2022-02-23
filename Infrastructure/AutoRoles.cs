using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class AutoRoles
    {
        private readonly MumeiContext _context;

        //reads the roles
        public AutoRoles(MumeiContext context)
        {
            _context = context;
        }

        public async Task<List<AutoRole>> GetAutoRolesAsync(ulong id)
        {
            var autoRoles = await _context.AutoRoles
                .Where(x => x.ServerId == id)
                .ToListAsync();

            return await Task.FromResult(autoRoles);
        }

        //adding roles
        public async Task AddAutoRoleAsync(ulong id, ulong roleId)
        {
            var server = await _context.Servers
                //search all servers for id
                .FindAsync(id);

            //if server doesn't exist in database
            if (server == null)
                _context.Add(new Server { Id = id });

            _context.Add(new AutoRole { RoleId = roleId, ServerId = id });
            await _context.SaveChangesAsync();
        }


        //removing roles
        public async Task RemoveAutoRoleAsync(ulong id, ulong roleId)
        {
            var autoRole = await _context.AutoRoles
                .Where(x => x.RoleId == roleId)
                .FirstOrDefaultAsync();

            _context.Remove(autoRole);
            await _context.SaveChangesAsync();
        }

        //clear multiple roles
        public async Task ClearAutoRolesAsync(List<AutoRole> autoRoles)
        {
            _context.RemoveRange(autoRoles);
            await _context.SaveChangesAsync();
        }
    }
}
