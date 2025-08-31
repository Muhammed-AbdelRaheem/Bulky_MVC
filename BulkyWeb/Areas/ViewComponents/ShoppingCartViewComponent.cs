using Bulky.DataAccess.Data;
using Bulky.DataAccess.IRepository;
using Bulky.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BulkyWeb.Areas.ViewComponents
{
    public class ShoppingCartViewComponent:ViewComponent
    {
        private readonly AppDbContext _dbContext;
        private readonly IUnitOfWork _unitOfWork;

        public ShoppingCartViewComponent(AppDbContext dbContext,IUnitOfWork unitOfWork   )
        {
            this._dbContext = dbContext;
            this._unitOfWork = unitOfWork;
        }
        public async Task<IViewComponentResult> InvokeAsync(int cartCount)
        {

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null)
            {
                if (HttpContext.Session.GetInt32(Sd.SessionCart)==null)
                {
                    HttpContext.Session.SetInt32(Sd.SessionCart,
                    _unitOfWork.ShoppingCart.GetAll(u => u.AppUserId == claim.Value).Count());
                }

                return View(HttpContext.Session.GetInt32(Sd.SessionCart));
            }
            else
            {
                HttpContext.Session.Clear();
                return View(0);
            }


        }
    }
}
