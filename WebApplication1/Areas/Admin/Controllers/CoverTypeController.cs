using Microsoft.AspNetCore.Mvc;
using WebApplication1.DataAcces;
using WebApplication1.DataAccess.Repository.IRepository;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Area("Admin")]
    public class CoverTypeController : Controller
    {
      private readonly IUnitOfWork _unitOfWork;
        public CoverTypeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            IEnumerable<CoverType> objCoverTypeList = _unitOfWork.CoverType.GetAll();
            return View(objCoverTypeList);
        }

        //Get
        public IActionResult Created()
        {

            return View();
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Created(CoverType obj)
        {
            if (obj.Name == obj.Id.ToString())
            {
                ModelState.AddModelError("Name", "Cover Type name and Id must be different");
            }
            if(ModelState.IsValid) {
                _unitOfWork.CoverType.Add(obj);
                _unitOfWork.Save();
                TempData["Success"]= "Cover Type Create Successfully";
                return RedirectToAction("Index");
            }
            return View(obj);
        }


        public IActionResult Edit(int? id)
        {
            if(id == null || id == 0)
            {
                return NotFound();
            }
            //var editDb = _db.CoverType.Find(id);
            var editDbFirst = _unitOfWork.CoverType.GetFirstOrDefault(u => u.Id== id);
            //var editDbSingle= _db.CoverType.SingleOrDefault(u => u.Id==id);
            if (editDbFirst == null)
            {
                return NotFound();
            }

            return View(editDbFirst);
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CoverType obj)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.CoverType.Update(obj);
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
            //var deleteDb = _db.CoverType.Find(id);
            var deleteDbFirst = _unitOfWork.CoverType.GetFirstOrDefault(u => u.Id == id);
            //var deleteDbSingle= _db.CoverType.SingleOrDefault(u => u.Id==id);
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
           var obj= _unitOfWork.CoverType.GetFirstOrDefault(u => u.Id == id);
            if (obj == null)
            {
                return NotFound();
            }

            _unitOfWork.CoverType.Remove(obj);
                _unitOfWork.Save();
                TempData["Success"] = "Delete Successfully";
                return RedirectToAction("Index");
        }

    }

}
