﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace temple.Models.Users
{
    public class LoginUser
    {
        
        public string access_token { get; set; }
        public string refresh_token { get; set; }
        public int expires_in { get; set; }
    }
}
