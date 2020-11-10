using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.Helpers;
using API.Models;
using API.Models.Upload;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {

        private readonly IConfiguration _config;
        private TempleDbContext _context;
        private readonly IUploadHelper _uploadHelper;
        /// <summary>
        /// 讀取組態用
        /// </summary>

        public UploadController(IConfiguration config, TempleDbContext context, IUploadHelper uploadHelper)
        {
            _config = config;
            _context = context;
            _uploadHelper = uploadHelper;
        }


        /// <summary>
        /// user 照片上傳
        /// </summary>
        /// <remarks>
        ///     Sample request:
        ///
        ///     POST /Upload/upload_photo
        ///     {
        ///        "file": xxx.jpg
        ///     }
        ///
        /// </remarks>
        /// <returns></returns>
        [Produces("application/json")]
        [HttpPost("photo")]
        public async Task<ResultModel> UploadPhoto([FromForm] uploadPhoto data)
        {
            var result = new ResultModel();
            var uploadFile = new uploadFile();
            uploadFile.files = data.files;
            //uploadFile.folder = _config["userPhotoUploadFolder"];
            uploadFile.folder = data.folder;
            var uploadResult = _uploadHelper.UploadData(uploadFile);
            return uploadResult;
        }

    }
}