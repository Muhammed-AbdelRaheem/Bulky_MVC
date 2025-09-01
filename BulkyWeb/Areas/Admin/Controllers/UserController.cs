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
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AppDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        public UserController(UserManager<IdentityUser> userManager, AppDbContext context, RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult RoleManagment(string userId)
        {
            var roleId = _context.UserRoles.FirstOrDefault(u => u.UserId == userId).RoleId;

           RoleManagmentVM RoleVM = new RoleManagmentVM()
            {
               ApplicationUser = _context.AppUsers.FirstOrDefault(u => u.Id == userId),
               RoleList = _roleManager.Roles.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Name
                }),
                CompanyList = _context.Companies.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                }),
            };

            RoleVM.ApplicationUser.Role = _context.Roles.FirstOrDefault(u => u.Id == roleId).Name;
            return View(RoleVM);
        }

        [HttpPost]
        public IActionResult RoleManagment(RoleManagmentVM roleManagmentVM)
        {

            string roleId = _context.UserRoles.FirstOrDefault(u => u.UserId == roleManagmentVM.ApplicationUser.Id).RoleId;
            string oldRole = _context.Roles.FirstOrDefault(u => u.Id == roleId).Name;




            if (!(roleManagmentVM.ApplicationUser.Role == oldRole))
            {


            ApplicationUser applicationUser = _context.AppUsers.FirstOrDefault(u => u.Id == roleManagmentVM.ApplicationUser.Id);
                //a role was updated
                if (roleManagmentVM.ApplicationUser.Role == Sd.Role_Company)
                {

                    applicationUser.companyId = roleManagmentVM.ApplicationUser.companyId;
                }
                if (oldRole == Sd.Role_Company)
                {
                    applicationUser.companyId = null;
                }
                _context.SaveChanges();

                _userManager.RemoveFromRoleAsync(applicationUser, oldRole).GetAwaiter().GetResult();
                _userManager.AddToRoleAsync(applicationUser, roleManagmentVM.ApplicationUser.Role).GetAwaiter().GetResult();

            }
          

            return RedirectToAction("Index");
        }


        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            var objUserList = _context.AppUsers.Include(u => u.company).ToList();
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

            var objFromDb = _context.AppUsers.FirstOrDefault(i=>i.Id==id);
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
            _context.AppUsers.Update(objFromDb);
            _context.SaveChanges();
            return Json(new { success = true, message = "Operation Successful" });
        }

        #endregion
    }

  
}