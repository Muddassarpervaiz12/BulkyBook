using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
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

        public IActionResult Index()
        {
            IEnumerable<Product> productList = _unitOfWork.Product.GetAll(includeProperties:"Catogery,CoverType");
            return View(productList);
        }

        public IActionResult Details(int id)
        {
            ShoppingCart cartobj = new()
            {
                Count=1,
                Product = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == id, includeProperties: "Catogery,CoverType"),
             };
            return View(cartobj);
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