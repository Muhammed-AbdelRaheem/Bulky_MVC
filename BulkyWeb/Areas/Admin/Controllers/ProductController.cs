
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
    [Authorize(Roles = Sd.Role_Admin)]

    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {

            _unitOfWork = unitOfWork;
            this._webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            var products = _unitOfWork.Product.GetAll("Category").ToList();



            return View(products);
        }


 
        public IActionResult Upsert(int? id)
        {
            ProductViewModel productVM = new()
            {
                CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                Product = new Product()
            };
            if (id == null || id == 0)
            {
                //create
                return View(productVM);
            }
            else
            {
                //update
                productVM.Product = _unitOfWork.Product.Get(u => u.Id == id);
                return View(productVM);
            }

        }

        [HttpPost]
        public IActionResult UpSert(ProductViewModel model, IFormFile? file)
        {



            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productPath = Path.Combine(wwwRootPath, @"images\product");

                    if (!string.IsNullOrEmpty(model.Product.ImageUrl))
                    {
                        var oldImagePath = Path.Combine(wwwRootPath, model.Product.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        } 
                    }
                    using var fileStrem = new FileStream(Path.Combine(productPath, fileName), FileMode.Create);
                    { file.CopyTo(fileStrem); }

                    model.Product.ImageUrl = @"\images\product\" + fileName;
                }

                if (model.Product.Id == 0)
                {
                    _unitOfWork.Product.Add(model.Product);

                }
                else
                {
                    _unitOfWork.Product.Update(model.Product);

                }
                _unitOfWork.Save();
                TempData["success"] = $"Category {model.Product.Title} Created Successfully";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                ModelState.AddModelError("", "Error while creating product");
                return View(model);
            }


        }




        //public IActionResult Delete(int? id)
        //{
        //    if (id == null || id == 0)
        //    {
        //        return NotFound();
        //    }
        //    var productFromDb = _unitOfWork.Product.Get(i => i.Id == id);

        //    if (productFromDb == null)
        //    {
        //        return NotFound();
        //    }
        //    return View(productFromDb);
        //}
        //[HttpPost, ActionName(nameof(Delete))]
        //public IActionResult DeletePost(int? id)
        //{
        //    var model = _unitOfWork.Product.Get(i => i.Id == id);

        //    if (model.Id == null)
        //    {
        //        return NotFound();

        //    }

        //    _unitOfWork.Product.Remove(model);
        //    _unitOfWork.Save();
        //    TempData["success"] = $"Category {model.Title} Deleted Successfully";

        //    return RedirectToAction(nameof(Index));
        //}



        #region Call API
        [HttpGet]
        public IActionResult GetAll()
        {
            var productList = _unitOfWork.Product.GetAll(include: "Category");
            return Json(new { data = productList });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var model = _unitOfWork.Product.Get(i => i.Id == id);
            if (model == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }
            var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, model.ImageUrl.TrimStart('\\'));
           
            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }

            _unitOfWork.Product.Remove(model);
            _unitOfWork.Save();
         return   Json(new { success = true, message = "Delete Successful" });
        }
        #endregion
    }
}

