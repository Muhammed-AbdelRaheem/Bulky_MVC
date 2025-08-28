using BulkyWebRazor_Temp.Data;
using BulkyWebRazor_Temp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BulkyWebRazor_Temp.Pages.Categories
{
    [BindProperties]
    public class CreateModel : PageModel
    {
        private readonly AppDbContext _context;
        public Category Category { get; set; }

        public CreateModel(AppDbContext context)
        {
            _context = context;
        }
        public void OnGet()
        {
        }


        public IActionResult OnPost()
        {
            _context.Add(Category);
            _context.SaveChanges();
            TempData["success"] = $"Category {Category.Name} Created Successfully";
            return RedirectToPage("Index");
        }
    }
}
