
using Bulky.DataAccess.Data;
using Bulky.DataAccess.IRepository;
using Bulky.Models.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize(Roles = Sd.Role_Admin)]

    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CompanyController(IUnitOfWork unitOfWork)
        {

            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var company = _unitOfWork.Company.GetAll().ToList();



            return View(company);
        }


 
        public IActionResult Upsert(int? id)
        {
          
            if (id == null || id == 0)
            {
                //create
                return View(new Company());
            }
            else
            {
                //update
                Company company = _unitOfWork.Company.Get(u => u.Id == id);
                return View(company);
            }

        }

        [HttpPost]
        public IActionResult UpSert(Company model)
        {



            if (ModelState.IsValid)
            {

                if (model.Id == 0)
                {
                    _unitOfWork.Company.Add(model);

                }
                else
                {
                    _unitOfWork.Company.Update(model);

                }
                _unitOfWork.Save();
                TempData["success"] = $"Category {model.Name} Created Successfully";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                ModelState.AddModelError("", "Error while creating Company");
                return View(model);
            }


        }







        #region Call API
        [HttpGet]
        public IActionResult GetAll()
        {
            var CompanyList = _unitOfWork.Company.GetAll();
            return Json(new { data = CompanyList });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var model = _unitOfWork.Company.Get(i => i.Id == id);
            if (model == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }
        

            _unitOfWork.Company.Remove(model);
            _unitOfWork.Save();
         return   Json(new { success = true, message = "Delete Successful" });
        }
        #endregion
    }
}

