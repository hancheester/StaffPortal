using System.Collections.Generic;
using System.Linq;

namespace StaffPortal.Data
{
    public interface IRepository<T>
    {
        int Create(T entity);
        T Return(object id);
        void Update(T entity);
        void Delete(T entity);
        void Delete(IEnumerable<T> entities);

        IList<T> GetAll();
        IQueryable<T> Table { get; }
        IQueryable<T> TableNoTracking { get; }
    }
}