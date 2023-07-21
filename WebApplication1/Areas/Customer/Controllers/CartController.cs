using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Stripe.Checkout;
using System.Linq;
using System.Security.Claims;
using WebApplication1.DataAccess.Repository.IRepository;
using WebApplication1.Models;
using WebApplication1.Models.ViewModels;
using WebApplication1.Utility;

namespace WebApplication1.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailSender _emailSender;
        //This peoperty is use for price total that was add. and display total price on cartcontroller/indexpage
        // public int OrderTotal { get; set; }
        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }
        public CartController(IUnitOfWork unitOfWork, IEmailSender emailSender)
        {
            _unitOfWork = unitOfWork;
            _emailSender = emailSender;
        }

        public IActionResult Index()
        {
            //display the book of user according he/she add to cart and every user has different book add to cart  
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            ShoppingCartVM = new ShoppingCartVM()
            {
                ListCart = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value, 
                includeProperties: "Product"),

                //when we create new shopping cart vm model then we order create order header
                //when we update the price, then does not give error   
                OrderHeader = new()
        };
            //All the item in our shopping cart and pass the quantity with price
            foreach(var cart in ShoppingCartVM.ListCart)
            {
                cart.Price = GetPriceBasedOnQuantity(cart.Count, cart.Product.Price,
                    cart.Product.Price50, cart.Product.Price100);
                //Price total
                //ShoppingCartVM.CartTotal += (cart.Count * cart.Price); //Comment this because in shoppingcartVM
                // now we use Order header so we have orertotal in that
				ShoppingCartVM.OrderHeader.OrderTotal += (cart.Count * cart.Price);
			}
            return View(ShoppingCartVM);
        }



        // summary button code
		public IActionResult Summary()
		{
            //display the book of user according he/she add to cart and every user has different book add to cart  
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            ShoppingCartVM = new ShoppingCartVM()
            {
                ListCart = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value, 
                includeProperties: "Product"),
                OrderHeader = new()
            };

            // retrive application user details for logged in user
            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.GetFirstOrDefault(
                u => u.Id == claim.Value);
            // populate all the properties inside the header
            ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
			ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
			ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
			ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
			ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;
			ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;


			//All the item in our shopping cart and pass the quantity with price
			foreach (var cart in ShoppingCartVM.ListCart)
            {
                cart.Price = GetPriceBasedOnQuantity(cart.Count, cart.Product.Price,
                    cart.Product.Price50, cart.Product.Price100);
				//Price total so use if we have cart total in shoppingcartvm
				//ShoppingCartVM.CartTotal += (cart.Count * cart.Price);

				ShoppingCartVM.OrderHeader.OrderTotal += (cart.Count * cart.Price);
			}
            return View(ShoppingCartVM);
        }


        //Place Order Button Code.......
        [HttpPost]
        [ActionName("Summary")]
        [ValidateAntiForgeryToken]
		public IActionResult SummaryPOST()
		{
			//display the book of user according he/she add to cart and every user has different book add to cart  
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVM.ListCart = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value,
                includeProperties: "Product");

            // order time
            ShoppingCartVM.OrderHeader.OrderDate = System.DateTime.Now;
            ShoppingCartVM.OrderHeader.ApplicationUserId = claim.Value;

			//calculate order total
			foreach (var cart in ShoppingCartVM.ListCart)
			{
				cart.Price = GetPriceBasedOnQuantity(cart.Count, cart.Product.Price,
					cart.Product.Price50, cart.Product.Price100);
				//Price total so use if we have cart total in shoppingcartvm
				//ShoppingCartVM.CartTotal += (cart.Count * cart.Price);

				ShoppingCartVM.OrderHeader.OrderTotal += (cart.Count * cart.Price);
			}

			ApplicationUser applicationUser = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);
           // check either user is from company or individual
            if(applicationUser.CompanyId.GetValueOrDefault()== 0)
            {
				//order status and payment status
				ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
				ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
			}
            else
            {

				// order status and payment status
				ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayPayment;
				ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
			}
			//save the order header details into database
			_unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
            _unitOfWork.Save();


			//now code for order detail
            //FOR ORDER DETAIL WE NEED PRODUCT ID, ORDER ID, AND PRICE and count.
			foreach (var cart in ShoppingCartVM.ListCart)
			{
                OrderDetail orderDetail = new()
                {
                    ProductId = cart.ProductId,
                    OrderId = ShoppingCartVM.OrderHeader.Id,
                    Price= cart.Price,  
                    Count= cart.Count
                };
                // save the details into database
                _unitOfWork.OrderDetail.Add(orderDetail);
                _unitOfWork.Save();
			}
            // check either user is company user or indivdual user
            if (applicationUser.CompanyId.GetValueOrDefault() == null)
            {
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
                    SuccessUrl = domain + $"customer/cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.Id}",
                    CancelUrl = domain + $"customer/cart/index",
                };
                foreach (var item in ShoppingCartVM.ListCart)
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
                _unitOfWork.OrderHeader.UpdateStripePaymentId(ShoppingCartVM.OrderHeader.Id,
                    session.Id, session.PaymentIntentId);
                _unitOfWork.Save();
                Response.Headers.Add("Location", session.Url);
                //303 means redirect to stripe portal
                return new StatusCodeResult(303);
            }

            else
            {
                return RedirectToAction("OrderConfirmation", "Cart", new { id= ShoppingCartVM.OrderHeader.Id});
            }
		}

		//this method is use for successurl=domain + $"customer/cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.Id}"
		public IActionResult OrderConfirmation(int id)
		{
			OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == id,includeProperties: " ApplicationUser");
            //if user is individual and this if condition check
            if (orderHeader.PaymentStatus != SD.PaymentStatusDelayPayment)
            {
                //get sessionid that was create in summarypost method
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);
                //check the stripe status
                if (session.PaymentStatus.ToLower() == "paid")
                {
					_unitOfWork.OrderHeader.UpdateStripePaymentId(id, orderHeader.SessionId, session.PaymentIntentId);
					_unitOfWork.OrderHeader.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
                    _unitOfWork.Save();
                }
            }
            //send email to user that order is confirm
            _emailSender.SendEmailAsync(orderHeader.ApplicationUser.Email, "New Order","<p>New Order Created</p>");
            //get the list of shoppingcart
            List<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == orderHeader.ApplicationUserId).ToList();
            //After all of this we will clear over shopping cart
            //use removerang method to remove a collection
            _unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
            _unitOfWork.Save();
            return View(id);
        }


		//Plus method in which we add more product when we click on plus button
		public IActionResult Plus( int cartId)
        {

            var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(x => x.Id == cartId);
            _unitOfWork.ShoppingCart.IncrementCount(cart, 1);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }


        //Minus the product into the cart
		public IActionResult Minus(int cartId)
		{

			var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(x => x.Id == cartId);
            //use if become if we do not use that and user click on minus button after 0 its goes 
            // to -1 -2 -3 and decrease the values
            if (cart.Count <=1)
            {
				_unitOfWork.ShoppingCart.Remove(cart);
			}
            else { 
                _unitOfWork.ShoppingCart.DecrementCount(cart, 1);
                }
			_unitOfWork.Save();
			return RedirectToAction(nameof(Index));
		}



        //Remove the product into the cart
		public IActionResult Remove(int cartId)
		{

			var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(x => x.Id == cartId);
			_unitOfWork.ShoppingCart.Remove(cart);
			_unitOfWork.Save();
			return RedirectToAction(nameof(Index));
		}




		//this method check the quantity of price if user select below 50,100 or above the 100
		private double GetPriceBasedOnQuantity(double quantity, double price, double price50, double price100)
        {
            if(quantity <= 50)
            {
                return price;
            }
            if (quantity <=100)
            {
                return price50;
            }
            return price100;
        }
    }
}
