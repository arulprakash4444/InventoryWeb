using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inventory.Models.ViewModels
{
    public class CarouselItemVM
    {
        public int Id { get; set; }
        public int Order { get; set; }

        public string? Link { get; set; }
        public string? Caption { get; set; }

        public IFormFile? Image { get; set; }

        public bool IsDeleted { get; set; }
    }
}
