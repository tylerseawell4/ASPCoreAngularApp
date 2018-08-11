using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SellingApp.API.DTOs
{
    public class UserForRegisterDTO
    {
        [Required]
        public string Username { get; set; }

        [Required]
        [StringLength(8, MinimumLength =4, ErrorMessage = "Passwords needs to be between 4-8 characters")]
        public string Password { get; set; }
    }
}
