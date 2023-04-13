using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;
using WebApplication1.DataAccess.Repository.IRepository;
using WebApplication1.Models;

namespace WebApplication1.Areas.Customer.Controllers
{
    [Area ("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }
        //display all books 
        public IActionResult Index()
        {
            IEnumerable<Product> productList = _unitOfWork.Product.GetAll(includeProperties:"Catogery,CoverType");
            return View(productList);
        }
        //click on details button then show details page where show count 1
        public IActionResult Details(int productid)
        {
            ShoppingCart cartobj = new()
            {
                Count=1,
                ProductId=productid,
                Product = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == productid, includeProperties: "Catogery,CoverType"),
             };
            return View(cartobj);
        }
        //if we want to add more same book in "Add to cart" with same user so this code use
        //Insert or create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            var claimsIdentity =(ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            shoppingCart.ApplicationUserId = claim.Value;
            ShoppingCart cardFromDb= _unitOfWork.ShoppingCart.GetFirstOrDefault(u=> u.ApplicationUserId==claim.Value && 
            u.ProductId==shoppingCart.ProductId);
            if (cardFromDb == null)
            {
                _unitOfWork.ShoppingCart.Add(shoppingCart);
            }
            else
            {
                _unitOfWork.ShoppingCart.IncrementCount(cardFromDb, shoppingCart.Count);
            }
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}