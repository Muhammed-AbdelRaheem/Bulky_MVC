using Bulky.DataAccess.Data;
using Bulky.DataAccess.IRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly AppDbContext _context;
        internal DbSet<T> _Dbset;
        public Repository(AppDbContext context)
        {
            _context = context;
            this._Dbset = _context.Set<T>();
            _context.Products.Include(u => u.Category).Include(u => u.CategoryId);
        }
        public IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter = null, string? include = null)
        {
            IQueryable<T> query = _Dbset;
            if (filter!=null)
            {
                query = query.Where(filter);
            }
            if (!string.IsNullOrEmpty(include))
            {
                foreach (var includeProp in include
                    .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }
            return query.ToList();

        }
        public T Get(Expression<Func<T, bool>> filter, string? include = null, bool tracked = false)
        {
            IQueryable<T> query;
            if (tracked)
            {
                 query = _Dbset;
            }
            else
            {
              query = _Dbset.AsNoTracking();              
            }
            query = query.Where(filter);
            if (!string.IsNullOrEmpty(include))
            {
                foreach (var includeProp in include
                    .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }
            return query.FirstOrDefault();
        }
        public void Add(T Entity)
        {
            _Dbset.Add(Entity);
        }



        public void Remove(T Entity)
        {
            _Dbset.Remove(Entity);
        }

        public void RemoveRange(IEnumerable<T> Entity)
        {
            _Dbset.RemoveRange(Entity);
        }
    }
}
