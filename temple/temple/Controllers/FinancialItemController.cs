using System;
using System.Collections.Generic;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using temple.Helpers;
using temple.Models;
using temple.Models.FinancialItem;


namespace temple.Controllers
{
    [Authorize]
    [AutoValidateAntiforgeryToken]
    public class FinancialItemController : Controller
    {
        

        private readonly IConfiguration _config;
        private readonly ICallApi _callApi;

        /// <summary>
        /// 讀取組態用
        /// </summary>

        public FinancialItemController(IConfiguration config, ICallApi callApi)
        {
            _config = config;
            _callApi = callApi;
            
        }

        

        /// <summary>
        /// 查詢財務項目資料
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetFinancialItem()
        {
            ResultModel result = new ResultModel();
            Uri address = new Uri(_config["api"]+ "/FinancialItem");
            string targetUri = address.ToString() + "/?";
            QueryViewModel model = new QueryViewModel();

            result = await _callApi.CallAPI(null, new Uri(targetUri), "GET");
            if (result.IsSuccess == true && result.Data != null)
            {
                model.FinancialItems= JsonConvert.DeserializeObject<List<FinancialItem>>(result.Data.ToString());
            }
            if (TempData["IsSuccess"] == null)
            {
                TempData["IsSuccess"] = result.IsSuccess;
                TempData["msg"] = result.Message;
            }
            return View(model);
        }

        ////// <summary>
        ///新增財務項目
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpGet]
        public async Task<IActionResult> CreateFinancialItem()
        {
            
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFinancialItem(FinancialItem item)
        {
            if (!ModelState.IsValid) {
                return View(item);
            }
            var data = JsonConvert.SerializeObject(item);
            Uri address = new Uri(_config["api"] + "/FinancialItem");
            ResultModel result = new ResultModel();
            result = await _callApi.CallAPI(data, address, "POST");

            TempData["IsSuccess"] = result.IsSuccess;
            TempData["msg"] = result.Message;
            if (!result.IsSuccess)
                return View(item);

            return RedirectToAction(nameof(CreateFinancialItem));
            
        }


        /// <summary>
        /// 編輯友宮資料
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            ResultModel result = new ResultModel();
            Uri address = new Uri(_config["api"]+ "/FinancialItem/" + id);
            result = await _callApi.CallAPI("null", address, "GET");

            FinancialItem f = new FinancialItem();
            if (result.IsSuccess == true)
            {

                f = JsonConvert.DeserializeObject<FinancialItem>(result.Data.ToString());
                TempData["isSuccess"] = "true";
                return View(f);
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
        public async Task<IActionResult> Edit(FinancialItem f)
        {
            if (!ModelState.IsValid)
            {
                return View(f);
            }


            var data = JsonConvert.SerializeObject(f);
            Uri address = new Uri(_config["api"]+ "/FinancialItem");
            ResultModel result = new ResultModel();
            result = await _callApi.CallAPI(data, address, "PUT");

            TempData["IsSuccess"] = result.IsSuccess;
            TempData["msg"] = result.Message;
            if (!result.IsSuccess)
                return View(f);
            return RedirectToAction(nameof(GetFinancialItem));
        }


        /// <summary>
        /// 刪除資料
        /// </summary>
        /// <returns></returns>
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ResultModel> Delete(int id)
        {
            Uri address = new Uri(_config["api"]+ "/FinancialItem/" + id);
            ResultModel result = new ResultModel();
            result = await _callApi.CallAPI("null", address, "DELETE");
            return result;
            
        }

    }
}