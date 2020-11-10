using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nancy.Json;
using Newtonsoft.Json;
using temple.Models;
using temple.Models.FriendsController;
using ExtensionMethods;
using temple.Helpers;

namespace temple.Controllers
{
    [Authorize]
    [AutoValidateAntiforgeryToken]
    public class FriendsController : Controller
    {
        
        private readonly IConfiguration _config;
        private readonly ICallApi _callApi;
        /// <summary>
        /// 讀取組態用
        /// </summary>

        public FriendsController(IConfiguration config, ICallApi callApi)
        {
            _config = config;
            _callApi = callApi;
        }

       
        /// <summary>
        /// 查詢友宮資料
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetFriend(QueryViewModel model)
        {
            ResultModel result = new ResultModel();
            string address = _config["api"]+ "/Friends";
            address += model.GenerateQueryString();
                          
            result = await _callApi.CallAPI(null, new Uri(address), "GET");
            if (result.IsSuccess == true && result.Data != null)
            {
                model.Friends = JsonConvert.DeserializeObject<List<Friend>>(result.Data.ToString());
            }
            if (TempData["IsSuccess"] == null)
            {
                TempData["IsSuccess"] = result.IsSuccess;
                TempData["msg"] = result.Message;
            }
            return View(model);
        }


        /// <summary>
        /// 新增友宮資料
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task< IActionResult > CreateFriend()
        {
            PostViewModel vm = new PostViewModel();
            //下載cityList
            var downloadCityList = new DownloadCityList(_config,_callApi);
            vm.CityList = await downloadCityList.DownloadToList();
         
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFriend(PostViewModel vm)
        {
            
            ResultModel Cityresult = new ResultModel();
            //下載cityList
            var downloadCityList = new DownloadCityList(_config, _callApi);
            vm.CityList = await downloadCityList.DownloadToList();
            var cityObj = await downloadCityList.Download();

            if (vm.friend.CityId != null) {
                var selectCity = cityObj.SingleOrDefault(c => c.Id == vm.friend.CityId);
                vm.TownShipList = from o in selectCity.TownShips
                                  select new SelectListItem
                                  {
                                      Value = o.Id.ToString(),
                                      Text = o.Name

                                  };
                if (vm.friend.TownshipId != null)
                {
                    var selectTownShip = selectCity.TownShips.SingleOrDefault(t => t.Id == vm.friend.TownshipId);
                    vm.friend.Zip = selectTownShip.Zip;
                  
                }
            }
            //END下載cityList
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var data = JsonConvert.SerializeObject(vm.friend);
            Uri address = new Uri(_config["api"]+ "/Friends");
            ResultModel result = new ResultModel();
            result = await _callApi.CallAPI(data, address, "POST");

            TempData["IsSuccess"] = result.IsSuccess;
            TempData["msg"] = result.Message;
            if (!result.IsSuccess)
                return View(vm);
            return RedirectToAction(nameof(CreateFriend));
        }


        /// <summary>
        /// 編輯友宮資料
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> EditFriend(int id)
        {
            ResultModel result = new ResultModel();
            Uri address = new Uri(_config["api"]+ "/Friends/" + id);
            result = await _callApi.CallAPI("null", address, "GET");

          
            PostViewModel vm = new PostViewModel();
            if (result.IsSuccess == true)
            {

                vm.friend = JsonConvert.DeserializeObject<Friend>(result.Data.ToString());
                TempData["isSuccess"] = "true";

                if (result.IsSuccess)//下載cityList
                {
                    ResultModel Cityresult = new ResultModel();
                    //下載cityList
                    var downloadCityList = new DownloadCityList(_config, _callApi);
                    vm.CityList = await downloadCityList.DownloadToList();
                    var cityObj = await downloadCityList.Download();
                   
                    var selectCity = cityObj.SingleOrDefault(c => c.Id == vm.friend.CityId);
                    var selectTownShip = selectCity.TownShips.SingleOrDefault(t => t.Id == vm.friend.TownshipId);
                    vm.friend.Zip = selectTownShip.Zip;
                    vm.TownShipList = from o in selectCity.TownShips
                                       select new SelectListItem
                                        {
                                            Value = o.Id.ToString(),
                                            Text = o.Name
                                        };
                   
                }

                return View(vm);
            }
            else
            { //result.IsSuccess == false
                TempData["isSuccess"] = "false";
                TempData["msg"] = result.Message;
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditFriend(PostViewModel vm)
        {

            //下載cityList
            var downloadCityList = new DownloadCityList(_config, _callApi);
            vm.CityList = await downloadCityList.DownloadToList();
            var cityObj = await downloadCityList.Download();

            var selectCity = cityObj.SingleOrDefault(c => c.Id == vm.friend.CityId);
            var selectTownShip = selectCity.TownShips.SingleOrDefault(t => t.Id == vm.friend.TownshipId);
            vm.friend.Zip = selectTownShip.Zip;
            vm.TownShipList = from o in selectCity.TownShips
                                select new SelectListItem
                                {
                                    Value = o.Id.ToString(),
                                    Text = o.Name
                                };
            //END下載cityList

            if (!ModelState.IsValid)
            {
                return View(vm);
            }
            

            var data = JsonConvert.SerializeObject(vm.friend);
            Uri address = new Uri(_config["api"]+ "/Friends");
            ResultModel result = new ResultModel();
            result = await _callApi.CallAPI(data, address, "PUT");
            
            TempData["IsSuccess"] = result.IsSuccess;
            TempData["msg"] = result.Message;
            if (!result.IsSuccess)
                return View(vm);
            return RedirectToAction(nameof(GetFriend));
        }


        /// <summary>
        /// 刪除友宮資料
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task< IActionResult > DeleteFriend(int id)
        {
            Uri address = new Uri(_config["api"]+ "/Friends/" + id);
            ResultModel result = new ResultModel();

            result = await _callApi.CallAPI("null", address, "GET");
            Friend r = new Friend();

            if (result.IsSuccess == true)
            {
                r = JsonConvert.DeserializeObject<Friend>(result.Data.ToString());
                TempData["isSuccess"] = "true";
            }
            else
            { //result.IsSuccess == false
                TempData["isSuccess"] = "false";
                TempData["msg"] = "Friend Temple not exist.";
              
            }
            return View(r);
        }

        [HttpPost, ActionName("DeleteFriend")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            Uri address = new Uri(_config["api"]+ "/Friends/" + id);
            ResultModel result = new ResultModel();
            result = await _callApi.CallAPI("null", address, "DELETE");
            TempData["IsSuccess"] = result.IsSuccess;
            TempData["msg"] = result.Message;
            if (result.IsSuccess == false)
                return View();
            else
                return RedirectToAction(nameof(GetFriend));
        }


        /// <summary>
        /// 載入CityList
        /// </summary>
        /// <returns></returns>
        private readonly List<City> cities = new List<City>();
        public async Task<ResultModel> CityList() {
            ResultModel result = new ResultModel();
            Uri address = new Uri(_config["api"]+ "/City");
            result = await _callApi.CallAPI("", address,"GET");
            if (result.IsSuccess) {
                result.Message = "Get cities list success";

                cities.AddRange(JsonConvert.DeserializeObject<List<City>>(result.Data.ToString()));
            }
            result.Data = JsonConvert.SerializeObject(result.Data);
          
            return result;
        }

        [HttpGet]
        public async Task<IActionResult> test(int id)
        {
            return View();
        }
    }
}