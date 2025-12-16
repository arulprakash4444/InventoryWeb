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
    public class RestaurantOrderRepository : Repository<RestaurantOrder>, IRestaurantOrderRepository
    {
        private ApplicationDbContext _db;
        public RestaurantOrderRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        // 1. Line Chart Data (Sum of Amount)
        //public async Task<IEnumerable<object>> GetOrdersGroupedByMonthAsync()
        //{
        //    var data = await _db.RestaurantOrders
        //        .GroupBy(o => o.CreatedAt.Month)
        //        .Select(g => new { time = g.Key, value = g.Sum(o => o.Amount) })
        //        .OrderBy(x => x.time)
        //        .ToListAsync();

        //    // Fill gaps (1-12 months)
        //    return Enumerable.Range(1, 12).GroupJoin(data,
        //        m => m, d => d.time,
        //        (m, d) => d.FirstOrDefault() ?? new { time = m, value = 0m }
        //    );
        //}


        public async Task<IEnumerable<object>> GetOrdersGroupedByMonthAsync()
        {
            // 1. Normalize to LOCAL DATE first (critical)
            var data = await _db.RestaurantOrders
                .Select(o => new
                {
                    LocalDate = o.CreatedAt.ToLocalTime().Date,
                    o.Amount
                })
                .GroupBy(x => x.LocalDate.Month)
                .Select(g => new
                {
                    time = g.Key,               // month (1–12)
                    value = g.Sum(x => x.Amount)
                })
                .ToListAsync();

            // 2. Fill gaps for months 1–12
            var result = Enumerable.Range(1, 12)
                .Select(m =>
                {
                    var match = data.FirstOrDefault(d => d.time == m);
                    return new
                    {
                        time = m,
                        value = match?.value ?? 0m
                    };
                });

            return result;
        }



        // 2. Bar Chart Data (Sum of Quantity)
        //public async Task<IEnumerable<object>> GetQuantityGroupedByMonthAsync()
        //{
        //    var data = await _db.RestaurantOrders
        //        .GroupBy(o => o.CreatedAt.Month)
        //        .Select(g => new { time = g.Key, value = g.Sum(o => o.Quantity) })
        //        .OrderBy(x => x.time)
        //        .ToListAsync();

        //    return Enumerable.Range(1, 12).GroupJoin(data,
        //        m => m, d => d.time,
        //        (m, d) => d.FirstOrDefault() ?? new { time = m, value = 0 }
        //    );
        //}

        public async Task<IEnumerable<object>> GetQuantityGroupedByMonthAsync()
        {
            // 1. Normalize to LOCAL DATE first (critical)
            var data = await _db.RestaurantOrders
                .Select(o => new
                {
                    LocalDate = o.CreatedAt.ToLocalTime().Date,
                    o.Quantity
                })
                .GroupBy(x => x.LocalDate.Month)
                .Select(g => new
                {
                    time = g.Key,                // month (1–12)
                    value = g.Sum(x => x.Quantity)
                })
                .ToListAsync();

            // 2. Fill gaps for months 1–12
            var result = Enumerable.Range(1, 12)
                .Select(m =>
                {
                    var match = data.FirstOrDefault(d => d.time == m);
                    return new
                    {
                        time = m,
                        value = match?.value ?? 0
                    };
                });

            return result;
        }




        // 3. Pie Chart Data (Count by Category)
        //public async Task<IEnumerable<object>> GetCategoryDistributionAsync()
        //{
        //    return await _db.RestaurantOrders
        //        .GroupBy(o => o.Category)
        //        .Select(g => new { label = g.Key, value = g.Count() }) // Using Count() as requested
        //        .ToListAsync();
        //}

        public async Task<IEnumerable<object>> GetCategoryDistributionAsync(DateTime? from = null, DateTime? to = null)
        {
            var query = _db.RestaurantOrders.AsQueryable();

            if (from.HasValue)
                query = query.Where(o => o.CreatedAt >= from.Value.ToLocalTime());
            if (to.HasValue)
                query = query.Where(o => o.CreatedAt <= to.Value.ToLocalTime());

            return await query
                .GroupBy(o => o.Category ?? "Unknown")
                .Select(g => new { label = g.Key, value = g.Count() })
                .ToListAsync();
        }




        // 4. Heatmap Data
        //public async Task<IEnumerable<object>> GetHeatmapDataAsync()
        //{
        //    return await _db.RestaurantOrders
        //        .Select(o => new
        //        {
        //            Month = o.CreatedAt.Month,
        //            Week = (o.CreatedAt.Day - 1) / 7 + 1
        //        })
        //        .GroupBy(x => new { x.Month, x.Week })
        //        .Select(g => new
        //        {
        //            Month = g.Key.Month,
        //            Week = g.Key.Week,
        //            Count = g.Count()
        //        })
        //        .ToListAsync();
        //}


        // --- HEATMAP LOGIC ---
        public async Task<IEnumerable<object>> GetHeatmapDataAsync()
        {
            // 1. Normalize CreatedAt to LOCAL DATE FIRST (CRITICAL FIX)
            // This prevents UTC spillover into the wrong calendar day/week
            var dailyData = await _db.RestaurantOrders
                .Select(o => o.CreatedAt.ToLocalTime().Date) // <-- FIX
                .GroupBy(d => new
                {
                    d.Year,
                    d.Month,
                    d.Day
                })
                .Select(g => new
                {
                    Date = new DateTime(g.Key.Year, g.Key.Month, g.Key.Day),
                    Count = g.Count()
                })
                .ToListAsync();

            // 2. Aggregate daily → calendar week (Monday-start)
            var weeklyData = dailyData
                .Select(d => new
                {
                    d.Date.Year,   // internal only (not returned)
                    d.Date.Month,
                    Week = GetCalendarWeekOfMonthMondayStart(d.Date),
                    d.Count
                })
                .GroupBy(x => new { x.Year, x.Month, x.Week })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    g.Key.Week,
                    Count = g.Sum(x => x.Count)
                })
                .ToList();

            // 3. Determine how many calendar weeks exist per month
            var weeksPerMonth = dailyData
                .Select(d => new { d.Date.Year, d.Date.Month })
                .Distinct()
                .Select(x =>
                {
                    var lastDay = DateTime.DaysInMonth(x.Year, x.Month);
                    var lastDate = new DateTime(x.Year, x.Month, lastDay);

                    return new
                    {
                        x.Year,
                        x.Month,
                        MaxWeek = GetCalendarWeekOfMonthMondayStart(lastDate)
                    };
                })
                .ToList();

            // 4. Generate only existing weeks, fill missing with zero
            var result =
                from m in weeksPerMonth
                from week in Enumerable.Range(1, m.MaxWeek)
                join w in weeklyData
                    on new { m.Year, m.Month, week }
                    equals new { w.Year, w.Month, week = w.Week }
                    into gj
                from sub in gj.DefaultIfEmpty()
                orderby m.Month, week
                select new
                {
                    month = m.Month,
                    week = week,
                    count = sub?.Count ?? 0
                };

            return result;
        }



        // Helper: Calculates Week 1-6 (Monday Start, Sunday End)
        // Monday-start calendar week of month (1–6)
        private int GetCalendarWeekOfMonthMondayStart(DateTime date)
        {
            var firstDayOfMonth = new DateTime(date.Year, date.Month, 1);

            // Monday = 0, Sunday = 6
            int startOffset = ((int)firstDayOfMonth.DayOfWeek + 6) % 7;

            return (date.Day + startOffset - 1) / 7 + 1;
        }







        public void Update(Category category)
        {
            _db.Categories.Update(category);
        }

        public async Task Save()
        {
            await _db.SaveChangesAsync();
        }
    }
}
