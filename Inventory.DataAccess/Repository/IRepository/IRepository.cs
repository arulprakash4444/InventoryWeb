using Inventory.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Inventory.DataAccess.Repository.IRepository
{
    public interface IRepository<T> where T : class
    {


        Task Add(T entity);

        Task<T?> Get(Expression<Func<T, bool>> filter, string? includeProperties = null);


        Task<IEnumerable<T>> GetAll(string? includeProperties = null);


        void Remove(T entity);

        //void RemoveRange(IEnumerable<T> entity);

        //public IQueryable<T> SearchStartsWithSorted<T>(
        //    string? searchTerm,
        //    string sortBy = "Name",
        //    bool descending = false,
        //    string? includeProperties = null
        //) where T : class;


        Task<PaginationResult<T>> SearchStartsWithSorted<T>(
    string? searchTerm,
    string sortBy = "Name",
    bool descending = false,
    string? includeProperties = null,
    int page = 1,
    int pageSize = 10
        ) where T : class;


    }
}
