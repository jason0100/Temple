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
using temple.Models.FinancialRecord;
using ExtensionMethods;
using temple.Helpers;

namespace temple.Controllers
{
    [Authorize]
    [AutoValidateAntiforgeryToken]
    public class FinancialRecordExpensesController : Controller
    {

        private readonly IConfiguration _config;
        private readonly ICallApi _callApi;
        /// <summary>
        /// 讀取組態用
        /// </summary>

        public FinancialRecordExpensesController(IConfiguration config, ICallApi callApi)
        {
            _config = config;
            _callApi = callApi;
        }


        /// <summary>
        /// 查詢財務紀錄
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetExpenses(QueryModel q)
        {
            var data = JsonConvert.SerializeObject(q);
            ResultModel result = new ResultModel();

            string targetUri = _config["api"] + "/FinancialRecord";
            targetUri += q.GenerateQueryString();

            result = await _callApi.CallAPI(null, new Uri(targetUri), "GET");
            if (result.IsSuccess == true && result.Data != null)
            {
                q.FinancialRecords = JsonConvert.DeserializeObject<List<FinancialRecord>>(result.Data.ToString());
                foreach (var i in q.FinancialRecords)
                {
                    if (!string.IsNullOrEmpty(i.ReturnDate))
                        i.ReturnDate = i.ReturnDate.Split("T")[0];
                }

    ;
            }


            //載入下拉選單
            var dropdownResult = new ResultModel();
            Uri DropdownUri = new Uri(_config["api"] + "/FinancialItem/list/?Type=" + q.Type);
            dropdownResult = await _callApi.CallAPI("", DropdownUri, "GET");
            if (dropdownResult.IsSuccess)
            {
                TempData["dropdown"] = JsonConvert.DeserializeObject<IEnumerable<SelectListItem>>(dropdownResult.Data.ToString());
            }
            //END載入下拉選單


            if (TempData["IsSuccess"] == null)
            {
                TempData["IsSuccess"] = result.IsSuccess;
                TempData["msg"] = result.Message;
            }
            return View(q);
        }


        ////// <summary>
        ///新增財務紀錄
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpGet]
        public async Task<IActionResult> CreateExpenses(string Type)
        {
            //載入下拉選單
            ResultModel result = new ResultModel();
            Uri targetUri = new Uri(_config["api"] + "/FinancialItem/list/?Type=" + Type);
            result = await _callApi.CallAPI("", targetUri, "GET");
            FinancialRecord record = new FinancialRecord();
            if (result.IsSuccess == true && result.Data != null)
            {
                record.FinancialItems = JsonConvert.DeserializeObject<IEnumerable<SelectListItem>>(result.Data.ToString());
            }
            //END載入下拉選單

            return View(record);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateExpenses(FinancialRecord model, string Type)
        {
            //載入下拉選單
            ResultModel SelectListresult = new ResultModel();
            Uri targetUri = new Uri(_config["api"] + "/FinancialItem/list/?Type=" + Type);
            SelectListresult = await _callApi.CallAPI("", targetUri, "GET");

            if (SelectListresult.IsSuccess == true && SelectListresult.Data != null)
            {
                model.FinancialItems = JsonConvert.DeserializeObject<IEnumerable<SelectListItem>>(SelectListresult.Data.ToString());
            }
            //END載入下拉選單
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var data = JsonConvert.SerializeObject(model);

            ResultModel result = new ResultModel();
            result = await _callApi.CallAPI(data, new Uri(_config["api"] + "/FinancialRecord"), "POST");

            TempData["IsSuccess"] = result.IsSuccess;
            TempData["msg"] = result.Message;
            if (!result.IsSuccess)
                return View(model);

            return RedirectToAction(nameof(CreateExpenses), new { Type = "支出" });

        }

        /// <summary>
        /// 編輯財務紀錄
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Edit(int id, string Type)
        {

            ResultModel result = new ResultModel();
            Uri address = new Uri(_config["api"] + "/FinancialRecord/" + id.ToString());
            result = await _callApi.CallAPI("null", address, "GET");

            FinancialRecord f = new FinancialRecord();
            if (result.IsSuccess == true)
            {

                f = JsonConvert.DeserializeObject<FinancialRecord>(result.Data.ToString());
                if (!string.IsNullOrEmpty(f.ReturnDate))
                    f.ReturnDate = f.ReturnDate.Split("T")[0];

                if (!string.IsNullOrEmpty(f.DueDate))
                    f.DueDate = f.DueDate.Split("T")[0];
                //載入下拉選單
                ResultModel SelectListresult = new ResultModel();
                Uri targetUri = new Uri(_config["api"] + "/FinancialItem/list/?Type=" + Type);
                SelectListresult = await _callApi.CallAPI("", targetUri, "GET");

                if (SelectListresult.IsSuccess == true && SelectListresult.Data != null)
                {
                    f.FinancialItems = JsonConvert.DeserializeObject<IEnumerable<SelectListItem>>(SelectListresult.Data.ToString());
                }
                //END載入下拉選單
                //TempData["isSuccess"] = "true";
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
        public async Task<IActionResult> Edit(FinancialRecord f, string Type)
        {
            //載入下拉選單
            ResultModel SelectListresult = new ResultModel();
            Uri targetUri = new Uri(_config["api"] + "/FinancialItem/list/?Type=" + Type);
            SelectListresult = await _callApi.CallAPI("", targetUri, "GET");

            if (SelectListresult.IsSuccess == true && SelectListresult.Data != null)
            {
                f.FinancialItems = JsonConvert.DeserializeObject<IEnumerable<SelectListItem>>(SelectListresult.Data.ToString());
            }
            //END載入下拉選單

            if (!ModelState.IsValid)
            {
                return View(f);
            }

            //if (f.DueDate == null) {
            //    f.DueDate = "";
            //}
            var data = JsonConvert.SerializeObject(f);

            ResultModel result = new ResultModel();
            result = await _callApi.CallAPI(data, new Uri(_config["api"] + "/FinancialRecord"), "PUT");

            TempData["IsSuccess"] = result.IsSuccess;
            TempData["msg"] = result.Message;
            if (!result.IsSuccess)
                return View(f);
            return RedirectToAction(nameof(GetExpenses), new { Type = "支出" });
        }

        /// <summary>
        /// 刪除財務紀錄
        /// 先跳到確認畫面Get資料
        /// /// 路由資料
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }


            ResultModel result = new ResultModel();
            Uri address = new Uri(_config["api"] + "/FinancialRecord/" + id.ToString());

            result = await _callApi.CallAPI("null", address, "GET");


            if (result.IsSuccess == true)
            {
                FinancialRecord r = new FinancialRecord();
                r = JsonConvert.DeserializeObject<FinancialRecord>(result.Data.ToString());
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

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            Uri address = new Uri(_config["api"] + "/FinancialRecord/" + id.ToString());
            ResultModel result = new ResultModel();
            result = await _callApi.CallAPI("null", address, "DELETE");
            TempData["IsSuccess"] = result.IsSuccess;
            TempData["msg"] = result.Message;
            if (result.IsSuccess == false)
                return View();
            else
                return RedirectToAction(nameof(GetExpenses), new { Type = "支出" });
        }

        /// <summary>
        /// 匯出Excel
        /// 及時產生filestream串流不存檔
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> ExportExcel(QueryModel q)
        {
            ResultModel result = new ResultModel();
            //var data = JsonConvert.SerializeObject(q);


            string targetUri = _config["api"] + "/FinancialRecord";
            targetUri += q.GenerateQueryString();

            result = await _callApi.CallAPI(null, new Uri(targetUri), "GET");

            if (result.IsSuccess == true && result.Data != null)
            {
                q.FinancialRecords = JsonConvert.DeserializeObject<List<FinancialRecord>>(result.Data.ToString());
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
                            CellValue = new CellValue("編號"),
                            DataType = CellValues.String
                        },
                        new Cell()
                        {
                            CellValue = new CellValue("姓名"),
                            DataType = CellValues.String
                        },
                         new Cell()
                         {
                             CellValue = new CellValue("時間"),
                             DataType = CellValues.String
                         },
                        new Cell()
                        {
                            CellValue = new CellValue("交易項目"),
                            DataType = CellValues.String
                        },
                        new Cell()
                        {
                            CellValue = new CellValue("數量"),
                            DataType = CellValues.String
                        }, new Cell()
                        {
                            CellValue = new CellValue("金額"),
                            DataType = CellValues.String
                        }, new Cell()
                        {
                            CellValue = new CellValue("位置"),
                            DataType = CellValues.String
                        }
                          , new Cell()
                          {
                              CellValue = new CellValue("到期日"),
                              DataType = CellValues.String
                          }
                         , new Cell()
                         {
                             CellValue = new CellValue("備註"),
                             DataType = CellValues.String
                         }
                    );
                    sheetData.AppendChild(row);

                    for (var i = 0; i < q.FinancialRecords.Count(); i++)
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
                                CellValue = new CellValue(q.FinancialRecords[i].Id.ToString()),
                                DataType = CellValues.String
                            },
                            new Cell()
                            {
                                CellValue = new CellValue(q.FinancialRecords[i].CustomerName),
                                DataType = CellValues.String
                            }
                            ,
                            new Cell()
                            {
                                CellValue = new CellValue(q.FinancialRecords[i].CreateDate),
                                DataType = CellValues.String
                            }
                            ,
                            new Cell()
                            {
                                CellValue = new CellValue(q.FinancialRecords[i].FinancialItem.Name),
                                DataType = CellValues.String
                            }
                           ,
                            new Cell()
                            {
                                CellValue = new CellValue(q.FinancialRecords[i].Quantity.ToString()),
                                DataType = CellValues.String
                            }
                           ,
                            new Cell()
                            {
                                CellValue = new CellValue(q.FinancialRecords[i].Amount.ToString()),
                                DataType = CellValues.String
                            }
                            , new Cell()
                            {
                                CellValue = new CellValue(q.FinancialRecords[i].Position.ToString()),
                                DataType = CellValues.String
                            }
                            , new Cell()
                            {
								//CellValue = new CellValue(q.FinancialRecords[i].DueDate),
								CellValue = new CellValue(""),
								DataType = CellValues.String
                            }, new Cell()
                            {
								CellValue = new CellValue(q.FinancialRecords[i].Notes),
								//CellValue = new CellValue(""),
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