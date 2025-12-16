using Inventory.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inventory.DataAccess.Repository.IRepository
{
    public interface IRestaurantOrderRepository : IRepository<RestaurantOrder>
    {
        // Custom methods specifically for the Dashboard
        Task<IEnumerable<object>> GetOrdersGroupedByMonthAsync();
        Task<IEnumerable<object>> GetQuantityGroupedByMonthAsync();
        Task<IEnumerable<object>> GetCategoryDistributionAsync(DateTime? from = null, DateTime? to = null);
        Task<IEnumerable<object>> GetHeatmapDataAsync();

        void Update(Category category);
        Task Save();
    }
}
