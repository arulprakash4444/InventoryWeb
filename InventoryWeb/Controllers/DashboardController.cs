using Inventory.DataAccess.Repository.IRepository;
using Inventory.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryWeb.Controllers
{

    [Authorize(Roles = SD.Role_Admin)]
    public class DashboardController : Controller
    {

        private readonly IRestaurantOrderRepository _restaurantOrderRepository;

        // Inject the specific repository
        public DashboardController(IRestaurantOrderRepository restaurantOrderRepository)
        {
            _restaurantOrderRepository = restaurantOrderRepository;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetStockTrends()
        {
            var data = await _restaurantOrderRepository.GetOrdersGroupedByMonthAsync();
            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetVolumeAnalysis()
        {
            var data = await _restaurantOrderRepository.GetQuantityGroupedByMonthAsync();
            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetPortfolioDistribution()
        {
            var data = await _restaurantOrderRepository.GetCategoryDistributionAsync();
            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetActivityHeatmap()
        {
            var data = await _restaurantOrderRepository.GetHeatmapDataAsync();
            return Json(data);
        }
    }
}
