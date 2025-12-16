using Inventory.DataAccess.Repository.IRepository;
using Inventory.Models;
using Inventory.Models.ViewModels;
using Inventory.Models.ViewModels;
using Inventory.Models.ViewModels.InventoryWeb.Models;
using Inventory.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;



namespace InventoryWeb.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly INotificationRepository _notificationRepository;

        public ProductController(IProductRepository db, ICategoryRepository db1, INotificationRepository db2, IWebHostEnvironment webHostEnvironment)
        {
            _productRepository = db;
            _categoryRepository = db1;
            _notificationRepository = db2;
            _webHostEnvironment = webHostEnvironment;
        }


        //public IActionResult Index()
        //{
        //    List<Product> productList = _productRepository.GetAll(includeProperties: "Category").ToList();
        //    return View(productList);
        //}


        //[Authorize]
        //public async Task<IActionResult> Index(string? search, string? order)
        //{
        //    bool desc = order == "desc";

        //    var query = _productRepository.SearchStartsWithSorted<Product>(
        //        searchTerm: search,
        //        sortBy: "Name",
        //        descending: desc,
        //        includeProperties: "Category"
        //    );

        //    // Execute SQL asynchronously
        //    var products = await query.ToListAsync();

        //    return View(products);
        //}



        [Authorize]
        public async Task<IActionResult> Index(
            string? search,
            string? order,
            int page = 1,
            int pageSize = 5
)
        {
            bool desc = order == "desc";

            PaginationResult<Product> result =
                await _productRepository.SearchStartsWithSorted<Product>(
                    searchTerm: search,
                    sortBy: "Name",
                    descending: desc,
                    includeProperties: "Category",
                    page: page,
                    pageSize: pageSize
                );

            return View(result);
        }






        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> Add()
        {
            var categories = await _categoryRepository.GetAll();

            IEnumerable<SelectListItem> CategoryList = categories.Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.Id.ToString()
            });

            ProductVM productVM = new ProductVM()
            {
                CategoryList = CategoryList,
                Product = new Product()
            };

            return View(productVM);
        }




        //-------------------------------------------------ADD POST--------------------------------------------
        //[Authorize(Roles = SD.Role_Admin)]
        //[HttpPost]
        //public async Task<IActionResult> Add(ProductVM productVM, IFormFile? file)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        IEnumerable<Category> categories = await _categoryRepository.GetAll();

        //        IEnumerable<SelectListItem> categoryList =
        //            categories.Select(i => new SelectListItem
        //            {
        //                Text = i.Name,
        //                Value = i.Id.ToString()
        //            });

        //        productVM.CategoryList = categoryList;

        //        return View(productVM);
        //    }

        //    string wwwRootPath = _webHostEnvironment.WebRootPath;

        //    // ----------------------- FILE UPLOAD -----------------------
        //    if (file != null)
        //    {
        //        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
        //        string productPath = Path.Combine(wwwRootPath, @"images/product");

        //        if (!Directory.Exists(productPath))
        //        {
        //            Directory.CreateDirectory(productPath);
        //        }

        //        FileStream fileStream = new FileStream(
        //            Path.Combine(productPath, fileName),
        //            FileMode.Create
        //        );

        //        await file.CopyToAsync(fileStream);
        //        fileStream.Close();

        //        productVM.Product.ImageUrl = "/images/product/" + fileName;
        //    }

        //    // ----------------------- SAVE PRODUCT -----------------------
        //    await _productRepository.Add(productVM.Product);
        //    await _productRepository.Save();

        //    return RedirectToAction("Index");
        //}


        //---------------------------ADD POST HELPER-----------------------------------------------
        private static readonly Dictionary<string, List<byte[]>> ImageSignatures =
        new Dictionary<string, List<byte[]>>
        {
        {
            ".jpg",
            new List<byte[]>
            {
                new byte[] { 0xFF, 0xD8, 0xFF }
            }
        },
        {
            ".jpeg",
            new List<byte[]>
            {
                new byte[] { 0xFF, 0xD8, 0xFF }
            }
        },
        {
            ".png",
            new List<byte[]>
            {
                new byte[] { 0x89, 0x50, 0x4E, 0x47 }
            }
        },
        {
            ".webp",
            new List<byte[]>
            {
                new byte[] { 0x52, 0x49, 0x46, 0x46 } // RIFF
            }
        }
        };


        private bool IsValidImageSignature(IFormFile file)
        {
            if (file == null || file.Length < 8)
            {
                return false;
            }

            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!ImageSignatures.ContainsKey(extension))
            {
                return false;
            }

            byte[] headerBytes = new byte[8];

            using (Stream stream = file.OpenReadStream())
            using (BinaryReader reader = new BinaryReader(stream))
            {
                headerBytes = reader.ReadBytes(8);
            }

            List<byte[]> validSignatures = ImageSignatures[extension];

            foreach (byte[] signature in validSignatures)
            {
                bool matches = true;

                for (int i = 0; i < signature.Length; i++)
                {
                    if (headerBytes[i] != signature[i])
                    {
                        matches = false;
                        break;
                    }
                }

                if (matches)
                {
                    return true;
                }
            }

            return false;
        }




        private async Task PopulateCategoryList(ProductVM vm)
        {
            vm.CategoryList = (await _categoryRepository.GetAll())
                .Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                });
        }



        [Authorize(Roles = SD.Role_Admin)]
        [HttpPost]
        public async Task<IActionResult> Add(ProductVM productVM)
        {
            // Model-level validation (includes ValidateImage)
            if (!ModelState.IsValid)
            {
                await PopulateCategoryList(productVM);
                return View(productVM);
            }

            IFormFile file = productVM.File;

            // ----------------------- IMAGE PROCESSING -----------------------
            if (file != null)
            {
                // 🔐 Extra security check (signature)
                if (!IsValidImageSignature(file))
                {
                    ModelState.AddModelError("File", "Invalid or tampered image file.");
                    await PopulateCategoryList(productVM);
                    return View(productVM);
                }

                Image image;
                try
                {
                    image = Image.Load(file.OpenReadStream());
                }
                catch
                {
                    ModelState.AddModelError("File", "Image is corrupted or unreadable.");
                    await PopulateCategoryList(productVM);
                    return View(productVM);
                }

                // Remove metadata
                image.Metadata.ExifProfile = null;

                string extension = Path.GetExtension(file.FileName).ToLowerInvariant();

                IImageEncoder encoder = extension switch
                {
                    ".jpg" or ".jpeg" => new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder(),
                    ".png" => new SixLabors.ImageSharp.Formats.Png.PngEncoder(),
                    ".webp" => new SixLabors.ImageSharp.Formats.Webp.WebpEncoder(),
                    _ => null
                };

                // Safety fallback (should never happen due to annotation)
                if (encoder == null)
                {
                    ModelState.AddModelError("File", "Unsupported image format.");
                    await PopulateCategoryList(productVM);
                    return View(productVM);
                }

                string wwwRootPath = _webHostEnvironment.WebRootPath;
                string fileName = Guid.NewGuid() + extension;
                string productPath = Path.Combine(wwwRootPath, "images/product");

                if (!Directory.Exists(productPath))
                    Directory.CreateDirectory(productPath);

                using (FileStream outStream =
                       new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                {
                    image.Save(outStream, encoder);
                }

                productVM.Product.ImageUrl = "/images/product/" + fileName;
            }

            // ----------------------- SAVE PRODUCT -----------------------
            await _productRepository.Add(productVM.Product);
            await _productRepository.Save();

            return RedirectToAction("Index");
        }







        //--------------------------------------------------EDIT-----------------------------------------------------------
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            Product? productFromDb = await _productRepository.Get(i => i.Id == id);

            if (productFromDb == null)
            {
                return NotFound();
            }

            IEnumerable<Category> categories = await _categoryRepository.GetAll();

            IEnumerable<SelectListItem> categoryList =
                categories.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                });

            ProductVM productVM = new ProductVM
            {
                CategoryList = categoryList,
                Product = productFromDb
            };

            return View(productVM);
        }







        [Authorize(Roles = SD.Role_Admin)]
        [HttpPost]
        public async Task<IActionResult> Edit(ProductVM productVM, string DeleteImage)
        {

            bool deleteImage = DeleteImage?.ToLower() == "true";

            // 1️⃣ Annotation validation (includes ValidateImage)
            if (!ModelState.IsValid)
            {
                await PopulateCategoryList(productVM);
                return View(productVM);
            }

            // 2️⃣ Fetch existing product
            Product existingProduct = await _productRepository.Get(p => p.Id == productVM.Product.Id);
            if (existingProduct == null)
                return NotFound();

            IFormFile file = productVM.File;
            string wwwRootPath = _webHostEnvironment.WebRootPath;
            string productPath = Path.Combine(wwwRootPath, "images/product");

            if (!Directory.Exists(productPath))
                Directory.CreateDirectory(productPath);

            // ------------------ HANDLE DELETE IMAGE ------------------
            if (deleteImage && !string.IsNullOrEmpty(existingProduct.ImageUrl))
            {
                string oldImagePath = Path.Combine(wwwRootPath, existingProduct.ImageUrl.TrimStart('/', '\\'));
                if (System.IO.File.Exists(oldImagePath))
                    System.IO.File.Delete(oldImagePath);

                existingProduct.ImageUrl = null; // mark as deleted
            }

            // ------------------ HANDLE NEW IMAGE UPLOAD ------------------
            if (file != null)
            {
                // 🔐 Security: check file signature
                if (!IsValidImageSignature(file))
                {
                    ModelState.AddModelError("File", "Invalid or tampered image file.");
                    await PopulateCategoryList(productVM);
                    return View(productVM);
                }

                Image image;
                try
                {
                    image = Image.Load(file.OpenReadStream());
                }
                catch
                {
                    ModelState.AddModelError("File", "Image is corrupted or unreadable.");
                    await PopulateCategoryList(productVM);
                    return View(productVM);
                }

                // Remove metadata
                image.Metadata.ExifProfile = null;

                string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                IImageEncoder encoder = extension switch
                {
                    ".jpg" or ".jpeg" => new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder(),
                    ".png" => new SixLabors.ImageSharp.Formats.Png.PngEncoder(),
                    ".webp" => new SixLabors.ImageSharp.Formats.Webp.WebpEncoder(),
                    _ => null
                };

                if (encoder == null)
                {
                    ModelState.AddModelError("File", "Unsupported image format.");
                    await PopulateCategoryList(productVM);
                    return View(productVM);
                }

                string newFileName = Guid.NewGuid() + extension;
                string newImagePath = Path.Combine(productPath, newFileName);

                // Save new image
                using (var outStream = new FileStream(newImagePath, FileMode.Create))
                {
                    image.Save(outStream, encoder);
                }

                // Delete old image if it exists (and wasn't already deleted above)
                if (!string.IsNullOrEmpty(existingProduct.ImageUrl))
                {
                    string oldImagePath = Path.Combine(wwwRootPath, existingProduct.ImageUrl.TrimStart('/', '\\'));
                    if (System.IO.File.Exists(oldImagePath))
                        System.IO.File.Delete(oldImagePath);
                }

                // Assign new image URL
                existingProduct.ImageUrl = "/images/product/" + newFileName;
            }

            // ------------------ UPDATE PRODUCT FIELDS ------------------
            existingProduct.Name = productVM.Product.Name;
            existingProduct.Price = productVM.Product.Price;
            existingProduct.Description = productVM.Product.Description;
            existingProduct.CategoryId = productVM.Product.CategoryId;

            _productRepository.Update(existingProduct);
            await _productRepository.Save();

            return RedirectToAction("Index");
        }











        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            Product? productFromDb = await _productRepository.Get(
                i => i.Id == id,
                includeProperties: "Category"
            );

            if (productFromDb == null)
            {
                return NotFound();
            }

            return View(productFromDb);
        }





        [Authorize(Roles = SD.Role_Admin)]
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeletePOST(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            Product? productFromDb = await _productRepository.Get(i => i.Id == id);

            if (productFromDb == null)
            {
                return NotFound();
            }

            // ----------------------- DELETE IMAGE -----------------------
            string? imagePath = null;

            if (!string.IsNullOrEmpty(productFromDb.ImageUrl))
            {
                imagePath = Path.Combine(
                    _webHostEnvironment.WebRootPath,
                    productFromDb.ImageUrl.TrimStart('\\', '/')
                );
            }

            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }

            // ----------------------- REMOVE PRODUCT -----------------------
            _productRepository.Remove(productFromDb);
            await _productRepository.Save();

            return RedirectToAction("Index");
        }











        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UpdateStock([FromBody] UpdateStockRequest request)
        {
            // --- Basic validation ---
            if (request.Action != "ADD" && request.Action != "REMOVE")
            {
                ModelState.AddModelError(nameof(request.Action), "Action must be either ADD or REMOVE");
            }

            if (request.Quantity <= 0)
            {
                ModelState.AddModelError(nameof(request.Quantity), "Quantity must be greater than zero");
            }


            // --- Fetch product ---
            Product? product = await _productRepository.Get(p => p.Id == request.ProductId);
            if (product == null)
            {
                ModelState.AddModelError(nameof(request.ProductId), $"Product with ID {request.ProductId} does not exist");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            request.CurrentStock = product.Stock;

            // --- Validate REMOVE action ---
            if (request.Action == "REMOVE" && request.Quantity > product.Stock)
            {
                ModelState.AddModelError(nameof(request.Quantity), $"Quantity cannot exceed current stock ({product.Stock})");
                return BadRequest(ModelState);
            }

            // ----------------------- UPDATE STOCK -----------------------
            if (request.Action == "ADD")
            {
                product.Stock += request.Quantity;
                product.LastAdded = DateTime.UtcNow;
            }
            else if (request.Action == "REMOVE")
            {
                product.Stock -= request.Quantity;
                product.LastRemoved = DateTime.UtcNow;
            }

            // ----------------------- NOTIFICATION -----------------------
            if (product.Stock < 75)
            {
                Notification notification = new Notification
                {
                    ProductId = product.Id,
                    TimeCreated = DateTime.UtcNow
                };

                await _notificationRepository.Add(notification);
                await _notificationRepository.Save();
            }
            else
            {
                // STOCK OK -> REMOVE ANY NOTIFICATION FOR THIS PRODUCT
                await _notificationRepository.DeleteByProductId(product.Id);
            }

            // ----------------------- SAVE PRODUCT -----------------------
            await _productRepository.Save();

            return Ok(new
            {
                message = "Stock updated",
                updatedStock = product.Stock
            });
        }



    }
}
