using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using InventoryWeb.Validators;

namespace Inventory.Models.ViewModels
{
    public class ProductVM
    {
        public Product Product { get; set; }

        [ValidateNever]
        public IEnumerable<SelectListItem> CategoryList { get; set; }


        [ValidateImage(MaxSizeKB = 200, MinWidth = 400, MinHeight = 400, MaxWidth = 2000, MaxHeight = 2000)]
        public IFormFile? File { get; set; }

        // NEW → to show filename after validation failure
        //public string? SelectedFileName { get; set; }
    }
}
