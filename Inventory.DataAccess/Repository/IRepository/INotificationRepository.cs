using Inventory.Models;
using Inventory.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inventory.DataAccess.Repository.IRepository
{
    public interface INotificationRepository : IRepository<Notification>
    {
        void Update(Notification notification);
        Task Save();

        //void AddOrReplace(Notification notification);


        Task DeleteByProductId(int productId);

        Task<int> GetUnreadNotificationCountAsync(string userId);

        Task<List<UnreadNotification>> GetUnreadNotificationsAsync(string userId);


        IQueryable<Notification> GetAllQueryable(string? includeProperties = null);
    }
}
