using Bulky.DataAccess.IRepository;
using Bulky.Models.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using Stripe.Climate;
using System.Diagnostics;
using System.Security.Claims;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public OrderVm  OrderVm { get; set; }
        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Details(int orderId)
        {
             OrderVm = new OrderVm()
            {
                orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderId, include: "ApplicationUser"),
                orderDetails = _unitOfWork.OrderDetail.GetAll(u => u.OrderHeaderId == orderId, include: "Product")
            };
            return View(OrderVm);
        }

        [HttpPost]
        [Authorize(Roles = Sd.Role_Admin + "," + Sd.Role_Employee)]
        public IActionResult UpdateOrderDetail()
        {
            var orderHeaderFromDb = _unitOfWork.OrderHeader.Get(u => u.Id == OrderVm.orderHeader.Id);
            orderHeaderFromDb.Name = OrderVm.orderHeader.Name;
            orderHeaderFromDb.PhoneNumber = OrderVm.orderHeader.PhoneNumber;
            orderHeaderFromDb.StreetAddress = OrderVm.orderHeader.StreetAddress;
            orderHeaderFromDb.City = OrderVm.orderHeader.City;
            orderHeaderFromDb.State = OrderVm.orderHeader.State;
            orderHeaderFromDb.PostalCode = OrderVm.orderHeader.PostalCode;
            if (!string.IsNullOrEmpty(OrderVm.orderHeader.Carrier))
            {
                orderHeaderFromDb.Carrier = OrderVm.orderHeader.Carrier;
            }
            if (!string.IsNullOrEmpty(OrderVm.orderHeader.TrackingNumber))
            {
                orderHeaderFromDb.Carrier = OrderVm.orderHeader.TrackingNumber;
            }
            _unitOfWork.OrderHeader.Update(orderHeaderFromDb);
            _unitOfWork.Save();

            TempData["Success"] = "Order Details Updated Successfully.";
            return RedirectToAction(nameof(Details), new { orderId = orderHeaderFromDb.Id });
        }

        [HttpPost]
        [Authorize(Roles = Sd.Role_Admin + "," + Sd.Role_Employee)]
        public IActionResult StartProcessing()
        {
            _unitOfWork.OrderHeader.UpdateStatus(OrderVm.orderHeader.Id, Sd.StatusInProcess);
            _unitOfWork.Save();
            TempData["Success"] = "Order Details Updated Successfully.";
            return RedirectToAction(nameof(Details), new { orderId = OrderVm.orderHeader.Id });
        }



        [HttpPost]
        [Authorize(Roles = Sd.Role_Admin + "," + Sd.Role_Employee)]
        public IActionResult ShipOrder()
        {

            var orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == OrderVm.orderHeader.Id);
            orderHeader.TrackingNumber = OrderVm.orderHeader.TrackingNumber;
            orderHeader.Carrier = OrderVm.orderHeader.Carrier;
            orderHeader.OrderStatus = Sd.StatusShipped;
            orderHeader.ShippingDate = DateTime.Now;
            if (orderHeader.PaymentStatus == Sd.PaymentStatusDelayedPayment)
            {
                orderHeader.PaymentDueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
            }

            _unitOfWork.OrderHeader.Update(orderHeader);
            _unitOfWork.Save();
            TempData["Success"] = "Order Shipped Successfully.";
            return RedirectToAction(nameof(Details), new { orderId = OrderVm.orderHeader.Id });
        }



        [HttpPost]
        [Authorize(Roles = Sd.Role_Admin + "," + Sd.Role_Employee)]
        public IActionResult CancelOrder()
        {

            var orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == OrderVm.orderHeader.Id);

            if (orderHeader.PaymentStatus == Sd.PaymentStatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeader.PaymentIntentId
                };

                var service = new RefundService();
                Refund refund = service.Create(options);

                _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, Sd.StatusCancelled, Sd.StatusRefunded);
            }
            else
            {
                _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, Sd.StatusCancelled, Sd.StatusCancelled);
            }
            _unitOfWork.Save();
            TempData["Success"] = "Order Cancelled Successfully.";
            return RedirectToAction(nameof(Details), new { orderId = OrderVm.orderHeader.Id });

        }



        [ActionName("Details")]
        [HttpPost]
        public IActionResult Details_PAY_NOW()
        {
            OrderVm.orderHeader = _unitOfWork.OrderHeader
                .Get(u => u.Id == OrderVm.orderHeader.Id, include: "ApplicationUser");
            OrderVm.orderDetails = _unitOfWork.OrderDetail
                .GetAll(u => u.OrderHeaderId == OrderVm.orderHeader.Id, include: "Product");
            
            //stripe logic
            var domain = Request.Scheme + "://" + Request.Host.Value + "/";
            var options = new SessionCreateOptions
            {
                SuccessUrl = domain + $"admin/order/PaymentConfirmation?orderHeaderId={OrderVm.orderHeader.Id}",
                CancelUrl = domain + $"admin/order/details?orderId={OrderVm.orderHeader.Id}",
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
            };

            foreach (var item in OrderVm.orderDetails)
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
            _unitOfWork.OrderHeader.UpdateStripePaymentId(OrderVm.orderHeader.Id, session.Id, session.PaymentIntentId);
            _unitOfWork.Save();
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        public IActionResult PaymentConfirmation(int orderHeaderId)
        {

            OrderHeader orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderHeaderId);
            if (orderHeader.PaymentStatus == Sd.PaymentStatusDelayedPayment)
            {
                //this is an order by company

                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);

                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.OrderHeader.UpdateStripePaymentId(orderHeaderId, session.Id, session.PaymentIntentId);
                    _unitOfWork.OrderHeader.UpdateStatus(orderHeaderId, orderHeader.OrderStatus, Sd.PaymentStatusApproved);
                    _unitOfWork.Save();
                }


            }


            return View(orderHeaderId);
        }



        #region Call API
        [HttpGet]
        public IActionResult GetAll(string status)
        {

            IEnumerable<OrderHeader> OrderHeaders;

            if (User.IsInRole(Sd.Role_Admin)|| User.IsInRole(Sd.Role_Employee))
            {
                OrderHeaders = _unitOfWork.OrderHeader.GetAll(include: "ApplicationUser").ToList();
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                OrderHeaders = _unitOfWork.OrderHeader.GetAll(u => u.ApplicationUserId == userId, include: "ApplicationUser");


            }

                switch (status)
                {
                    case "pending":
                        OrderHeaders = OrderHeaders.Where(u => u.PaymentStatus == Sd.PaymentStatusDelayedPayment).ToList();
                        break;
                    case "inprocess":
                        OrderHeaders = OrderHeaders.Where(u => u.PaymentStatus == Sd.StatusInProcess).ToList();
                        break;
                    case "completed":
                        OrderHeaders = OrderHeaders.Where(u => u.PaymentStatus == Sd.StatusShipped).ToList();
                        break;
                    case "approved":
                        OrderHeaders = OrderHeaders.Where(u => u.PaymentStatus == Sd.StatusApproved).ToList();
                        break;
                    default:
                        break;

                }
            return Json(new { data = OrderHeaders });
        }

   
        #endregion

    }
}
