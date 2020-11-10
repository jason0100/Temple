using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace API.Models.Users
{
    public class Token
    {
        
        public string access_token { get; set; }
        //Refresh Token
        [Required]
        public string refresh_token { get; set; }
        //幾秒過期
        public int expires_in { get; set; }
    }
}
