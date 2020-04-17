using Microsoft.EntityFrameworkCore;
using StaffPortal.Common;
using System.Collections.Generic;
using System.Linq;

namespace StaffPortal.Data
{
    public class Repository<T> : IRepository<T> where T : BaseEntity, new()
    {
        protected ApplicationDbContext _context;
        private DbSet<T> _entities;

        protected DbSet<T> Entities
        {
            get
            {
                if (_entities == null) _entities = _context.Set<T>();
                return _entities;
            }
        }

        public virtual IQueryable<T> Table
        {
            get { return Entities; }
        }

        public virtual IQueryable<T> TableNoTracking
        {
            get { return Entities.AsNoTracking(); }
        }

        public Repository(ApplicationDbContext context)
        {
            _context = context;
        }

        public T Return(object id)
        {
            return Entities.Find(id);
        }

        public int Create(T entity)
        {
            _context.Set<T>().Add(entity);
            _context.SaveChanges();
            
            return entity.Id;
        }

        public void Update(T entity)
        {
            _context.Entry<T>(entity).State = EntityState.Modified;
            _context.SaveChanges();
        }

        public void Delete(T entity)
        {
            _context.Set<T>().Remove(entity);
            _context.SaveChanges();
        }

        public void Delete(IEnumerable<T> entities)
        {
            _context.Set<T>().RemoveRange(entities);
            _context.SaveChanges();
        }

        public IList<T> GetAll()
        {
            //var all = _context.Set<T>().ToList<T>();
            var all = _context.Set<T>().ToList<T>();
            OnGetAll(all);

            return all;
        }

        public virtual void OnGetAll(IList<T> model)
        { }       
    }
}