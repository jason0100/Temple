using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Nancy.Json;
using Newtonsoft.Json;
using temple.Helpers;
using temple.Models;
using temple.Models.RitualMoneyRecord;

namespace temple.Controllers
{
    [Authorize]
    [AutoValidateAntiforgeryToken]
    public class RitualMoneyController : Controller
    {
   
        private readonly IConfiguration _config;
        private readonly ICallApi _callApi;
        /// <summary>
        /// 讀取組態用
        /// </summary>

        public RitualMoneyController(IConfiguration config, ICallApi callApi)
        {
            _config = config;
            _callApi = callApi;
        }

        

        /// <summary>
        /// 查詢借還金紀錄
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetRitualMoney(QueryViewModel? q)
        {
            var data = JsonConvert.SerializeObject(q);
            Uri address = new Uri(_config["api"]+ "/RitualMoney");

            ResultModel result = new ResultModel();
            

            string targetUri = address.ToString() + "/?";
            if (!string.IsNullOrEmpty(q.Name))
            {
                targetUri = targetUri + "&Name=" + q.Name.Trim();
            }
            if (!string.IsNullOrEmpty(q.Identity))
            {
                targetUri = targetUri + "&Identity=" + q.Identity.Trim();
            }
            if (q.Year!=null)
            {
                targetUri = targetUri + "&Year=" + q.Year;
            }
            if (q.Month!=null)
            {
                targetUri = targetUri + "&Month=" + q.Month;
            }
             
            result = await _callApi.CallAPI(null, new Uri(targetUri), "GET");
            if (result.IsSuccess == true && result.Data!=null)
            {
                q.RitualMoneyRecords = JsonConvert.DeserializeObject<List<RitualMoneyRecord>>(result.Data.ToString());
                foreach (var i in q.RitualMoneyRecords)
                {
                    if (!string.IsNullOrEmpty(i.ReturnDate))
                        i.ReturnDate = i.ReturnDate.Split("T")[0];
                    if (!string.IsNullOrEmpty(i.BorrowDate))
                        i.BorrowDate = i.BorrowDate.Split("T")[0];
                }
            }
            if (TempData["IsSuccess"] == null)
            {
                TempData["IsSuccess"] = result.IsSuccess;
                TempData["msg"] = result.Message;
            }
            return View(q);
        }




        /// <summary>
        /// 借用發財金
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// 
        [HttpGet]
        public async Task<IActionResult> CreateRitualMoney()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRitualMoney(BorrowViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var data = JsonConvert.SerializeObject(model);
            
            Uri address = new Uri(_config["api"]+ "/RitualMoney");
            ResultModel result = new ResultModel();
            result = await _callApi.CallAPI(data, address, "POST");
           
            TempData["IsSuccess"] = result.IsSuccess;
            TempData["msg"] = result.Message;
            if (!result.IsSuccess)
            {
                return View(model);
            }
            return RedirectToAction(nameof(CreateRitualMoney));
        }


        /// <summary>
        /// 發財金還願
        /// </summary>
        /// <returns></returns>

        [HttpGet]
        public async Task<IActionResult> ReturnRitualMoney()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReturnRitualMoney(ReturnViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var data = JsonConvert.SerializeObject(model);
            Uri address = new Uri(_config["api"]+ "/RitualMoney");
            ResultModel result = new ResultModel();
            result = await _callApi.CallAPI(data, address, "PUT");
          
            TempData["IsSuccess"] = result.IsSuccess;
            TempData["msg"] = result.Message;
            if (!result.IsSuccess)
            {
                return View(model);
            }
            return View();
        }


        /// <summary>
        ///修改發財金紀錄
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// 
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Uri address = new Uri(_config["api"]+"/RitualMoney/" + id);
            ResultModel result = new ResultModel();

            result = await _callApi.CallAPI("null", address, "GET");


            if (result.IsSuccess == true)
            {
                DetailViewModel r = new DetailViewModel();
                r = JsonConvert.DeserializeObject<DetailViewModel>(result.Data.ToString());
                if (!string.IsNullOrEmpty(r.Record.ReturnDate))
                    r.Record.ReturnDate = r.Record.ReturnDate.Split("T")[0];
                if (!string.IsNullOrEmpty(r.Record.BorrowDate))
                    r.Record.BorrowDate = r.Record.BorrowDate.Split("T")[0];
                TempData["isSuccess"] = "true";
                return View(r);
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
        public async Task<IActionResult> Edit(DetailViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            EditRecord editRecord = new EditRecord();
            if (model.Record.BorrowAmount != null)
                editRecord.BorrowAmount =Convert.ToInt32( model.Record.BorrowAmount);
            if (model.Record.ReturnAmount != null)
                editRecord.ReturnAmount = Convert.ToInt32( model.Record.ReturnAmount);

            var data = JsonConvert.SerializeObject(editRecord);
            string targetUri = _config["api"] + "/RitualMoney/" + model.Record.RitualMoneyRecordId;
            Uri address = new Uri(targetUri);
            
            ResultModel result = new ResultModel();
            result = await _callApi.CallAPI(data, address, "PUT");

            TempData["isSuccess"] = result.IsSuccess;
            TempData["msg"] = result.Message;

            return RedirectToAction(nameof(GetRitualMoney));
        }



        /// <summary>
        /// 刪除借還款紀錄
        /// 先跳到確認畫面Get資料
        /// /// 路由資料
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> DeleteRitualMoney(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Uri address = new Uri(_config["api"]+ "/RitualMoney/" + id);
            ResultModel result = new ResultModel();

            result = await _callApi.CallAPI("null", address, "GET");


            if (result.IsSuccess == true)
            {
                DetailViewModel r = new DetailViewModel();
                r = JsonConvert.DeserializeObject<DetailViewModel>(result.Data.ToString());
                TempData["isSuccess"] = "true";
                return View(r);
            }
            else
            { //result.IsSuccess == false
                TempData["isSuccess"] = "false";
                TempData["msg"] = "RitualMoneyRecordId not exist.";
                return View();
            }


        }

        [HttpPost, ActionName("DeleteRitualMoney")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            Uri address = new Uri(_config["api"]+ "/RitualMoney/" + id);
            ResultModel result = new ResultModel();
            result = await _callApi.CallAPI("null", address, "DELETE");
            TempData["IsSuccess"] = result.IsSuccess;
            TempData["msg"] = result.Message;
            if (result.IsSuccess == false)
                return View();
            else
                return RedirectToAction(nameof(GetRitualMoney));
        }

        /// <summary>
        /// 匯出Excel
        /// 及時產生filestream串流不存檔
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<FileStreamResult> ExportExcel(QueryViewModel q)
        {
            ResultModel result = new ResultModel();
            //var data = JsonConvert.SerializeObject(q);
            Uri address = new Uri(_config["api"]+ "/RitualMoney");

            string targetUri = address.ToString() + "/?";
            if (!string.IsNullOrEmpty(q.Name))
            {
                targetUri = targetUri + "&Name=" + q.Name.Trim();
            }
            if (!string.IsNullOrEmpty(q.Identity))
            {
                targetUri = targetUri + "&Identity=" + q.Identity.Trim();
            }
            if (q.Year != null)
            {
                targetUri = targetUri + "&Year=" + q.Year;
            }
            if (q.Month != null)
            {
                targetUri = targetUri + "&Month=" + q.Month;
            }

            result = await _callApi.CallAPI(null, new Uri(targetUri), "GET");
            if (result.IsSuccess == true && result.Data != null)
            {
                q.RitualMoneyRecords = JsonConvert.DeserializeObject<List<RitualMoneyRecord>>(result.Data.ToString());
                var memoryStream = new MemoryStream();

                using (var document = SpreadsheetDocument.Create(memoryStream, SpreadsheetDocumentType.Workbook))
                {
                    var workbookPart = document.AddWorkbookPart();
                    workbookPart.Workbook = new Workbook();

                    var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                    worksheetPart.Worksheet = new Worksheet(new SheetData());

                    var sheets = workbookPart.Workbook.AppendChild(new Sheets());

                    sheets.Append(new Sheet()
                    {
                        Id = workbookPart.GetIdOfPart(worksheetPart),
                        SheetId = 1,
                        Name = "Sheet 1"
                    });

                    var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

                    var row = new Row();
                    row.Append(
                         new Cell()
                         {
                             CellValue = new CellValue(""),
                             DataType = CellValues.String
                         },
                        new Cell()
                        {
                            CellValue = new CellValue("發財金編號"),
                            DataType = CellValues.String
                        },
                        new Cell()
                        {
                            CellValue = new CellValue("姓名"),
                            DataType = CellValues.String
                        },
                        new Cell()
                        {
                            CellValue = new CellValue("身分證"),
                            DataType = CellValues.String
                        },
                         new Cell()
                         {
                             CellValue = new CellValue("借用日期"),
                             DataType = CellValues.String
                         }, new Cell()
                         {
                             CellValue = new CellValue("借用金額"),
                             DataType = CellValues.String
                         }, new Cell()
                         {
                             CellValue = new CellValue("還願日期"),
                             DataType = CellValues.String
                         }, new Cell()
                         {
                             CellValue = new CellValue("還願金額"),
                             DataType = CellValues.String
                         }
                         , new Cell()
                         {
                             CellValue = new CellValue("備註"),
                             DataType = CellValues.String
                         }
                    );
                    sheetData.AppendChild(row);

                    for (var i = 0; i < q.RitualMoneyRecords.Count(); i++)
                    {

                        row = new Row();
                        row.Append(
                            new Cell()
                            {
                                CellValue = new CellValue((i + 1).ToString()),
                                DataType = CellValues.Number
                            },
                            new Cell()
                            {
                                CellValue = new CellValue(q.RitualMoneyRecords[i].RitualMoneyRecordId.ToString()),
                                DataType = CellValues.String
                            },
                            new Cell()
                            {
                                CellValue = new CellValue(q.RitualMoneyRecords[i].Member.Name),
                                DataType = CellValues.String
                            }
                            ,
                            new Cell()
                            {
                                CellValue = new CellValue(q.RitualMoneyRecords[i].Member.Identity),
                                DataType = CellValues.String
                            }
                            ,
                            new Cell()
                            {
                                CellValue = new CellValue(q.RitualMoneyRecords[i].BorrowDate),
                                DataType = CellValues.String
                            }
                            ,
                            new Cell()
                            {
                                CellValue = new CellValue(q.RitualMoneyRecords[i].BorrowAmount.ToString()),
                                DataType = CellValues.String
                            }
                            ,
                            new Cell()
                            {
                                CellValue = new CellValue(q.RitualMoneyRecords[i].ReturnDate),
                                DataType = CellValues.String
                            }
                            ,
                            new Cell()
                            {
                                CellValue = new CellValue(q.RitualMoneyRecords[i].ReturnAmount.ToString()),
                                DataType = CellValues.String
                            }
                            ,
                            new Cell()
                            {
                                CellValue = new CellValue((q.RitualMoneyRecords[i].IsReturn) ? "已還願" : "未還願"),
                                DataType = CellValues.String
                            }
                        );
                        sheetData.AppendChild(row);
                    }
                }

                memoryStream.Seek(0, SeekOrigin.Begin);
                return new FileStreamResult(memoryStream,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

            }
            return null;
            //result.Data = q.RitualMoneyRecords;
            //return result;
        }





    }
}
