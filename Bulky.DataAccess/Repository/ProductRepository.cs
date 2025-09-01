using Bulky.DataAccess.Data;
using Bulky.DataAccess.IRepository;
using Bulky.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly AppDbContext _context;

        public ProductRepository(AppDbContext context) : base(context)
        {
            this._context = context;
        }

    
        public void Update(Product Entity)
        {
            var EntityFromDb = _context.Products.FirstOrDefault(u => u.Id == Entity.Id);
            if (EntityFromDb != null)
            {
                EntityFromDb.Title = Entity.Title;
                EntityFromDb.ISBN = Entity.ISBN;
                EntityFromDb.Price = Entity.Price;
                EntityFromDb.Price50 = Entity.Price50;
                EntityFromDb.ListPrice = Entity.ListPrice;
                EntityFromDb.Price100 = Entity.Price100;
                EntityFromDb.Description = Entity.Description;
                EntityFromDb.CategoryId = Entity.CategoryId;
                EntityFromDb.Author = Entity.Author;
                EntityFromDb.ProductImages = Entity.ProductImages;

           
            }
        }
    
    }
}
