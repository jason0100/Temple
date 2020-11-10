using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace API.Dtos
{
    public class UserForChangePassword
    {
        public string Username { get; set; }
        [StringLength(50, MinimumLength = 6)]
        public string Password { get; set; }
        public string NewPassword { get; set; }
    }
}
