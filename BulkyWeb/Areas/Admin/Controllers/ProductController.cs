
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
            var products = _unitOfWork.Product.GetAll(null,"Category").ToList();



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
                productVM.Product = _unitOfWork.Product.Get(u => u.Id == id,include:"ProductImages");

                return View(productVM);
            }

        }

        [HttpPost]
        public IActionResult UpSert(ProductViewModel model, List<IFormFile>? files)
        {



            if (ModelState.IsValid)
            {
                if (model.Product.Id == 0)
                {
                    _unitOfWork.Product.Add(model.Product);

                }
                else
                {
                    _unitOfWork.Product.Update(model.Product);

                }
                _unitOfWork.Save();




                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (files != null)
                {

                    foreach (IFormFile file in files)
                    {
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        string productPath = @"images\products\product-" + model.Product.Id;
                        string finalPath = Path.Combine(wwwRootPath, productPath);

                        if (!Directory.Exists(finalPath))
                            Directory.CreateDirectory(finalPath);

                        using (var fileStream = new FileStream(Path.Combine(finalPath, fileName), FileMode.Create))
                        {
                            file.CopyTo(fileStream);
                        }

                        ProductImage productImage = new()
                        {
                            ImageUrl = @"\" + productPath + @"\" + fileName,
                            ProductId = model.Product.Id,
                        };

                        if (model.Product.ProductImages == null)
                            model.Product.ProductImages = new List<ProductImage>();

                        model.Product.ProductImages.Add(productImage);

                    }

                    _unitOfWork.Product.Update(model.Product);
                    _unitOfWork.Save();




                }

                TempData["success"] = $"Category {model.Product.Title} Created Successfully";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                ModelState.AddModelError("", "Error while creating product");
                return View(model);
            }


        }




        public IActionResult DeleteImage(int imageId)
        {
            var imageToBeDeleted = _unitOfWork.ProductImage.Get(u => u.Id == imageId);
            int productId = imageToBeDeleted.ProductId;
            if (imageToBeDeleted != null)
            {
                if (!string.IsNullOrEmpty(imageToBeDeleted.ImageUrl))
                {
                    var oldImagePath =
                                   Path.Combine(_webHostEnvironment.WebRootPath,
                                   imageToBeDeleted.ImageUrl.TrimStart('\\'));

                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                _unitOfWork.ProductImage.Remove(imageToBeDeleted);
                _unitOfWork.Save();

                TempData["success"] = "Deleted successfully";
            }

            return RedirectToAction(nameof(Upsert), new { id = productId });
        }


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
            string productPath = @"images\products\product-" + id;
            string finalPath = Path.Combine(_webHostEnvironment.WebRootPath, productPath);

            if (Directory.Exists(finalPath))
            {
                string[] filePaths = Directory.GetFiles(finalPath);
                foreach (string filePath in filePaths)
                {
                    System.IO.File.Delete(filePath);
                }

                Directory.Delete(finalPath);
            }
            _unitOfWork.Product.Remove(model);
            _unitOfWork.Save();
         return   Json(new { success = true, message = "Delete Successful" });
        }
        #endregion
    }
}

