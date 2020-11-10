using API.Data;
using API.Helpers;
using API.Models;
using API.Models.Users;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Helpers
{
    public class TokenGetUserHelper:ITokenGetUserHelper
    {
        private readonly TempleDbContext _context;
        public  TokenGetUserHelper(TempleDbContext context) {
            _context = context;
        }
        public async Task<User> GetUser(string accessToken) {
            
            var payloadObj = new payload();
            var access_tokenSplit = accessToken.Split(".");
            var payloadBase64 = access_tokenSplit[1];
            payloadBase64 = payloadBase64.PadRight(payloadBase64.Length + (4 - payloadBase64.Length % 4) % 4, '=');
            var payloadText = System.Text.Encoding.GetEncoding("utf-8").GetString(Convert.FromBase64String(payloadBase64));
            payloadObj = JsonConvert.DeserializeObject<payload>(payloadText);
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == payloadObj.nameid); //Get user from database.
            if (user != null)
                return user;
            else
            return user;
            
        }
    }
}
