using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using temple.Models;

namespace temple.Helpers
{
    public interface ICallApi
    {
        Task<ResultModel> CallAPI(string data, Uri url, string method);
        Task<ResultModel> RefreshMyToken();
    }
}
