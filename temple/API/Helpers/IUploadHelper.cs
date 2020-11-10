using API.Models;
using API.Models.Upload;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Helpers
{
    public interface IUploadHelper
    {

        ResultModel UploadData(uploadFile uploadData);
    }
}
