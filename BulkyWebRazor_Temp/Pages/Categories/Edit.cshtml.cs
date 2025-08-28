using BulkyWebRazor_Temp.Data;
using BulkyWebRazor_Temp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BulkyWebRazor_Temp.Pages.Categories
{
    [BindProperties]
    public class EditModel : PageModel
    {
        private readonly AppDbContext _context;
        public Category? Category { get; set; }

        public EditModel(AppDbContext context)
        {
            _context = context;
        }
        public void OnGet(int? id)
        {
            if (id != null && id != 0)
            {
                Category = _context.Categories.Find(id);
            }
        }


        public IActionResult OnPost()
        {
            if (ModelState.IsValid)
            {
                _context.Categories.Update(Category);
                _context.SaveChanges();
                TempData["success"] = $"Category {Category.Name} Updated Successfully";

                return RedirectToPage(nameof(Index));
            }
            return Page();
        }
    }
}
