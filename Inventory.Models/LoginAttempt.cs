using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inventory.Models
{
    public class LoginAttempt
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(256, ErrorMessage = "Username cannot exceed 256 characters.")]
        public required string Username { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public required DateTime AttemptedAt { get; set; }

        [Required]
        public required bool IsSuccessful { get; set; }
    }
}
