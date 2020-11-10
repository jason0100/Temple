using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using temple.Helpers;

namespace temple.Controllers
{
    public class UploadController : Controller
    {
        IWebHostEnvironment _env;
        private readonly IConfiguration _config;
        private readonly Ihttpclient _httpclient;
        public UploadController(IWebHostEnvironment env, IConfiguration config, Ihttpclient httpclient)
        {
            _env = env;
            _config = config;
            _httpclient = httpclient;

        }
        [HttpPost]
        public async Task <IActionResult> UploadImage(List<IFormFile> files)
        {
            if (files.Count() <= 0) return null;
            

            var url = _config["api"] + "/Upload/photo" ;
            var uploadFileResult = await _httpclient.CallApi(files, url, "app_photos");

            return Json(uploadFileResult);
        }
    }
}