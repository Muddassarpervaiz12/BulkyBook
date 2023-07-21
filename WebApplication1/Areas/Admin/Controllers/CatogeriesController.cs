using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.DataAccess.Repository;
using WebApplication1.DataAccess.Repository.IRepository;
using WebApplication1.Models;
using WebApplication1.Utility;

namespace WebApplication1.Controllers
{
    [Area("Admin")]
    [Authorize(Roles=SD.Role_Admin)]
    public class CatogeriesController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CatogeriesController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            IEnumerable<Catogery> objCategoryList = _unitOfWork.Category.GetAll();
            return View(objCategoryList);
        }

        //Get
        public IActionResult Created()
        {

            return View();
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Created(Catogery obj)
        {
            if (obj.Name == obj.DisplayOrder.ToString())
            {
                ModelState.AddModelError("Name", "Category name and Display Order must be different");
            }
            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Add(obj);
                _unitOfWork.Save();
                TempData["Success"] = "Create Category Successfully";
                return RedirectToAction("Index");
            }
            return View(obj);
        }


        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            //var editDb = _db.products.Find(id);
            var editDbFirst = _unitOfWork.Category.GetFirstOrDefault(u => u.Id == id);
            //var editDbSingle= _db.products.SingleOrDefault(u => u.Id==id);
            if (editDbFirst == null)
            {
                return NotFound();
            }

            return View(editDbFirst);
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Catogery obj)
        {
            if (obj.Name == obj.DisplayOrder.ToString())
            {
                ModelState.AddModelError("Name", "Category name and Display Order must be different");
            }
            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Update(obj);
                _unitOfWork.Save();
                TempData["Success"] = "Update Successfully";
                return RedirectToAction("Index");
            }
            return View(obj);
        }


        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            //var deleteDb = _db.products.Find(id);
            var deleteDbFirst = _unitOfWork.Category.GetFirstOrDefault(u => u.Id == id);
            //var deleteDbSingle= _db.products.SingleOrDefault(u => u.Id==id);
            if (deleteDbFirst == null)
            {
                return NotFound();
            }

            return View(deleteDbFirst);
        }

        //POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePOST(int? id)
        {
            var obj = _unitOfWork.Category.GetFirstOrDefault(u => u.Id == id);
            if (obj == null)
            {
                return NotFound();
            }

            _unitOfWork.Category.Remove(obj);
            _unitOfWork.Save();
            TempData["Success"] = "Delete Successfully";
            return RedirectToAction("Index");
        }
    }
}
