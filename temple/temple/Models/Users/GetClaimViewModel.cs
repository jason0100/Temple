using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace temple.Models.Users
{
    public class GetClaimViewModel
    {
        public string access_token { get; set; }
        public IEnumerable<Claim> claims { get; set; }
    }
}
