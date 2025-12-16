using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inventory.Models.ViewModels
{
    public class CarouselSaveVM
    {
        public List<CarouselItemVM> Items { get; set; } = new();
    }
}
