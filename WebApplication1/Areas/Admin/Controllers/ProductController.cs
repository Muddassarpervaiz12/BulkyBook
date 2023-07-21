using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebApplication1.DataAccess.Repository.IRepository;
using WebApplication1.Models.ViewModels;
using WebApplication1.Utility;

namespace WebApplication1.Controllers
{
    [Area("Admin")]
	[Authorize(Roles = SD.Role_Admin)]
	public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
        }

        public IActionResult Index()
        {

            return View();
        }
        //Get
        public IActionResult Upsert(int? id)
        {
			//load the catogery and covertype list on the page
            ProductVM productVM = new()
            {
                product = new(),
                CategoryList = _unitOfWork.Category.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                }),

                CoverTypeList = _unitOfWork.CoverType.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                }),

            };

            if (id == null || id == 0)
            {
                // Create Product
                //if we do not create view model folder in webapplication1.models then we use this code
                //ViewBag.CategoryList = CategoryList;
                //ViewData["CoverTypeList"] = CoverTypeList;
                return View(productVM);
            }
            else
            {
                //Update Product
                productVM.product = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == id);
                return View(productVM);
            }

        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductVM obj, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _hostEnvironment.WebRootPath;
                // upload image on wwwroot folder
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(wwwRootPath, @"images\product");
                    var extension = Path.GetExtension(file.FileName);

                    //old image is delete when we update image
                    if (obj.product.Imageurl != null)
                    {
                        var oldImagePath = Path.Combine(wwwRootPath, obj.product.Imageurl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }
                    //image url update
                    using (var fileStreams = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                    {
                        file.CopyTo(fileStreams);
                    }
                    obj.product.Imageurl = @"\images\product\" + fileName + extension;
                }
                //image add or update
                if (obj.product.Id == 0)
                {
                    _unitOfWork.Product.Add(obj.product);
                    _unitOfWork.Save();
                    TempData["Success"] = "Added Successfully";
                }
                else
                {
                    _unitOfWork.Product.Update(obj.product);
                    _unitOfWork.Save();
                    TempData["Success"] = "Update Successfully";
                }
                return RedirectToAction("Index");
            }
            return View(obj);
       }
        

        //SWEERALERT
        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            var productList = _unitOfWork.Product.GetAll(includeProperties: "Catogery,CoverType");
            return Json(new { data = productList });
        }

        //POST
        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var obj = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == id);
            if (obj == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }
            //delete old image from wwwroot folder
            var oldImagePath = Path.Combine(_hostEnvironment.WebRootPath, obj.Imageurl.TrimStart('\\'));
            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }

            _unitOfWork.Product.Remove(obj);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Delete Successfully" });
            
        }

        #endregion
    }
}