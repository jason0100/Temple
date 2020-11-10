using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace temple.Models
{
    public interface IDownloadCityList
    {
        public Task<ResultModel> Download();
    }
}
