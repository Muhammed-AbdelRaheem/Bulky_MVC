using Bulky.DataAccess.IRepository;
using Bulky.Models.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;

namespace BulkyWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        [BindProperty]
        public ShoppingCartVw ShoppingCartVw { get; set; }
        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var cliams = (ClaimsIdentity)User.Identity;
            var userId = cliams.FindFirst(ClaimTypes.NameIdentifier).Value;
            var cartList = _unitOfWork.ShoppingCart.GetAll(u => u.AppUserId == userId, include: "Product");

            ShoppingCartVw = new ShoppingCartVw
            {
                ShoppingCartList = cartList,
                OrderHeader = new()

            };
            foreach (var cart in ShoppingCartVw.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVw.OrderHeader.OrderTotal += (cart.Price * cart.Count);



            }


            return View(ShoppingCartVw);
        }
        private double GetPriceBasedOnQuantity(ShoppingCart shoppingCart)
        {
            if (shoppingCart.Count <= 50)
            {
                return shoppingCart.Product.Price;
            }
            else
            {
                if (shoppingCart.Count <= 100)
                {
                    return shoppingCart.Product.Price50;
                }
                else
                {
                    return shoppingCart.Product.Price100;
                }
            }
        }

        public IActionResult Plus(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId);
            cartFromDb.Count += 1;
            _unitOfWork.ShoppingCart.Update(cartFromDb);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }


        public IActionResult Minus(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId);
            if (cartFromDb.Count <= 1)
            {
                _unitOfWork.ShoppingCart.Remove(cartFromDb);
            }
            else
            {
                cartFromDb.Count -= 1;
                _unitOfWork.ShoppingCart.Update(cartFromDb);
            }
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }


        public IActionResult Remove(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId);
            _unitOfWork.ShoppingCart.Remove(cartFromDb);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));

        }


        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVw = new()
            {
                ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.AppUserId == userId,
                include: "Product"),
                OrderHeader = new()
            };

            ShoppingCartVw.OrderHeader.ApplicationUser = _unitOfWork.applicationUser.Get(u => u.Id == userId);

            ShoppingCartVw.OrderHeader.Name = ShoppingCartVw.OrderHeader.ApplicationUser.name;
            ShoppingCartVw.OrderHeader.PhoneNumber = ShoppingCartVw.OrderHeader.ApplicationUser.PhoneNumber;
            ShoppingCartVw.OrderHeader.StreetAddress = ShoppingCartVw.OrderHeader.ApplicationUser.streetAddress;
            ShoppingCartVw.OrderHeader.City = ShoppingCartVw.OrderHeader.ApplicationUser.city;
            ShoppingCartVw.OrderHeader.State = ShoppingCartVw.OrderHeader.ApplicationUser.state;
            ShoppingCartVw.OrderHeader.PostalCode = ShoppingCartVw.OrderHeader.ApplicationUser.postalCode;



            foreach (var cart in ShoppingCartVw.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVw.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }
            return View(ShoppingCartVw);
        }

        [HttpPost]
        [ActionName("Summary")]
        public IActionResult SummaryPOST()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVw.ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.AppUserId == userId,
                include: "Product");

            ShoppingCartVw.OrderHeader.OrderDate = System.DateTime.Now;
            ShoppingCartVw.OrderHeader.ApplicationUserId = userId;

            ApplicationUser applicationUser = _unitOfWork.applicationUser.Get(u => u.Id == userId);


            foreach (var cart in ShoppingCartVw.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVw.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            if (applicationUser.companyId.GetValueOrDefault() == 0)
            {
                //it is a regular customer 
                ShoppingCartVw.OrderHeader.PaymentStatus = Sd.PaymentStatusPending;
                ShoppingCartVw.OrderHeader.OrderStatus = Sd.StatusPending;
            }
            else
            {
                //it is a company user
                ShoppingCartVw.OrderHeader.PaymentStatus = Sd.PaymentStatusDelayedPayment;
                ShoppingCartVw.OrderHeader.OrderStatus = Sd.StatusApproved;
            }
            _unitOfWork.OrderHeader.Add(ShoppingCartVw.OrderHeader);
            _unitOfWork.Save();
            foreach (var cart in ShoppingCartVw.ShoppingCartList)
            {
                OrderDetail orderDetail = new()
                {
                    ProductId = cart.ProductId,
                    OrderHeaderId = ShoppingCartVw.OrderHeader.Id,
                    Price = cart.Price,
                    Count = cart.Count
                };
                _unitOfWork.OrderDetail.Add(orderDetail);
                _unitOfWork.Save();
            }




            if (applicationUser.companyId.GetValueOrDefault() == 0)
            {
                //it is a regular customer account and we need to capture payment
                //stripe logic
              
            }

                return RedirectToAction(nameof(OrderConfirmation), new {id=ShoppingCartVw.OrderHeader.Id});
        }


        public IActionResult OrderConfirmation(int id)
        {
            return View(id);

        }
    }
}
