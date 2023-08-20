using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApplication1.DataAccess.Repository.IRepository;
using WebApplication1.Utility;

namespace WebApplication1.ViewComponents
{
    public class ShoppingCartViewComponent : ViewComponent
    {
        private readonly IUnitOfWork _unitOfWork;
        public ShoppingCartViewComponent(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        //this is use for check session id and then display on view/page in which display count
        //so fo view we use shared folder/components and then shoppingCart folder 
        public async Task<IViewComponentResult> InvokeAsync()
        {
            //user log in or not first check that 
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null)
            {
                //check if session is null or not 
                if(HttpContext.Session.GetInt32(SD.SessionCart)!= null)
                {
                    return View(HttpContext.Session.GetInt32(SD.SessionCart));
                }
                else
                {
                    HttpContext.Session.SetInt32(SD.SessionCart,  
                        _unitOfWork.ShoppingCart.GetAll(u=> u.ApplicationUserId == claim.Value).ToList().Count);
                    return View(HttpContext.Session.GetInt32(SD.SessionCart));
                }
            }
            else
            {
                HttpContext.Session.Clear();
                return View(0);
            }
        }
    }
}
