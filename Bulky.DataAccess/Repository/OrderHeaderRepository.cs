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
    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {
        private readonly AppDbContext _context;

        public OrderHeaderRepository(AppDbContext context) : base(context)
        {
            this._context = context;
        }

    
        public void Update(OrderHeader Entity)
        {
            _context.orderHeaders.Update(Entity);
        }
    }
}
