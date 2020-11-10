using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.Models;
using API.Models.FinancialReport;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using API.Helpers;
using NLog;

namespace API.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class FinancialReportController : ControllerBase
    {
        private TempleDbContext _context;
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private readonly ITokenGetUserHelper _TokenGetUserHelper;
        public FinancialReportController(TempleDbContext context, ITokenGetUserHelper TokenGetUserHelper)
        {
            _context = context;
            _TokenGetUserHelper = TokenGetUserHelper;
        }

        /// <summary>
        /// 查詢財務項目資料
        /// </summary>

        /// <returns></returns>
         [HttpGet]
        public async Task<object> Get([FromQuery]string Name, int? Year, string Type) {
            var result = new ResultModel();
            if (string.IsNullOrEmpty(Type)) {
                result.IsSuccess = false;
                result.Message = "Type is required.";
                return result;
            }
         
         if (!String.IsNullOrEmpty(Name))
            Name = Name.Trim();
          
           
            var queryItem = from i in _context.FinancialItems
                        where (i.Type==Type)&&
                            (String.IsNullOrEmpty(Name)?true: i.Name.Contains(Name))
                        select i.Name;

            Report report = new Report();
         
            foreach (var i in queryItem) {
                var item = new ReportItem();
                item.Name = i;
                report.ReportItems.Add(item);
               
            }
            int thisYear = DateTime.Now.Year;
            var queryRecord = from r in _context.FinancialRecords
                              .Include(r => r.FinancialItem)
                              where (r.FinancialItem.Type == Type)
                                &&((String.IsNullOrEmpty(Name)) ? true : r.FinancialItem.Name.Contains(Name))
                                &&(Year == null ? r.CreateDate.Year == thisYear : r.CreateDate.Year == Year)

                              select r;
            foreach (var i in queryRecord) {
                report.MonthSubtotal[Convert.ToDateTime(i.CreateDate).Month - 1] += Convert.ToInt32( i.Amount);
                
                //各項目的每月加總
                foreach (var r in report.ReportItems) {
                    if (i.FinancialItem.Name == r.Name) {
                        r.Subtotal[Convert.ToDateTime(i.CreateDate).Month - 1] += Convert.ToInt32(i.Amount);
                    }
                    
                }
            }
         
         
            if (report.ReportItems.Count() != 0)
            {
               
                result.IsSuccess = true;
                result.Data = JsonConvert.SerializeObject(report);
                //result.Data = report;

            }
            else
            {
                result.Message = "查無資料";
                result.IsSuccess = true;
                result.Data = new { };
            }

            return result;
        }
    }
}