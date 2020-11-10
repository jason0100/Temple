using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using temple.Helpers;

namespace temple.Models
{
    public class DownloadCityList
    {
        private readonly IConfiguration _config;
        private readonly ICallApi _callApi;

        public DownloadCityList(IConfiguration config, ICallApi callApi) {
            _config = config;
            _callApi = callApi;
        }
    
        public async Task< List< City>  >Download()
        {
            ResultModel Cityresult = new ResultModel();
            string address = _config["api"] + "/City";
            Cityresult = await _callApi.CallAPI("", new Uri(address), "GET");
            var cityObj = JsonConvert.DeserializeObject<List<City>>(Cityresult.Data.ToString());
            return cityObj;
        }
        public async Task<IEnumerable<SelectListItem>> DownloadToList()
        {
            ResultModel Cityresult = new ResultModel();
            string address = _config["api"] + "/City";
            Cityresult = await _callApi.CallAPI("", new Uri(address), "GET");
            var cityObj = JsonConvert.DeserializeObject<List<City>>(Cityresult.Data.ToString());
            var CityList = from o in cityObj
                            select new SelectListItem
                            {
                                Value = o.Id.ToString(),
                                Text = o.Name
                            };
            return CityList;
        }
    }
   
}   
    


