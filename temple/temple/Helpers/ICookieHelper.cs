using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using temple.Models;

namespace temple.Helpers
{
    public interface ICookieHelper
    {
        
        public ResultModel Set(string key, string value, CookieOptions options = null);
        public ResultModel Get(string key);
        public ResultModel Remove(string key);
        public string Encrypt(string value);
        public ResultModel Decrypt(string CipherText);
    }
}
