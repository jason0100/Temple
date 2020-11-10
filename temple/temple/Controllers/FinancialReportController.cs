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
using Newtonsoft.Json;
using temple.Helpers;
using temple.Models;
using temple.Models.FinancialRecord;
using temple.Models.FinancialReport;

namespace temple.Controllers
{
    [Authorize]
    [AutoValidateAntiforgeryToken]
    public class FinancialReportController : Controller
    {

        private readonly IConfiguration _config;
        private readonly ICallApi _callApi;

        /// <summary>
        /// 讀取組態用
        /// </summary>

        public FinancialReportController(IConfiguration config, ICallApi callApi)
        {
            _config = config;
            _callApi = callApi;
        }
        

        /// <summary>
        /// 查詢財務報表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetReport(QueryReportModel model, string Type)
        {
            
            ResultModel result = new ResultModel();
            model.Type = Type;
            string address = _config["api"] + "/FinancialReport?Type=" + Type;
            if (!String.IsNullOrEmpty(model.Name)) {
                address = address + "&Name="+ model.Name.Trim();
            }
            if (model.Year !=0)
            {
                address = address + "&Year=" + model.Year;
            }
            

            result = await _callApi.CallAPI(null, new Uri(address), "GET");
            model.report = new Report();
            if (result.IsSuccess == true && result.Data != null)
            {
                model.report = JsonConvert.DeserializeObject<Report>(result.Data.ToString());
            }
            
            if (model.Year == 0)
                TempData["Year"] = DateTime.Now.Year;
            else
                TempData["Year"] = model.Year;

            if (TempData["IsSuccess"] == null)
            {
                TempData["IsSuccess"] = result.IsSuccess;
                TempData["msg"] = result.Message;
            }
            return View(model);

        }

     

        /// <summary>
        /// 匯出Excel
        /// 及時產生filestream串流不存檔
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<FileStreamResult> ExportExcel(QueryReportModel model)
        {
            ResultModel result = new ResultModel();
            string address = _config["api"] + "/FinancialReport?Type=" + model.Type;
            if (!String.IsNullOrEmpty(model.Name))
            {
                address = address + "&Name=" + model.Name.Trim();
            }
            if (model.Year != 0)
            {
                address = address + "&Year=" + model.Year;
            }

            result = await _callApi.CallAPI(null, new Uri(address), "GET");
            if (result.IsSuccess == true && result.Data != null)
            {
                string [] monthTw = new string[] { "一","二","三","四","五","六","七","八","九","十", "十一","十二" };

                model.report = JsonConvert.DeserializeObject<Report>(result.Data.ToString());
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


                    if (model.Year == 0)
                        model.Year = DateTime.Now.Year;

                    var row = new Row();
                    row.Append(
                      new Cell()
                      {
                          
                          CellValue = new CellValue(model.Year.ToString()+ "年收入年報表"),
                          DataType = CellValues.String
                      });
                    sheetData.AppendChild(row);
                    row = new Row();
                    row.Append(
                         new Cell()
                         {
                             CellValue = new CellValue("財務項目"),
                             DataType = CellValues.String
                         }
                    
                    );
                    for (int i = 0; i < 12; i++) {
                        row.Append(
                           new Cell()
                           {
                               CellValue = new CellValue(monthTw[i] + "月"),
                               DataType = CellValues.String
                           });
                    }
                    sheetData.AppendChild(row);

                    foreach (var i in model.report.ReportItems)
                    {

                        row = new Row();
                        row.Append(
                         
                            new Cell()
                            {
                                CellValue = new CellValue(i.Name),
                                DataType = CellValues.String
                            }
                        );
                        for (int j = 0; j < 12; j++)
                        {
                            row.Append(
                               new Cell()
                               {
                                   CellValue = new CellValue(Convert.ToInt32( i.Subtotal[j]).ToString()),
                                   DataType = CellValues.String
                               });
                        }
                        sheetData.AppendChild(row);
                    }

                    row = new Row();
                    row.Append(

                        new Cell()
                        {
                            CellValue = new CellValue("總計"),
                            DataType = CellValues.String
                        }
                    );
                    for (int j = 0; j < 12; j++)
                    {
                        row.Append(
                           new Cell()
                           {
                               CellValue = new CellValue(Convert.ToInt32( model.report.MonthSubtotal[j]).ToString()),
                               DataType = CellValues.String
                           });
                    }
                    sheetData.AppendChild(row);
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