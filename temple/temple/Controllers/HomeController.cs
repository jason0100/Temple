using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using temple.Helpers;
using temple.Models;
using temple.Models.FinancialRecord;
using temple.Models.ToDoList;

namespace temple.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {

        private readonly IConfiguration _config;
        private readonly ICallApi _callApi;
        /// <summary>
        /// 讀取組態用
        /// </summary>

        public HomeController(IConfiguration config, ICallApi callApi)
        {
            _config = config;
            _callApi = callApi;
        }





        public async Task<IActionResult> Index()
        {
            ResultModel result = new ResultModel();
            Uri address = new Uri(_config["api"] + "/ToDoList");
            string targetUri = address.ToString() + "/?";

            //下載todolist
            result = await _callApi.CallAPI(null, new Uri(targetUri), "GET");
            if (result.IsSuccess == true && result.Data != null)
            {
                ViewBag.ToDoList = JsonConvert.DeserializeObject<List<ToDoListItem>>(result.Data.ToString());
            }

            //下載快到期光明燈
            targetUri = _config["api"] + "/FinancialRecord/GetToBeExpired";
            result = await _callApi.CallAPI(null, new Uri(targetUri), "GET");
            if (result.IsSuccess == true && result.Data != null)
            {
                var query = JsonConvert.DeserializeObject<List<FinancialRecord>>(result.Data.ToString());
                foreach (var i in query)
                {
                    if (i.DueDate != null)
                        i.DueDate = Convert.ToDateTime(i.DueDate).ToString("yyyy-MM-dd");
                }
                ViewBag.ToBeExpired = query;
            }


            return View();
        }

        [HttpPost]
        public async Task<ResultModel> CreateItem(ToDoListItem model)
        {
            ResultModel result = new ResultModel();
            Uri address = new Uri(_config["api"] + "/ToDoList");
            string targetUri = address.ToString() + "/";
            var data = JsonConvert.SerializeObject(model);
            result = await _callApi.CallAPI(data, new Uri(targetUri), "POST");
            TempData["IsSuccess"] = result.IsSuccess;
            TempData["msg"] = result.Message;
            result.Data = JsonConvert.SerializeObject(result.Data);
            return result;

        }
        public IActionResult Error()
        {
            return View();
        }


        [HttpPost]
        public async Task<ResultModel> ChangeToDoListItem(ToDoListItemForEdit model)
        {
            ResultModel result = new ResultModel();
            Uri address = new Uri(_config["api"] + "/ToDoList");
            string targetUri = address.ToString() + "/?";
            var data = JsonConvert.SerializeObject(model);
            result = await _callApi.CallAPI(data, new Uri(targetUri), "PUT");
            return result;
        }

        [HttpPost]
        public async Task<ResultModel> DeleteToDoListItem(int id)
        {
            ResultModel result = new ResultModel();
            Uri address = new Uri(_config["api"] + "/ToDoList");
            string targetUri = address.ToString() + "/" + id;

            result = await _callApi.CallAPI(null, new Uri(targetUri), "DELETE");
            return result;
        }

        //forNotification
        [HttpPost]
        public async Task<ResultModel> GetNotifyCountForAjax()
        {
            ResultModel result = new ResultModel();
            Uri address = new Uri(_config["api"] + "/Notify/count");
            string targetUri = address.ToString();

            result = await _callApi.CallAPI(null, new Uri(targetUri), "GET");
            return result;
        }

        [HttpPost]
        public async Task<ResultModel> GetNotifyDataForAjax()
        {
            ResultModel result = new ResultModel();
            Uri address = new Uri(_config["api"] + "/Notify/GetData");
            string targetUri = address.ToString();

            result = await _callApi.CallAPI(null, new Uri(targetUri), "GET");
            result.Data = JsonConvert.SerializeObject(result.Data);
            return result;
        }

        [HttpPost]
        public async Task<ResultModel> ReadNotifyDataForAjax()
        {
            ResultModel result = new ResultModel();
            Uri address = new Uri(_config["api"] + "/Notify/SetRead");
            string targetUri = address.ToString();

            result = await _callApi.CallAPI(null, new Uri(targetUri), "GET");
            return result;
        }




    }
}