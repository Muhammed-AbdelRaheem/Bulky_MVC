using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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

        public int? companyId { get; set; }
        [ForeignKey("companyId")]
        [ValidateNever]
        public Company company { get; set; }
    }
}
