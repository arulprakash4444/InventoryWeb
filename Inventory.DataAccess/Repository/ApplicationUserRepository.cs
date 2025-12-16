using Inventory.DataAccess.Data;
using Inventory.DataAccess.Repository.IRepository;
using Inventory.Models;
using Inventory.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inventory.DataAccess.Repository
{
    public class ApplicationUserRepository : Repository<ApplicationUser>, IApplicationUserRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public ApplicationUserRepository(UserManager<ApplicationUser> userManager, ApplicationDbContext db) : base(db)
        {
            _userManager = userManager;
            _db = db;
        }

        public void Update(ApplicationUser user)
        {
            _db.ApplicationUsers.Update(user);
        }




    public async Task<List<ApplicationUserWithRole>> GetAllUsersWithRolesAsync()
        {
            IQueryable<ApplicationUserWithRole> usersWithRoles =
                from ApplicationUser user in _db.ApplicationUsers
                join IdentityUserRole<string> userRole in _db.UserRoles
                    on user.Id equals userRole.UserId into ur
                from userRole in ur.DefaultIfEmpty()
                join IdentityRole role in _db.Roles
                    on userRole.RoleId equals role.Id into r
                from role in r.DefaultIfEmpty()
                select new ApplicationUserWithRole
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    Name = user.Name != null ? user.Name : "No Name",
                    PhoneNumber = user.PhoneNumber != null ? user.PhoneNumber : "No PhoneNumber",
                    Address = user.Address,
                    Role = role != null ? role.Name : "No Role"
                };

            List<ApplicationUserWithRole> result = await usersWithRoles.ToListAsync();
            return result;
        }





    public async Task<ApplicationUser?> GetUserAsync(string id)
        {
            return await _userManager.FindByIdAsync(id);
        }

        public async Task UpdateUserAsync(ApplicationUser user)
        {
            await _userManager.UpdateAsync(user);
        }

        public async Task DeleteUserAsync(ApplicationUser user)
        {
            await _userManager.DeleteAsync(user);
        }
    }
}
