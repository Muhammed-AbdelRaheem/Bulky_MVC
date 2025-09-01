using Bulky.DataAccess.Data;
using Bulky.DataAccess.IRepository;
using Bulky.Models.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;



namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = Sd.Role_Admin)]
    public class UserController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public UserController(IUnitOfWork unitOfWork,UserManager<IdentityUser> userManager, AppDbContext context, RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult RoleManagment(string userId)
        {

            RoleManagmentVM RoleVM = new RoleManagmentVM()
            {
                ApplicationUser = _unitOfWork.applicationUser.Get(u => u.Id == userId, include: "company"),
                RoleList = _roleManager.Roles.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Name
                }),
                CompanyList = _unitOfWork.Company.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                }),
            };

            RoleVM.ApplicationUser.Role = _userManager.GetRolesAsync(_unitOfWork.applicationUser.Get(u => u.Id == userId))
                    .GetAwaiter().GetResult().FirstOrDefault();
            return View(RoleVM);
        }

        [HttpPost]
        public IActionResult RoleManagment(RoleManagmentVM roleManagmentVM)
        {

            string oldRole = _userManager.GetRolesAsync(_unitOfWork.applicationUser.Get(u => u.Id == roleManagmentVM.ApplicationUser.Id))
                    .GetAwaiter().GetResult().FirstOrDefault();

            ApplicationUser applicationUser = _unitOfWork.applicationUser.Get(u => u.Id == roleManagmentVM.ApplicationUser.Id);


            if (!(roleManagmentVM.ApplicationUser.Role == oldRole))
            {
                //a role was updated
                if (roleManagmentVM.ApplicationUser.Role == Sd.Role_Company)
                {
                    applicationUser.companyId = roleManagmentVM.ApplicationUser.companyId;
                }
                if (oldRole == Sd.Role_Company)
                {
                    applicationUser.companyId = null;
                }
                _unitOfWork.applicationUser.Update(applicationUser);
                _unitOfWork.Save();

                _userManager.RemoveFromRoleAsync(applicationUser, oldRole).GetAwaiter().GetResult();
                _userManager.AddToRoleAsync(applicationUser, roleManagmentVM.ApplicationUser.Role).GetAwaiter().GetResult();

            }
            else
            {
                if (oldRole == Sd.Role_Company && applicationUser.companyId != roleManagmentVM.ApplicationUser.companyId)
                {
                    applicationUser.companyId = roleManagmentVM.ApplicationUser.companyId;
                    _unitOfWork.applicationUser.Update(applicationUser);
                    _unitOfWork.Save();
                }
            }

            return RedirectToAction("Index");
        }

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            var objUserList = _unitOfWork.applicationUser.GetAll(include: "company").ToList();
            foreach (var user in objUserList)
            {

                user.Role = _userManager.GetRolesAsync(user).GetAwaiter().GetResult().FirstOrDefault();

                if (user.company == null)
                {
                    user.company = new Company()
                    {
                        Name = ""
                    };
                }
            }

            return Json(new { data = objUserList });
        }


        [HttpPost]
        public IActionResult LockUnlock([FromBody] string id)
        {

            var objFromDb = _unitOfWork.applicationUser.Get(i=>i.Id==id);
            if (objFromDb == null)
            {
                return Json(new { success = false, message = "Error while Locking/Unlocking" });
            }

            if (objFromDb.LockoutEnd != null && objFromDb.LockoutEnd > DateTime.Now)
            {
                //user is currently locked and we need to unlock them
                objFromDb.LockoutEnd = DateTime.Now;
            }
            else
            {
                objFromDb.LockoutEnd = DateTime.Now.AddYears(1000);
            }
            _unitOfWork.applicationUser.Update(objFromDb);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Operation Successful" });
        }

        #endregion
    }

  
}