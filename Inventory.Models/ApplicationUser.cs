using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inventory.Models
{
    public class ApplicationUser : IdentityUser
    {
        public String? Address { get; set; }

        public String? Name { get; set; }

        public string? CurrentSessionId { get; set; }

        public DateTime? SessionExpiresAt { get; set; }
    }
}
