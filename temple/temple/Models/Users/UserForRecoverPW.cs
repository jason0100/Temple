using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace temple.Models.Users
{
    public class UserForRecoverPW
    {
        public int id { get; set; }
        public string username { get; set; }
        public string token { get;set; }
        public string newPassword { get; set; }
        public string ConfirmNewPassword { get; set; }
    }
}
