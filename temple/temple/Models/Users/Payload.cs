using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace temple.Models.Users
{
    public class Payload
    {
        //使用者資訊
        public int nameid { get; set; }
        public string unique_name { get; set; }
        //過期時間
        public int exp { get; set; }
    }
}
