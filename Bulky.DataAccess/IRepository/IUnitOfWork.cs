using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.IRepository
{
    public interface IUnitOfWork
    {

        ICategoryRepository Category { get; }
        IProductRepository Product { get; }

        ICompanyRepository Company { get; }
        IShoppingCartRepository ShoppingCart { get; }
        IApplicationUserRepository applicationUser { get; }
        IOrderHeaderRepository OrderHeader { get; }

        IOrderDetailRepository OrderDetail { get; }
        IProductImageRepository ProductImage { get; }


        void Save();
    }
}
