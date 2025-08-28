using BulkyWebRazor_Temp.Data;
using BulkyWebRazor_Temp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BulkyWebRazor_Temp.Pages.Categories
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;
        public List<Category> CategoryList { get; set; }
       
        public IndexModel(AppDbContext context)
        {
            _context = context;
        }
        public void OnGet()
        {
            CategoryList = _context.Categories.ToList();

        }
    }
}
