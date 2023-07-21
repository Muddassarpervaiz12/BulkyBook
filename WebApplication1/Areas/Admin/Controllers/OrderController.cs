using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Diagnostics;
using System.Security.Claims;
using WebApplication1.DataAccess.Repository.IRepository;
using WebApplication1.Models;
using WebApplication1.Models.ViewModels;
using WebApplication1.Utility;

namespace WebApplication1.Areas.Admin.Controllers
{
	[Area("Admin")]
	//here use authorize because only authorize user can view order details, make payment 
	[Authorize]
	public class OrderController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;
		[BindProperty]
		public OrderVM OrderVM { get; set; }
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
			OrderVM = new OrderVM()
			{
				orderHeader=_unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == orderId, includeProperties: "ApplicationUser"),
                OrderDetail = _unitOfWork.OrderDetail.GetAll(u => u.OrderId == orderId, includeProperties: "Product"),
            };
            return View(OrderVM);
        }

		//Deatils for Pay Now button
		[ActionName("Details")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Details_PAY_NOW()
		{
			//retrive order header and order details with the help of orderviewmodel when click on pay now button
			//and that button is only for company user
			OrderVM.orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == OrderVM.orderHeader.Id, includeProperties: "ApplicationUser");
			OrderVM.OrderDetail = _unitOfWork.OrderDetail.GetAll(u => u.OrderId == OrderVM.orderHeader.Id, includeProperties: "Product");

			//Now process of payment with stripe
			//stripe setting
			var domain = "https://localhost:44392/";
			// create session and add lineitems
			var options = new SessionCreateOptions
			{
				PaymentMethodTypes = new List<string>
				 {
					 "card",
				 },
				LineItems = new List<SessionLineItemOptions>(),
				Mode = "payment",
				SuccessUrl = domain + $"admin/order/PaymentConfirmation?orderHeaderid={OrderVM.orderHeader.Id}",
				CancelUrl = domain + $"admin/order/details?orderId={OrderVM.orderHeader.Id}",
			};
			foreach (var item in OrderVM.OrderDetail)
			{
				var sessionLineItem = new SessionLineItemOptions
				{
					PriceData = new SessionLineItemPriceDataOptions
					{
						UnitAmount = (long)(item.Price * 100),//20.00 -> 2000
						Currency = "pkr",
						ProductData = new SessionLineItemPriceDataProductDataOptions
						{
							Name = item.Product.Title,
						},
					},
					Quantity = item.Count,
				};
				options.LineItems.Add(sessionLineItem);

			}
			var service = new SessionService();
			//in this we have session id and payment intent id so we save that ids into the unitofwork
			Session session = service.Create(options);
			///updatestripepaymentId is a method use in orderheaderrepository
			_unitOfWork.OrderHeader.UpdateStripePaymentId(OrderVM.orderHeader.Id,
				session.Id, session.PaymentIntentId);
			_unitOfWork.Save();
			Response.Headers.Add("Location", session.Url);
			//303 means redirect to stripe portal
			return new StatusCodeResult(303);
		}

		//this method is use for successurl=domain + $"admin/order/PaymentConfirmation?orderHeaderid={OrderVM.orderHeader.Id}"
		public IActionResult PaymentConfirmation(int orderHeaderid)
		{
			OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == orderHeaderid);
			//if user is individual and this if condition check
			if (orderHeader.PaymentStatus == SD.PaymentStatusDelayPayment)
			{
				//get sessionid that was create in summarypost method
				var service = new SessionService();
				Session session = service.Get(orderHeader.SessionId);
				//check the stripe status
				if (session.PaymentStatus.ToLower() == "paid")
				{
					_unitOfWork.OrderHeader.UpdateStripePaymentId(orderHeaderid, orderHeader.SessionId, session.PaymentIntentId);
					//so only payment status is change and become approve
					_unitOfWork.OrderHeader.UpdateStatus(orderHeaderid, orderHeader.OrderStatus, SD.PaymentStatusApproved);
					_unitOfWork.Save();
				}
			}
			return View(orderHeaderid);
		}

		//update order details
		[HttpPost]
		//For using this authorize this function is only done by admin or employee role 
		[Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
		[ValidateAntiForgeryToken]
		public IActionResult UpdateOrderDetail()
		{
			//get order header from database
			var orderHeaderFromDb = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == OrderVM.orderHeader.Id , tracked: false);
			//now update all these parameter and save into orderheaderfromdb and get from orderVM
			orderHeaderFromDb.Name = OrderVM.orderHeader.Name;
			orderHeaderFromDb.PhoneNumber = OrderVM.orderHeader.PhoneNumber;
			orderHeaderFromDb.StreetAddress = OrderVM.orderHeader.StreetAddress;
			orderHeaderFromDb.City = OrderVM.orderHeader.City;
			orderHeaderFromDb.State = OrderVM.orderHeader.State;
			orderHeaderFromDb.PostalCode = OrderVM.orderHeader.PostalCode;

			// if some reason user want to update carrier and tracking number 
			if(OrderVM.orderHeader.Carrier != null)
			{
				orderHeaderFromDb.Carrier = OrderVM.orderHeader.Carrier;
			}
			if (OrderVM.orderHeader.TrackingNumber != null)
			{
				orderHeaderFromDb.TrackingNumber = OrderVM.orderHeader.TrackingNumber;
			}
			// now update all the changes and then save
			_unitOfWork.OrderHeader.Update(orderHeaderFromDb);
			_unitOfWork.Save();
			TempData["Success"] = "Order Detail Update Successfully";
			return RedirectToAction("Details", "Order", new { orderId=orderHeaderFromDb.Id});
			
		}


		//Start Processing button
		[HttpPost]
		//For using this authorize this function is only done by admin or employee role 
		[Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
		[ValidateAntiForgeryToken]
		public IActionResult StartProcessing()
		{
			//change status
			_unitOfWork.OrderHeader.UpdateStatus(OrderVM.orderHeader.Id, SD.StatusInProcess);
			_unitOfWork.Save();
			TempData["Success"] = "Order Status Update Successfully";
			//return to details pages and set orderId that is equal to orderVm orderHeader Id.	
			return RedirectToAction("Details", "Order", new { orderId = OrderVM.orderHeader.Id });
		}

		//Ship Order button
		[HttpPost]
		//For using this authorize this function is only done by admin or employee role 
		[Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
		[ValidateAntiForgeryToken]
		public IActionResult ShipOrder()
		{  //for ship order we need tracking number and carrrier details

		   //get order header from database
			var orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(
				u => u.Id == OrderVM.orderHeader.Id, tracked: false);
			//change these attribute
			orderHeader.TrackingNumber = OrderVM.orderHeader.TrackingNumber;
			orderHeader.Carrier = OrderVM.orderHeader.Carrier;
			orderHeader.OrderStatus = SD.StatusShipped;
			orderHeader.ShippingDate = DateTime.Now;
			if(orderHeader.PaymentStatus == SD.PaymentStatusApproved)
			{
				orderHeader.PaymentDueDate = DateTime.Now.AddDays(30);
			}
			//update orderheader or updarte all details
			_unitOfWork.OrderHeader.Update(orderHeader);
			_unitOfWork.Save();
			TempData["Success"] = "Order Shipped Successfully";
			//return to details pages and set orderId that is equal to orderVm orderHeader Id.	
			return RedirectToAction("Details", "Order", new { orderId = OrderVM.orderHeader.Id });
		}


		//Cancel Order button if payment is done then first refund, if
		//payment not done then directly cancel order
		[HttpPost]
		//For using this authorize this function is only done by admin or employee role 
		[Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
		[ValidateAntiForgeryToken]
		public IActionResult CancelOrder()
		{  //get order header from database
			var orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(
				u => u.Id == OrderVM.orderHeader.Id, tracked: false);
			//check payment status if approve then refund and cancel order else direct cancel order
			if(orderHeader.OrderStatus == SD.PaymentStatusApproved)
			{
				//refund logic
				var options = new RefundCreateOptions
				{ //reason and payment intent id for refund actual amount, but if we want to add custom
				  //amount then use Amount="1000",
					Reason = RefundReasons.RequestedByCustomer,
					PaymentIntent = orderHeader.PaymentIntentId
				};
				//create service it will be reference refundservice
				var service = new RefundService();
				//create refund object and pass options that will go to portal and 
				//make the actual refund on stripe
				Refund refund = service.Create(options);
				//update order status and payment Stauts
				_unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusRefunded);
			}
			else
			{
				_unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusCancelled);
			}
			_unitOfWork.Save();
			TempData["Success"] = "Order Cancelled Successfully";
			//return to details pages and set orderId that is equal to orderVm orderHeader Id.	
			return RedirectToAction("Details", "Order", new { orderId = OrderVM.orderHeader.Id });
		}
		#region API CALLS
		[HttpGet]
		public IActionResult GetAll(string status)
		{
			IEnumerable<OrderHeader> orderHeaders;
			//role base(admin,employee,user) order list display
			//admin and company employee both can see the order list
			if(User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
			{
               orderHeaders = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser");
            }
			//else is individual customer
			else
			{
				//get user id of the logged in user
				var claimsIdentity = (ClaimsIdentity)User.Identity;
				var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                orderHeaders = _unitOfWork.OrderHeader.GetAll(u=>u.ApplicationUserId == claim.Value,includeProperties: "ApplicationUser");
            }
			
			//filter order header status based on switch condition
			switch (status)
			{

                case "pending":
					//use where to filter only delaypayment status
                    orderHeaders= orderHeaders.Where(u=>u.PaymentStatus == SD.PaymentStatusDelayPayment);
                    break;

				case "inprocess":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusInProcess);
                    break;

                case "completed":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusShipped);
                    break;

                case "approved":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusApproved);
                    break;

                default:
                    break;
            }
			return Json(new { data = orderHeaders });
		}
		#endregion
	}
}
