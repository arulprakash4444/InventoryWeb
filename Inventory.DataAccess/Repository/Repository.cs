using Inventory.DataAccess.Data;
using Inventory.DataAccess.Repository.IRepository;
using Inventory.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Inventory.DataAccess.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _db;

        internal DbSet<T> dbSet;
        public Repository(ApplicationDbContext db)
        {
            _db = db;
            this.dbSet = _db.Set<T>(); //Setting to a generic class
            // _db.Products.Include(u => u.Category).include(u => u.OtherFieldFromCategory); populate category in products
            //_db.Products.Include(u => u.Category).Include(u => u.CategoryId);
        }

        public async Task Add(T entity)
        {
            await dbSet.AddAsync(entity);
        }

        public async Task<T?> Get(Expression<Func<T, bool>> filter, string? includeProperties = null)
        {
            IQueryable<T> query = dbSet;
            query = query.Where(filter);

            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var includeProp in includeProperties
                    .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }

            return await query.FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<T>> GetAll(string? includeProperties = null)
        {
            IQueryable<T> query = dbSet;

            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var includeProp in includeProperties
                    .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }

            return await query.ToListAsync();
        }

        public void Remove(T entity)
        {
            dbSet.Remove(entity);
        }

        //public void RemoveRange(IEnumerable<T> entity)
        //{
        //    dbSet.RemoveRange(entity);
        //}



        //public IQueryable<T> SearchStartsWithSorted<T>(
        //    string? searchTerm,
        //    string sortBy = "Name",
        //    bool descending = false,
        //    string? includeProperties = null
        //) where T : class
        //{
        //    IQueryable<T> query = (IQueryable<T>)dbSet;



        //    //// Include navigation properties
        //    //if (!string.IsNullOrEmpty(includeProperties))
        //    //{
        //    //    foreach (var includeProp in includeProperties
        //    //        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
        //    //    {
        //    //        query = query.Include(includeProp);
        //    //    }
        //    //}

        //    // Case-insensitive search
        //    if (!string.IsNullOrWhiteSpace(searchTerm))
        //    {
        //        query = query.Where(e =>
        //            EF.Functions.ILike(
        //                EF.Property<string>(e, sortBy),
        //                searchTerm + "%"
        //            )
        //        );
        //    }



        //    // Include navigation properties
        //    if (!string.IsNullOrEmpty(includeProperties))
        //    {
        //        foreach (var includeProp in includeProperties
        //            .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
        //        {
        //            query = query.Include(includeProp);
        //        }
        //    }


        //    // if searchTerm is present or not sort

        //    // Sorting
        //    query = descending
        //        ? query.OrderByDescending(e => EF.Property<object>(e, sortBy))
        //        : query.OrderBy(e => EF.Property<object>(e, sortBy));

        //    return query; //  no ToList(), deferred execution
        //}




    public async Task<PaginationResult<T>> SearchStartsWithSorted<T>(

    string? searchTerm,
    string sortBy = "Name",
    bool descending = false,
    string? includeProperties = null,
    int page = 1,
    int pageSize = 10
        ) where T : class
        {
            IQueryable<T> query = (IQueryable<T>)dbSet;

            // Filtering (case-insensitive)
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(e =>
                    EF.Functions.ILike(
                        EF.Property<string>(e, sortBy),
                        searchTerm + "%"
                    )
                );
            }

            // Include related entities
            if (!string.IsNullOrEmpty(includeProperties))
            {
                string[] includes = includeProperties
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string include in includes)
                {
                    query = query.Include(include);
                }
            }

            // Sorting
            query = descending
                ? query.OrderByDescending(e => EF.Property<object>(e, sortBy))
                : query.OrderBy(e => EF.Property<object>(e, sortBy));

            // Count before pagination
            int totalItems = await query.CountAsync();

            // Calculate total pages
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            // Fix page bounds
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            // Apply pagination
            List<T> items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Return pagination result
            PaginationResult<T> result = new PaginationResult<T>
            {
                Items = items,
                CurrentPage = page,
                TotalPages = totalPages,
                PageSize = pageSize,
                TotalItems = totalItems
            };

            return result;
        }




    }
}
