using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using SixLabors.ImageSharp;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace InventoryWeb.Validators
{
    public class ValidateImageAttribute : ValidationAttribute
    {
        // ===== CONFIGURABLE PROPERTIES =====
        public int MaxSizeKB { get; set; }
        public int MinWidth { get; set; }
        public int MinHeight { get; set; }
        public int MaxWidth { get; set; }
        public int MaxHeight { get; set; }
        public string[] AllowedExtensions { get; set; }
            = new[] { ".jpg", ".jpeg", ".png", ".webp" };

        // ===== IMAGE SIGNATURES (MAGIC BYTES) =====
        private static readonly Dictionary<string, List<byte[]>> ImageSignatures =
            new Dictionary<string, List<byte[]>>()
            {
                { ".jpg",  new List<byte[]> { new byte[] { 0xFF, 0xD8, 0xFF } } },
                { ".jpeg", new List<byte[]> { new byte[] { 0xFF, 0xD8, 0xFF } } },
                { ".png",  new List<byte[]> { new byte[] { 0x89, 0x50, 0x4E, 0x47 } } },
                { ".webp", new List<byte[]> { new byte[] { 0x52, 0x49, 0x46, 0x46 } } } // RIFF
            };

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            IFormFile file = value as IFormFile;

            // File is optional
            if (file == null)
                return ValidationResult.Success;

            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            // 1️⃣ Extension check
            if (!AllowedExtensions.Contains(extension))
            {
                return new ValidationResult(
                    $"Only {string.Join(", ", AllowedExtensions)} images are allowed."
                );
            }

            // 2️⃣ File size check
            if (MaxSizeKB > 0 && file.Length > MaxSizeKB * 1024)
            {
                return new ValidationResult(
                    $"Image size must not exceed {MaxSizeKB} KB."
                );
            }

            // 3️⃣ Signature (magic bytes) check
            if (!IsValidImageSignature(file, extension))
            {
                return new ValidationResult(
                    "Invalid image file or file extension was changed."
                );
            }

            // 4️⃣ Corruption + dimension check
            try
            {
                using Image image = Image.Load(file.OpenReadStream());

                if (image.Width < MinWidth || image.Height < MinHeight ||
                    image.Width > MaxWidth || image.Height > MaxHeight)
                {
                    return new ValidationResult(
                        $"Image dimensions must be between " +
                        $"{MinWidth}x{MinHeight} and {MaxWidth}x{MaxHeight} pixels."
                    );
                }
            }
            catch
            {
                return new ValidationResult(
                    "Image is corrupted or unreadable."
                );
            }

            return ValidationResult.Success;
        }

        // ===== SIGNATURE VALIDATION =====
        private static bool IsValidImageSignature(IFormFile file, string extension)
        {
            if (!ImageSignatures.ContainsKey(extension))
                return false;

            using (BinaryReader reader = new BinaryReader(file.OpenReadStream()))
            {
                byte[] headerBytes = reader.ReadBytes(8);

                return ImageSignatures[extension].Any(signature =>
                    headerBytes.Take(signature.Length)
                               .SequenceEqual(signature)
                );
            }
        }
    }
}
