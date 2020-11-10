using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using temple.Models;
using temple.Models.Upload;

namespace temple.Helpers
{
    public interface Ihttpclient
    {
        Task<ResultModel> CallApi(List<IFormFile> files, string url, string folder);
        
    }
}
