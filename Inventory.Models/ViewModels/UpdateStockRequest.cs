using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inventory.Models.ViewModels
{
    namespace InventoryWeb.Models
    {
        public class UpdateStockRequest
        {
            public int ProductId { get; set; }
            public string Action { get; set; } = string.Empty; // "ADD" or "REMOVE"
            public uint Quantity { get; set; }
            public uint CurrentStock { get; set; }
        }
    }
}
