using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inventory.Models
{
    public class UserNotification
    {
        [Key]
        public int Id { get; set; }


        public Boolean IsDissmissed { get; set; }

        [Required]
        public int NotificationId { get; set; }

        [ForeignKey("NotificationId")]
        [ValidateNever]
        public Notification? Notification { get; set; } // Navigation property


        [Required]
        public required string UserId { get; set; }

        [ForeignKey("UserId")]
        [ValidateNever]
        public  ApplicationUser? User { get; set; } // Navigation property


        [Required]
        [DataType(DataType.Date)]
        public DateTime TimeCreated { get; set; }
    }
}
