using Inventory.DataAccess.Data;
using Inventory.DataAccess.Repository.IRepository;
using Inventory.Models;
using Inventory.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inventory.DataAccess.Repository
{
    public class NotificationRepository : Repository<Notification>, INotificationRepository
    {

        private ApplicationDbContext _db;
        public NotificationRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Notification notification)
        {
            _db.Notifications.Update(notification);
        }

        public async Task Save()
        {
            await _db.SaveChangesAsync();
        }


        //public void AddOrReplace(Notification notification)
        //{
        //    // Check if a notification already exists for this ProductId
        //    var existing = _db.Notifications
        //        .FirstOrDefault(n => n.ProductId == notification.ProductId);

        //    if (existing != null)
        //    {
        //        _db.Notifications.Remove(existing);
        //    }

        //    _db.Notifications.Add(notification);
        //}

        public async Task DeleteByProductId(int productId)
        {
            Notification existing = await _db.Notifications
                                             .FirstOrDefaultAsync(n => n.ProductId == productId);

            if (existing != null)
            {
                _db.Notifications.Remove(existing);
                await _db.SaveChangesAsync();
            }
        }



        public async Task<int> GetUnreadNotificationCountAsync(string userId)
        {
            return await _db.Notifications
                .CountAsync(n =>
                    !_db.UserNotifications
                        .Any(un =>
                            un.NotificationId == n.Id &&
                            un.UserId == userId &&
                            un.IsDissmissed
                        )
                );
        }


        public async Task<List<UnreadNotification>> GetUnreadNotificationsAsync(string userId)
        {
            return await _db.Notifications
                .Include(n => n.Product)   // navigation property
                .Where(n => !_db.UserNotifications
                    .Any(un => un.UserId == userId
                            && un.NotificationId == n.Id
                            && un.IsDissmissed))
                .OrderByDescending(n => n.TimeCreated)
                .Select(n => new UnreadNotification
                {
                    NotificationId = n.Id,
                    ProductName = n.Product.Name,
                    TimeCreated = n.TimeCreated,
                    CurrentStock = n.Product.Stock
                })
                .ToListAsync();
        }




        public IQueryable<Notification> GetAllQueryable(string? includeProperties = null)
        {
            IQueryable<Notification> query = _db.Notifications;

            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (string includeProp in includeProperties
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }

            return query;
        }


    }
}
