using Inventory.Models;
using Inventory.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inventory.DataAccess.Repository.IRepository
{
    public interface IApplicationUserRepository : IRepository<ApplicationUser>
    {
        void Update(ApplicationUser user);

        Task<List<ApplicationUserWithRole>> GetAllUsersWithRolesAsync();

        Task<ApplicationUser?> GetUserAsync(string id);


        Task UpdateUserAsync(ApplicationUser user);

        Task DeleteUserAsync(ApplicationUser user);
    }
}
