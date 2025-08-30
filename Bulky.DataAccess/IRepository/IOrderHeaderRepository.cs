
using Bulky.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.IRepository
{
    public interface IOrderHeaderRepository : IRepository<OrderHeader> 

    {
        void Update(OrderHeader Entity);
     


    }
}
