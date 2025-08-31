using Bulky.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.Models.ViewModels
{
    public class OrderVm
    {
        public OrderHeader orderHeader { get; set; }
        public IEnumerable<OrderDetail> orderDetails { get; set; }

    }
}
