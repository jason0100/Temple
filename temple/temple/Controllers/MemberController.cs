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
using temple.Models.MemberData;
using ExtensionMethods;
using temple.Helpers;

namespace temple.Controllers
{
    [Authorize]
    [AutoValidateAntiforgeryToken]
    public class MemberController : Controller
    {
        private readonly ILogger<MemberController> _logger;
        private readonly IConfiguration _config;
        private readonly ICallApi _callApi;
    
        public MemberController(ILogger<MemberController> logger, IConfiguration config, ICallApi callApi)
        {
            _logger = logger;
            _config = config;
            _callApi = callApi;
        }



        /// <summary>
        /// 新增信徒資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task< IActionResult >CreateMember()
        {
            PostViewModel vm = new PostViewModel();
            //下載cityList
            ResultModel Cityresult = new ResultModel();
            Uri address1 = new Uri(_config["api"]+ "/City");
            Cityresult = await _callApi.CallAPI("", address1, "GET");
            var cityObj = JsonConvert.DeserializeObject<List<City>>(Cityresult.Data.ToString());
            vm.CityList = from o in cityObj
                          select new SelectListItem
                          {
                              Value = o.Id.ToString(),
                              Text = o.Name
                          };
            
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMember(PostViewModel vm)
        {

            //下載cityList
            ResultModel Cityresult = new ResultModel();
            Uri address1 = new Uri(_config["api"]+ "/City");
            Cityresult = await _callApi.CallAPI("", address1, "GET");
            var cityObj = JsonConvert.DeserializeObject<List<City>>(Cityresult.Data.ToString());
            vm.CityList = from o in cityObj
                          select new SelectListItem
                          {
                              Value = o.Id.ToString(),
                              Text = o.Name
                          };
            if (vm.member.CityId != null)
            {
                var selectCity = cityObj.SingleOrDefault(c => c.Id == vm.member.CityId);
                vm.TownShipList = from o in selectCity.TownShips
                                  select new SelectListItem
                                  {
                                      Value = o.Id.ToString(),
                                      Text = o.Name

                                  };
                if (vm.member.TownshipId != null)
                {
                    var selectTownShip = selectCity.TownShips.SingleOrDefault(t => t.Id == vm.member.TownshipId);
                    vm.member.Zip = selectTownShip.Zip;
                   
                }
            }
            //END下載cityList
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var data = JsonConvert.SerializeObject(vm.member);
            Uri address = new Uri(_config["api"]+ "/member");
            ResultModel result = new ResultModel();
            result = await _callApi.CallAPI(data, address, "POST");

            TempData["IsSuccess"] = result.IsSuccess;
            TempData["msg"] = result.Message;
            if (!result.IsSuccess)
                return View(vm);
            return RedirectToAction(nameof(CreateMember));
        }

        /// <summary>
        /// 查詢信徒資料
        /// </summary>
        /// <returns></returns>
        /// 

        [HttpGet]
        public async Task<IActionResult> GetMember(QueryMemberViewModel q)
        {
            if (!ModelState.IsValid)
            {
                return View(q);
            }

            var data = JsonConvert.SerializeObject(q);
            
            string targetUri = _config["api"] + "/member";
            targetUri += q.GenerateQueryString();

            q.ResultModel = await _callApi.CallAPI(data, new Uri(targetUri), "GET");
            if (q.ResultModel.IsSuccess == true)
            {
                q.members = JsonConvert.DeserializeObject<List<member>>(q.ResultModel.Data.ToString());
            }
            if (TempData["IsSuccess"] == null) {
                TempData["IsSuccess"] = q.ResultModel.IsSuccess;
                TempData["msg"] = q.ResultModel.Message;
            }
           
            return View(q);

        }

        /// <summary>
        /// 編輯會員資料GET
        /// 路由資料
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            PostViewModel vm = new PostViewModel();
            if (id == null)
            {
                return NotFound();
            }
            Uri address = new Uri(_config["api"]+ "/member/" + id);
            ResultModel result = new ResultModel();
            result = await _callApi.CallAPI("null", address, "GET");

            if (result.IsSuccess == true)
            {
                vm.member = JsonConvert.DeserializeObject<member>(result.Data.ToString());

            }

           
            //下載cityList
            ResultModel Cityresult = new ResultModel();
            Uri address1 = new Uri(_config["api"]+ "/City");
            Cityresult = await _callApi.CallAPI("", address1, "GET");
            var cityObj = JsonConvert.DeserializeObject<List<City>>(Cityresult.Data.ToString());
            vm.CityList = from o in cityObj
                          select new SelectListItem
                          {
                              Value = o.Id.ToString(),
                              Text = o.Name
                          };
            if (vm.member.CityId != null)
            {
                var selectCity = cityObj.SingleOrDefault(c => c.Id == vm.member.CityId);
                vm.TownShipList = from o in selectCity.TownShips
                                  select new SelectListItem
                                  {
                                      Value = o.Id.ToString(),
                                      Text = o.Name

                                  };
                if (vm.member.TownshipId != null)
                {
                    var selectTownShip = selectCity.TownShips.SingleOrDefault(t => t.Id == vm.member.TownshipId);
                    vm.member.Zip = selectTownShip.Zip;

                }
            }
            //END下載cityList

            

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PostViewModel vm)
        {
            //下載cityList
            ResultModel Cityresult = new ResultModel();
            Uri address1 = new Uri(_config["api"]+ "/City");
            Cityresult = await _callApi.CallAPI("", address1, "GET");
            var cityObj = JsonConvert.DeserializeObject<List<City>>(Cityresult.Data.ToString());
            vm.CityList = from o in cityObj
                          select new SelectListItem
                          {
                              Value = o.Id.ToString(),
                              Text = o.Name
                          };
            if (vm.member.CityId != null)
            {
                var selectCity = cityObj.SingleOrDefault(c => c.Id == vm.member.CityId);
                vm.TownShipList = from o in selectCity.TownShips
                                  select new SelectListItem
                                  {
                                      Value = o.Id.ToString(),
                                      Text = o.Name

                                  };
                if (vm.member.TownshipId != null)
                {
                    var selectTownShip = selectCity.TownShips.SingleOrDefault(t => t.Id == vm.member.TownshipId);
                    vm.member.Zip = selectTownShip.Zip;

                }
            }
            //END下載cityList
            if (!ModelState.IsValid) {
                return View(vm);
            }
            var result = new ResultModel();
            var data = JsonConvert.SerializeObject(vm.member);
            Uri address = new Uri(_config["api"]+ "/member");
            result = await _callApi.CallAPI(data, address, "PUT");

            TempData["IsSuccess"] = result.IsSuccess;
            TempData["msg"] = result.Message;

            if (result.IsSuccess == true) {
                return RedirectToAction(nameof(GetMember));
            }
            else
            {
                return View(vm);
            }
            
            
        }



        /// <summary>
        /// 刪除會員-
        /// 先跳到確認畫面Get資料
        /// /// 路由資料
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> DeleteMember(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Uri address = new Uri(_config["api"]+ "/member/"+id);
            ResultModel result = new ResultModel();

            result = await _callApi.CallAPI("null", address, "GET");


            if (result.IsSuccess == true)
            {
                member m = new member();
                m = JsonConvert.DeserializeObject<member>(result.Data.ToString());
                return View(m);
            }
            else { //result.IsSuccess == false
                TempData["msg"] = "MemberId not exist.";
                return View();
            }

            
        }

        [HttpPost, ActionName("DeleteMember")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            Uri address = new Uri(_config["api"]+ "/member/" + id);
            ResultModel result = new ResultModel();
            result = await _callApi.CallAPI("null", address, "DELETE");
            TempData["IsSuccess"] = result.IsSuccess;
            TempData["msg"] = result.Message;
            if (result.IsSuccess == false)
                return View();
           else 
                return RedirectToAction(nameof(GetMember));
        }



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
