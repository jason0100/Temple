using API.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Dtos
{
    public class LoginUser
    {
        public int Id { get; set; }

        public string UserName { get; set; }
  
        public Token token { get; set; }
    }
}
