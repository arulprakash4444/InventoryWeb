using Inventory.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inventory.DataAccess.Repository.IRepository
{
    public interface ILoginAttemptRepository : IRepository<LoginAttempt>
    {
        Task<IEnumerable<LoginAttempt>> GetLast10AttemptsAsync(string username);

        void Update(LoginAttempt loginAttempt);
        Task Save();
    }
}
