using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace API.Models.Users
{
    public class User
    {
        public int Id { get; set; }
        
        public string UserName { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        //Refresh Token
        public string refresh_token { get; set; }
        public string guid { get; set; }
    }
}
