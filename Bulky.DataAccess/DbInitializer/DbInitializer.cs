using Bulky.DataAccess.Data;
using Bulky.Models.Models;
using Bulky.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.DbInitializer
{

    public class DbInitializer : IDbInitializer
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DbInitializer(AppDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public void Initialize()
        {
            //migrations if they are not applied
            try
            {
                if (_context.Database.GetPendingMigrations().Count() > 0)
                {
                    _context.Database.Migrate();

                }
            }
            catch (Exception ex)
            {

                throw;
            }
            IdentityResult result;
            //create roles if they are not created
            if (!_roleManager.RoleExistsAsync(Sd.Role_Customer).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(Sd.Role_Customer)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(Sd.Role_Employee)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(Sd.Role_Admin)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(Sd.Role_Company)).GetAwaiter().GetResult();
                //if roles are not created, then we will create admin user as well
                result= _userManager.CreateAsync(new ApplicationUser
                {
                    UserName = "MuhammedAbdelRaheem293@gmail.com",
                    Email = "MuhammedAbdelRaheem293@gmail.com",
                    name = "Muhammed Abdel Raheem ",
                    PhoneNumber = "01027268605",
                    streetAddress = "303 Gamal Abdel Nasser",
                    state = "Miame",
                    postalCode = "23422",
                    city = "Alexandria"
                }, "P@ssW0rd").GetAwaiter().GetResult();
                if (!result.Succeeded)
                {
                    throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
                }

                ApplicationUser user = _context.AppUsers.FirstOrDefault(u => u.Email == "MuhammedAbdelRaheem293@gmail.com");
                _userManager.AddToRoleAsync(user, Sd.Role_Admin).GetAwaiter().GetResult();
            }

            return;
        }
    }
}
        

