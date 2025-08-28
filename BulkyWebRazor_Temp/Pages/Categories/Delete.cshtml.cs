using BulkyWebRazor_Temp.Data;
using BulkyWebRazor_Temp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BulkyWebRazor_Temp.Pages.Categories
{
    [BindProperties]
    public class DeleteModel : PageModel
    {
        private readonly AppDbContext _context;
        public Category Category { get; set; }

        public DeleteModel(AppDbContext context)
        {
            _context = context;
        }
        public void OnGet(int? id)
        {
            if (id !=null && id!=0)
            {
                Category = _context.Categories.Find(id);
            }


        }


        public IActionResult OnPost()
        {
           var category= _context.Categories.Find(Category.Id);

            if (category == null)
            {
                return NotFound();

            }

            _context.Categories.Remove(category);
            _context.SaveChanges();
            TempData["success"] = $"Category {category.Name} Deleted Successfully";

            return RedirectToPage(nameof(Index));
        }
    }
}
