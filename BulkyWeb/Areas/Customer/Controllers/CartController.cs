using Bulky.DataAccess.IRepository;
using Bulky.Models.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
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
        public ShoppingCartVm? ShoppingCartVm { get; set; }
        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var cliams = (ClaimsIdentity)User.Identity;
            var userId = cliams.FindFirst(ClaimTypes.NameIdentifier).Value;
            var cartList = _unitOfWork.ShoppingCart.GetAll(u => u.AppUserId == userId, include: "Product");

            ShoppingCartVm = new ShoppingCartVm
            {
                ShoppingCartList = cartList,
                OrderHeader = new()

            };
            IEnumerable<ProductImage> productImages = _unitOfWork.ProductImage.GetAll();

            foreach (var cart in ShoppingCartVm.ShoppingCartList)
            {
                cart.Product.ProductImages = productImages.Where(u => u.ProductId == cart.Product.Id).ToList();
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVm.OrderHeader.OrderTotal += (cart.Price * cart.Count);



            }
       


            return View(ShoppingCartVm);
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


        public IActionResult Minus(int cartId )
        {
            var cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId,tracked:true);
            if (cartFromDb.Count <= 1)
            {
                HttpContext.Session.SetInt32(Sd.SessionCart, _unitOfWork.ShoppingCart
                  .GetAll(u => u.AppUserId == cartFromDb.AppUserId).Count() - 1);
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
            var cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId, tracked: true);
            HttpContext.Session.SetInt32(Sd.SessionCart, _unitOfWork.ShoppingCart
             .GetAll(u => u.AppUserId == cartFromDb.AppUserId).Count() - 1);
            _unitOfWork.ShoppingCart.Remove(cartFromDb);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));

        }


        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVm = new()
            {
                ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.AppUserId == userId,
                include: "Product"),
                OrderHeader = new()
            };

            ShoppingCartVm.OrderHeader.ApplicationUser = _unitOfWork.applicationUser.Get(u => u.Id == userId);

            ShoppingCartVm.OrderHeader.Name = ShoppingCartVm.OrderHeader.ApplicationUser.name;
            ShoppingCartVm.OrderHeader.PhoneNumber = ShoppingCartVm.OrderHeader.ApplicationUser.PhoneNumber;
            ShoppingCartVm.OrderHeader.StreetAddress = ShoppingCartVm.OrderHeader.ApplicationUser.streetAddress;
            ShoppingCartVm.OrderHeader.City = ShoppingCartVm.OrderHeader.ApplicationUser.city;
            ShoppingCartVm.OrderHeader.State = ShoppingCartVm.OrderHeader.ApplicationUser.state;
            ShoppingCartVm.OrderHeader.PostalCode = ShoppingCartVm.OrderHeader.ApplicationUser.postalCode;



            foreach (var cart in ShoppingCartVm.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVm.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }
            return View(ShoppingCartVm);
        }

        [HttpPost]
        [ActionName("Summary")]
        public IActionResult SummaryPOST()
        {
            
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                ShoppingCartVm.ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.AppUserId == userId,
                    include: "Product");

                ShoppingCartVm.OrderHeader.OrderDate = DateTime.Now;
                ShoppingCartVm.OrderHeader.ApplicationUserId = userId;

                ApplicationUser applicationUser = _unitOfWork.applicationUser.Get(u => u.Id == userId);


                foreach (var cart in ShoppingCartVm.ShoppingCartList)
                {
                    cart.Price = GetPriceBasedOnQuantity(cart);
                    ShoppingCartVm.OrderHeader.OrderTotal += (cart.Price * cart.Count);
                }

                if (applicationUser.companyId.GetValueOrDefault() == 0)
                {
                    //it is a regular customer 
                    ShoppingCartVm.OrderHeader.PaymentStatus = Sd.PaymentStatusPending;
                    ShoppingCartVm.OrderHeader.OrderStatus = Sd.StatusPending;
                }
                else
                {
                    //it is a company user
                    ShoppingCartVm.OrderHeader.PaymentStatus = Sd.PaymentStatusDelayedPayment;
                    ShoppingCartVm.OrderHeader.OrderStatus = Sd.StatusApproved;
                }
                _unitOfWork.OrderHeader.Add(ShoppingCartVm.OrderHeader);
                _unitOfWork.Save();
                foreach (var cart in ShoppingCartVm.ShoppingCartList)
                {
                    OrderDetail orderDetail = new()
                    {
                        ProductId = cart.ProductId,
                        OrderHeaderId = ShoppingCartVm.OrderHeader.Id,
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
                    var domain = "https://localhost:7143/";
                    var options = new SessionCreateOptions
                    {
                        SuccessUrl = domain + $"Customer/Cart/OrderConfirmation?id={ShoppingCartVm.OrderHeader.Id}",
                        CancelUrl = domain + "Customer/Cart/index",
                        LineItems = new List<SessionLineItemOptions>(),
                        Mode = "payment",
                    };
                    foreach (var item in ShoppingCartVm.ShoppingCartList)
                    {
                        var sessionLineItem = new SessionLineItemOptions
                        {
                            PriceData = new SessionLineItemPriceDataOptions
                            {
                                UnitAmount = (long)(item.Price * 100), // $20.50 => 2050
                                Currency = "usd",
                                ProductData = new SessionLineItemPriceDataProductDataOptions
                                {
                                    Name = item.Product.Title
                                }
                            },
                            Quantity = item.Count
                        };
                        options.LineItems.Add(sessionLineItem);
                    }
                    var service = new SessionService();
                    Session session = service.Create(options);
                    _unitOfWork.OrderHeader.UpdateStripePaymentId(ShoppingCartVm.OrderHeader.Id, session.Id, session.PaymentIntentId);
                    _unitOfWork.Save();
                    Response.Headers.Add("Location", session.Url);
                    return new StatusCodeResult(303);

                }

         

                return RedirectToAction(nameof(OrderConfirmation), new { id = ShoppingCartVm.OrderHeader.Id });
        }


        public IActionResult OrderConfirmation(int id)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == id, include: "ApplicationUser");
            if (orderHeader.PaymentStatus != Sd.PaymentStatusDelayedPayment)
            {
                //this is an order by customer

                var service = new SessionService(); 
                Session session = service.Get(orderHeader.SessionId);

                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.OrderHeader.UpdateStripePaymentId(id, session.Id, session.PaymentIntentId);
                    _unitOfWork.OrderHeader.UpdateStatus(id, Sd.StatusApproved, Sd.PaymentStatusApproved);
                    _unitOfWork.Save();
                }
                HttpContext.Session.Clear();

            }
            List<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCart
               .GetAll(u => u.AppUserId == orderHeader.ApplicationUserId).ToList();

            _unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
            _unitOfWork.Save();

            return View(id);

        }
    }
}
