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
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using temple.Models;
using temple.Models.Transfer;
using ExtensionMethods;
using temple.Helpers;

namespace temple.Controllers
{
    [Authorize]
    [AutoValidateAntiforgeryToken]
    public class TransferController : Controller
    {
        //public static readonly Uri apiUrl = new Uri("http://localhost:5001/Transfer");
        private readonly IConfiguration _config;
        private readonly ICallApi _callApi;
       

        /// <summary>
        /// 讀取組態用
        /// </summary>

        public TransferController(IConfiguration config, ICallApi callApi)
        {
            _config = config;
            _callApi = callApi;
            //_checkToken = checkToken;
        }

       
        ////// <summary>
        ///新增轉帳紀錄
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpGet]
        public async Task<IActionResult> CreateTransfer()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTransfer(TransferRecord model)
        {

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var data = JsonConvert.SerializeObject(model);

            ResultModel result = new ResultModel();
            result = await _callApi.CallAPI(data, new Uri(_config["api"] + "/Transfer"), "POST");

            if (TempData["IsSuccess"] == null)
            {
                TempData["IsSuccess"] = result.IsSuccess;
                TempData["msg"] = result.Message;
            }
            if (!result.IsSuccess)
                return View(model);

            return RedirectToAction(nameof(CreateTransfer));

        }


        /// <summary>
        /// 查詢轉帳紀錄
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        /// 
        [HttpGet]
        public async Task<IActionResult> GetTransfer([FromQuery]QueryViewModel q)
        {
            var data = JsonConvert.SerializeObject(q);
            ResultModel result = new ResultModel();

            string targetUri = _config["api"] + "/Transfer";
            targetUri += q.GenerateQueryString();

            result = await _callApi.CallAPI(null, new Uri(targetUri), "GET");
            //result = await CallAPI(null, new Uri(targetUri), "GET");
            if (result.IsSuccess == true && result.Data != null)
            {
                q.TransferRecords = JsonConvert.DeserializeObject<List<TransferRecord>>(result.Data.ToString());
            }
            if (result.Message == "Unauthorized or token exppired.") {
                return RedirectToAction("Login", "User");
            }
            if (TempData["IsSuccess"] == null) {
                TempData["IsSuccess"] = result.IsSuccess;
                TempData["msg"] = result.Message;
            }
         
            return View(q);
        }


        /// <summary>
        /// 編輯財務紀錄
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {

            ResultModel result = new ResultModel();
            Uri address = new Uri(_config["api"] + "/Transfer/" + id.ToString());
            result = await _callApi.CallAPI("null", address, "GET");
            TempData["IsSuccess"] = result.IsSuccess;
            TempData["msg"] = result.Message;
            TransferRecord f = new TransferRecord();
            if (result.IsSuccess == true)
            {
                f = JsonConvert.DeserializeObject<TransferRecord>(result.Data.ToString());
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
        public async Task<IActionResult> Edit(TransferRecord f, string Type)
        {
            if (!ModelState.IsValid)
            {
                return View(f);
            }

            var data = JsonConvert.SerializeObject(f);

            ResultModel result = new ResultModel();
            result = await _callApi.CallAPI(data, new Uri(_config["api"] + "/Transfer"), "PUT");

            TempData["IsSuccess"] = result.IsSuccess;
            TempData["msg"] = result.Message;
            if (!result.IsSuccess)
                return View(f);
            return RedirectToAction(nameof(GetTransfer));
        }

        /// <summary>
        /// 刪除財務紀錄
        /// 先跳到確認畫面Get資料
        /// /// 路由資料
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> DeleteTransfer(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ResultModel result = new ResultModel();
            Uri address = new Uri(_config["api"] + "/Transfer/" + id.ToString());

            result = await _callApi.CallAPI("null", address, "GET");
            if (result.IsSuccess == true)
            {
                TransferRecord r = new TransferRecord();
                r = JsonConvert.DeserializeObject<TransferRecord>(result.Data.ToString());
                TempData["isSuccess"] = "true";
                return View(r);
            }
            else
            { //result.IsSuccess == false
                TempData["isSuccess"] = "false";
                TempData["msg"] = "Record Id not exist.";
                return View();
            }

        }

        [HttpPost, ActionName("DeleteTransfer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            Uri address = new Uri(_config["api"] + "/Transfer/" + id);
            ResultModel result = new ResultModel();
            result = await _callApi.CallAPI("null", address, "DELETE");
            TempData["IsSuccess"] = result.IsSuccess;
            TempData["msg"] = result.Message;
            if (result.IsSuccess == false)
                return View();
            else
                return RedirectToAction(nameof(GetTransfer));
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
           

            string targetUri = _config["api"] + "/Transfer?";
            if (!string.IsNullOrEmpty(q.KeyWord))
            {
                targetUri = targetUri + "KeyWord=" + q.KeyWord.Trim();
            }
            if (q.Year != 0)
            {
                targetUri = targetUri + "&Year=" + q.Year;
            }
            if (q.Month != 0)
            {
                targetUri = targetUri + "&Month=" + q.Month;
            }

          
            result = await _callApi.CallAPI(null, new Uri(targetUri), "GET");

            if (result.IsSuccess == true && result.Data != null)
            {
                q.TransferRecords = JsonConvert.DeserializeObject<List<TransferRecord>>(result.Data.ToString());
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
                            CellValue = new CellValue("時間"),
                            DataType = CellValues.String
                        },
                        new Cell()
                        {
                            CellValue = new CellValue("項目名稱"),
                            DataType = CellValues.String
                        },
                         new Cell()
                         {
                             CellValue = new CellValue("類別"),
                             DataType = CellValues.String
                         },
                        new Cell()
                        {
                            CellValue = new CellValue("匯入銀行"),
                            DataType = CellValues.String
                        },
                        new Cell()
                        {
                            CellValue = new CellValue("匯款帳號"),
                            DataType = CellValues.String
                        }, new Cell()
                        {
                            CellValue = new CellValue("金額"),
                            DataType = CellValues.String
                        }, new Cell()
                        {
                            CellValue = new CellValue("備註"),
                            DataType = CellValues.String
                        }
                       
                    );
                    sheetData.AppendChild(row);

                    for (var i = 0; i < q.TransferRecords.Count(); i++)
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
                                CellValue = new CellValue(q.TransferRecords[i].CreateDate.ToString()),
                                DataType = CellValues.String
                            },
                            new Cell()
                            {
                                CellValue = new CellValue(q.TransferRecords[i].eventName),
                                DataType = CellValues.String
                            }
                            ,
                            new Cell()
                            {
                                CellValue = new CellValue(q.TransferRecords[i].TransferType),
                                DataType = CellValues.String
                            }
                            ,
                            new Cell()
                            {
                                CellValue = new CellValue(q.TransferRecords[i].BankName),
                                DataType = CellValues.String
                            }
                            ,
                            new Cell()
                            {
                                CellValue = new CellValue(q.TransferRecords[i].BankAccount),
                                DataType = CellValues.String
                            }
                            ,
                            new Cell()
                            {
                                CellValue = new CellValue(q.TransferRecords[i].Amount.ToString()),
                                DataType = CellValues.String
                            }
                            , new Cell()
                            {
                                CellValue = new CellValue(q.TransferRecords[i].Notes),
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