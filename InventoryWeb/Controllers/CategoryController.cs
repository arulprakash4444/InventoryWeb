using Inventory.DataAccess.Data;
using Inventory.DataAccess.Repository.IRepository;
using Inventory.Models;
using Inventory.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryWeb.Controllers
{
    public class CategoryController : Controller
    {

        private readonly ICategoryRepository _categoryRepository;
        public CategoryController(ICategoryRepository db) {
            _categoryRepository = db;
        }


        [Authorize]
        public async Task<IActionResult> Index()
        {
            IEnumerable<Category> categoryList = await _categoryRepository.GetAll();
            return View(categoryList);
        }



        [Authorize(Roles =SD.Role_Admin)]
        public IActionResult Add()
        {
            return View();
        }


        [Authorize(Roles = SD.Role_Admin)]
        [HttpPost]
        public async Task<IActionResult> Add(Category category)
        {

            //custom errors
            //ModelState.AddModelError("name / can be empty", "The Error Message");


            if (!ModelState.IsValid)
            {
                return View(category);
            }

            await _categoryRepository.Add(category);
            await _categoryRepository.Save();

            return RedirectToAction("Index");

        }



        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> Edit(int? id)
            
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            Category? categoryFromDb = await _categoryRepository.Get(i => i.Id == id);
            //Category? categoryFromDb = _db.Categories.FirstOrDefault(u => u.Id == id);
            //Category? categoryFromDb = _db.Categories.Where(u => u.Id == id).FirstOrDefault();

            if (categoryFromDb == null)
            {
                return NotFound();
            }

            return View(categoryFromDb);
        }


        [Authorize(Roles = SD.Role_Admin)]
        [HttpPost]
        public async Task<IActionResult> Edit(Category category)
        {

            if (!ModelState.IsValid)
            {
                return View(category);
            }

            Category existingCategory = await _categoryRepository.Get(c => c.Id == category.Id);
            if (existingCategory == null)
            {
                return NotFound();
            }

            

            // Update only editable fields
            existingCategory.Name = category.Name;

            _categoryRepository.Update(existingCategory);
            await _categoryRepository.Save();

            return RedirectToAction("Index");
        }


        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var categoryFromDb = await _categoryRepository.Get(i => i.Id == id);

            if (categoryFromDb == null)
            {
                return NotFound();
            }

            return View(categoryFromDb);
        }


        [Authorize(Roles = SD.Role_Admin)]
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeletePOST(int? id)
        {
            Category? obj = await _categoryRepository.Get(i => i.Id == id);
            if (obj == null)
            {
                return NotFound();
            }
            _categoryRepository.Remove(obj);
            await _categoryRepository.Save();
            return RedirectToAction("Index");
        }
    }
}
