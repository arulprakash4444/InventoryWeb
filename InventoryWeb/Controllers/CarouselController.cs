using Inventory.DataAccess.Repository;
using Inventory.DataAccess.Repository.IRepository;
using Inventory.Models;
using Inventory.Models.ViewModels;
using Inventory.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryWeb.Controllers
{
    [Authorize(Roles = SD.Role_Admin)]
    public class CarouselController : Controller
    {
        private readonly ICarouselRepository _carouselRepository;
        private readonly IWebHostEnvironment _env;

        public CarouselController(ICarouselRepository carouselRepository, IWebHostEnvironment env)
        {
            _carouselRepository = carouselRepository;
            _env = env;
        }

        public async Task<IActionResult> Edit()
        {
            var items = await _carouselRepository.GetAll();
            return View(items.OrderBy(x => x.Order));
        }







        private async Task<string> SaveImage(IFormFile file)
        {
            // 🔴 1️⃣ IMAGE TYPE VALIDATION — PUT THIS FIRST
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLower();

            if (!allowed.Contains(extension))
                throw new InvalidOperationException("Only JPG, PNG, WEBP images are allowed");

            // 🟡 2️⃣ ENSURE DIRECTORY EXISTS — PUT THIS NEXT
            string directoryPath = Path.Combine(
                _env.WebRootPath,
                "images",
                "carousel"
            );

            Directory.CreateDirectory(directoryPath);

            // 🟢 3️⃣ SAVE IMAGE
            string fileName = Guid.NewGuid() + extension;

            string path = Path.Combine(
                directoryPath,
                fileName
            );

            using var stream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(stream);

            return "/images/carousel/" + fileName;
        }




        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> SaveAll(CarouselSaveVM model)
        {
            if (model.Items == null || !model.Items.Any())
                return BadRequest("No carousel items submitted");

            // Get all existing items from DB
            var existingItems = (await _carouselRepository.GetAll()).ToDictionary(x => x.Id);

            foreach (var vm in model.Items)
            {
                // =========================================================
                // 1. DELETE ACTION (Soft Delete)
                // =========================================================
                if (vm.IsDeleted && vm.Id > 0)
                {
                    if (existingItems.TryGetValue(vm.Id, out var toDelete))
                    {
                        // Delete image from disk
                        if (!string.IsNullOrEmpty(toDelete.ImageUrl))
                        {
                            // Handle both / and \ just to be safe
                            var relativePath = toDelete.ImageUrl.TrimStart('/', '\\');
                            var path = Path.Combine(_env.WebRootPath, relativePath);

                            if (System.IO.File.Exists(path))
                                System.IO.File.Delete(path);
                        }

                        _carouselRepository.Remove(toDelete);
                    }
                    continue;
                }

                // =========================================================
                // 2. VALIDATION: New Item must have an Image
                // =========================================================
                if (vm.Id == 0 && vm.Image == null)
                {
                    return BadRequest("Image is required for new carousel items");
                }

                CarouselItem entity;

                // =========================================================
                // 3. EDIT EXISTING ITEM
                // =========================================================
                if (vm.Id > 0 && existingItems.TryGetValue(vm.Id, out entity))
                {
                    entity.Order = vm.Order;
                    entity.Link = vm.Link;
                    entity.Caption = vm.Caption;

                    // ➤ LOGIC TO REPLACE IMAGE
                    if (vm.Image != null)
                    {
                        // A. Delete the OLD image first
                        if (!string.IsNullOrEmpty(entity.ImageUrl))
                        {
                            var oldRelativePath = entity.ImageUrl.TrimStart('/', '\\');
                            var oldFullPath = Path.Combine(_env.WebRootPath, oldRelativePath);

                            if (System.IO.File.Exists(oldFullPath))
                            {
                                System.IO.File.Delete(oldFullPath);
                            }
                        }

                        // B. Save the NEW image
                        try
                        {
                            entity.ImageUrl = await SaveImage(vm.Image);
                        }
                        catch (Exception ex)
                        {
                            return BadRequest(ex.Message);
                        }
                    }
                }
                // =========================================================
                // 4. ADD NEW ITEM
                // =========================================================
                else
                {
                    string imageUrl = "";

                    if (vm.Image != null)
                    {
                        try
                        {
                            imageUrl = await SaveImage(vm.Image);
                        }
                        catch (Exception ex)
                        {
                            return BadRequest(ex.Message);
                        }
                    }

                    entity = new CarouselItem
                    {
                        Order = vm.Order,
                        Link = vm.Link,
                        Caption = vm.Caption,
                        ImageUrl = imageUrl
                    };

                    await _carouselRepository.Add(entity);
                }
            }

            await _carouselRepository.Save();
            return Ok();
        }


    }
}
