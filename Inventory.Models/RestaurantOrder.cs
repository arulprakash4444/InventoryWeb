using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inventory.Models
{
    [Table("RestaurantOrders")] // Maps to PostgreSQL table
    public class RestaurantOrder
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("category")]
        public string Category { get; set; }

        [Column("amount")]
        public decimal Amount { get; set; }

        [Column("quantity")]
        public int Quantity { get; set; }
    }
}
