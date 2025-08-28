
using Bulky.DataAccess.Data;
using Bulky.DataAccess.IRepository;
using Bulky.Models.Models;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles =Sd.Role_Admin)]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryController(IUnitOfWork unitOfWork)
        {

            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var Categories = _unitOfWork.Category.GetAll().ToList();

            return View(Categories);
        }


        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(Category model)
        {


            if (model.Name.ToLower() == model.DisplayOrder.ToString())
            {
                ModelState.AddModelError("name", "The Display Order Cannot Exactly Match The Name ");
            }
            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Add(model);
                _unitOfWork.Save();
                TempData["success"] = $"Category {model.Name} Created Successfully";
                return RedirectToAction(nameof(Index));
            }
            return View(model);


        }

        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var categoryFromDb = _unitOfWork.Category.Get(i => i.Id == id);

            if (categoryFromDb == null)
            {
                return NotFound();
            }
            return View(categoryFromDb);
        }
        [HttpPost]
        public IActionResult Edit(Category model)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Update(model);
                _unitOfWork.Save();
                TempData["success"] = $"Category {model.Name} Updated Successfully";

                return RedirectToAction(nameof(Index));
            }
            return View();
        }

        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var categoryFromDb = _unitOfWork.Category.Get(i => i.Id == id);

            if (categoryFromDb == null)
            {
                return NotFound();
            }
            return View(categoryFromDb);
        }
        [HttpPost, ActionName(nameof(Delete))]
        public IActionResult DeletePost(int? id)
        {
            var model = _unitOfWork.Category.Get(i => i.Id == id);

            if (model.Id == null)
            {
                return NotFound();

            }

            _unitOfWork.Category.Remove(model);
            _unitOfWork.Save();
            TempData["success"] = $"Category {model.Name} Deleted Successfully";

            return RedirectToAction(nameof(Index));
        }


    }
}

