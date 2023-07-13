using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using WebApplication1.DataAcces;
using WebApplication1.DataAccess.Repository.IRepository;

namespace WebApplication1.DataAccess.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _db;
        internal DbSet<T> dbSet;
        public Repository(ApplicationDbContext db)
        {
            _db = db;
            _db.products.Include(u => u.Catogery).Include(u => u.CoverType);
            this.dbSet=_db.Set<T>();
        }

        public void Add(T entity)
        {
            dbSet.Add(entity);
        }
        //includeProp - "Catogery,CoverType"
        public IEnumerable <T> GetAll(Expression<Func<T, bool>>? filter = null, string? includeProperties = null)
        {
            IQueryable<T> query = dbSet;
            //use if statement for filter null if do not use then send null and display error
            if (filter != null)
            {
                query = query.Where(filter);
            }
            if (includeProperties!= null)
            {
                foreach(var includeProp in includeProperties.Split (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)) 
                {
                    query = query.Include(includeProp);
                }
            }
            return query.ToList();
        }

        public T GetFirstOrDefault(Expression<Func<T, bool>> filter, string? includeProperties = null, bool tracked = true)
        {
            IQueryable<T> query;
            if(tracked)
            {
                query = dbSet;
            }
            else
            {
                //due to this we do not tracking entity anymore
                query = dbSet.AsNoTracking();
            }
            
            query=query.Where(filter);
            if (includeProperties != null)
            {
                foreach (var includeProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }
            return query.FirstOrDefault();
        }

        public void Remove(T entity)
        {
            dbSet.Remove(entity);
        }

        public void RemoveRange(IEnumerable<T> entity)
        {
            dbSet.RemoveRange(entity);
        }
    }
}
