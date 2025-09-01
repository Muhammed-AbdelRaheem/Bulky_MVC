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
        public ICompanyRepository Company { get; }
        public IShoppingCartRepository ShoppingCart { get; }
        public IApplicationUserRepository applicationUser { get; }
        public IOrderHeaderRepository OrderHeader { get; }
        public IOrderDetailRepository OrderDetail { get; }

        public IProductImageRepository ProductImage{ get; }
        public UnitOfWork(AppDbContext context)
        {
            _context = context;
            Category = new CategoryRepository(_context);
            Product= new ProductRepository(_context);
            Company= new CompanyRepository(_context);
            ShoppingCart = new ShoppingCartRepository(_context);
            applicationUser= new ApplicationUserRepository(_context);
            OrderDetail= new OrderDetailRepository(_context);
            OrderHeader= new OrderHeaderRepository(_context);
            ProductImage = new ProductImageRepository(_context);
        }



        public void Save()
        {
            _context.SaveChanges();
        }
    }
}
