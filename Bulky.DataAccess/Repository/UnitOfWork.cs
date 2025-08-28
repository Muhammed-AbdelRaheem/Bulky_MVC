using Bulky.DataAccess.Data;
using Bulky.DataAccess.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        public ICategoryRepository Category { get; }
        public IProductRepository Product { get; }


        public UnitOfWork(AppDbContext context)
        {
            _context = context;
            Category = new CategoryRepository(_context);
            Product= new ProductRepository(_context);
        }



        public void Save()
        {
            _context.SaveChanges();
        }
    }
}
