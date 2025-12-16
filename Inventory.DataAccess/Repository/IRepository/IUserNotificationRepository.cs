using Inventory.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inventory.DataAccess.Repository.IRepository
{
    public interface IUserNotificationRepository : IRepository<UserNotification>
    {
        void Update(UserNotification userNotification);
        Task Save();

        Task DismissNotificationAsync(int notificationId, string userId);
    }
}
