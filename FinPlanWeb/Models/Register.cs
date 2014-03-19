using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Globalization;

namespace FinPlanWeb.Models
{
    public class Register
    {
        [Required]
        [Display(Name = "Username")]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email Address")]
        [RegularExpression(@"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*", ErrorMessage = "Email have to be in YourEmail@Domain.com.")]
        public string EmailAddress { get; set; }
        [Required(ErrorMessage = "The {0} field is required!")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "RetypePassword")]
        [Compare("Password", ErrorMessage = "The both password do not matced.")]
        public string ConfirmPassword { get; set; }
    }
}