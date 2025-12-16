using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inventory.Models
{
    public class CarouselItem
    {
        public int Id { get; set; }

        [Required]
        public string ImageUrl { get; set; } = string.Empty;

        public int Order { get; set; }

        public string? Link { get; set; }

        public string? Caption { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
