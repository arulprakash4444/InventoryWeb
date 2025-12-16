using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inventory.Models.ViewModels
{
    public class UnreadNotification
    {
        public int NotificationId { get; set; }
        public string ProductName { get; set; }
        public DateTime TimeCreated { get; set; }
        public uint CurrentStock { get; set; }
    }
}
