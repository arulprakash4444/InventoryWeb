using Inventory.DataAccess.Data;
using Inventory.DataAccess.Repository.IRepository;
using Inventory.Models;
using Microsoft.EntityFrameworkCore;


namespace Inventory.DataAccess.Repository
{
    public class LoginAttemptRepository : Repository<LoginAttempt>, ILoginAttemptRepository
    {
        private ApplicationDbContext _db;
        public LoginAttemptRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }


        public async Task<IEnumerable<LoginAttempt>> GetLast10AttemptsAsync(string username)
        {
            return await _db.LoginAttempts
                .Where(a => a.Username == username)
                .OrderByDescending(a => a.AttemptedAt)
                .Take(10)
                .ToListAsync();
        }

        public void Update(LoginAttempt loginAttempt)
        {
            _db.LoginAttempts.Update(loginAttempt);
        }

        public async Task Save()
        {
            await _db.SaveChangesAsync();
        }

    }

}
