using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;



namespace Inventory.Models
{
    public class Product
    {

        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Product Name is required.")]
        [StringLength(50, ErrorMessage = "Name must be between 1 and 50 characters long.")]
        public string? Name { get; set; }

        [Range(0, 500, ErrorMessage = "Stock must be between 0 and 500.")]
        public uint Stock { get; set; }


        // Price of type (x.xx only
        [Required(ErrorMessage = "Price is required.")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 9999999999999999.99, ErrorMessage = "Price must be a positive value.")]
        [RegularExpression(@"^\d+(\.\d{2})?$", ErrorMessage = "Price must be an integer or a decimal with exactly two digits after the decimal.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(250, ErrorMessage = "Description must be 1 to 250 characters long.")]
        public string? Description { get; set; }

        [DataType(DataType.Date)]
        public DateTime? LastAdded { get; set; }

        [DataType(DataType.Date)]
        public DateTime? LastRemoved { get; set; }

        [ValidateNever]
        public string? ImageUrl { get; set; }


        // ---------- CATEGORY RELATIONSHIP ----------
        [Required(ErrorMessage = "Product Category is required.")]
        public int? CategoryId { get; set; }   // Foreign key

        [ForeignKey("CategoryId")]
        [ValidateNever]
        public Category? Category { get; set; } // Navigation property

    }
}
