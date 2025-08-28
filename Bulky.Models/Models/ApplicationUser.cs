using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.Models.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string name { get; set; }    
        public  string? streetAddress { get; set; }
        public string? city { get; set; }
        public string? state { get; set; }
        public string? postalCode { get; set; }
    }
}
