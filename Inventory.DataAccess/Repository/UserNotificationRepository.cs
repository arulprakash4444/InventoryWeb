using Inventory.DataAccess.Data;
using Inventory.DataAccess.Repository.IRepository;
using Inventory.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inventory.DataAccess.Repository
{
    public class UserNotificationRepository : Repository<UserNotification>, IUserNotificationRepository
    {
        private ApplicationDbContext _db;
        public UserNotificationRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(UserNotification userNotification)
        {
            _db.UserNotifications.Update(userNotification);
        }

        public async Task Save()
        {
           await _db.SaveChangesAsync();
        }


        public async Task DismissNotificationAsync(int notificationId, string userId)
        {
            UserNotification newRow = new UserNotification
            {
                NotificationId = notificationId,
                UserId = userId,
                IsDissmissed = true,
                TimeCreated = DateTime.UtcNow
            };

            await _db.UserNotifications.AddAsync(newRow);
            await _db.SaveChangesAsync();
        }


    }
}
