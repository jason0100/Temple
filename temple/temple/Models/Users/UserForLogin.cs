using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace temple.Models.Users
{
    public class UserForLogin 
    {
        [Required]
        public string Username { get; set; }

        [Required]
         public string Password { get; set; }

        public bool keepLogin { get; set; }
    }


}
