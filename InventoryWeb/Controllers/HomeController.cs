using Inventory.DataAccess.Repository.IRepository;
using Inventory.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace InventoryWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ICarouselRepository _carouselRepository;

        public HomeController(ILogger<HomeController> logger, ICarouselRepository carouselRepository)
        {
            _logger = logger;
            _carouselRepository = carouselRepository;
        }

        public async Task<IActionResult> Index()
        {
            var carouselItems = await _carouselRepository.GetActiveOrdered();
            return View(carouselItems);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult BlockedTab()
        {
            return View();
        }
    }
}
