using Inventory.DataAccess.Data;
using Inventory.DataAccess.Repository.IRepository;
using Inventory.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inventory.DataAccess.Repository
{
    public class CarouselRepository : Repository<CarouselItem>, ICarouselRepository
    {
        private readonly ApplicationDbContext _db;

        public CarouselRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }


        public async Task<IEnumerable<CarouselItem>> GetAllNoTracking()
        {
            return await _db.CarouselItems
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<CarouselItem>> GetActiveOrdered()
        {
            return await _db.CarouselItems
                .Where(x => x.IsActive)
                .OrderBy(x => x.Order)
                .ToListAsync();
        }

        public async Task UpdateRangeAsync(IEnumerable<CarouselItem> items)
        {
            _db.CarouselItems.UpdateRange(items);
            await _db.SaveChangesAsync();
        }


        public async Task Save()
        {
            await _db.SaveChangesAsync();
        }
    }
}
