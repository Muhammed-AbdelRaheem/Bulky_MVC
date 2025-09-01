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
    public class ProductImageRepository : Repository<ProductImage>, IProductImageRepository
    {
        private readonly AppDbContext _context;

        public ProductImageRepository(AppDbContext context) : base(context)
        {
            this._context = context;
        }

    
        public void Update(ProductImage Entity)
        {
            _context.ProductImages.Update(Entity);
        }
    }
}
