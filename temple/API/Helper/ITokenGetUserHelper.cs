using API.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Helpers
{
    public interface ITokenGetUserHelper
    {
           Task< User >GetUser(string accessToken);
    
    }
}
