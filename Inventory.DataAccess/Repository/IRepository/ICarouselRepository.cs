using Inventory.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inventory.DataAccess.Repository.IRepository
{
    public interface ICarouselRepository : IRepository<CarouselItem>
    {
        Task<IEnumerable<CarouselItem>> GetAllNoTracking();
        Task<IEnumerable<CarouselItem>> GetActiveOrdered();
        Task UpdateRangeAsync(IEnumerable<CarouselItem> items);

        Task Save();
    }
}
