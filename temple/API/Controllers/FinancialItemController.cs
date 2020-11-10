using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using API.Data;
using API.Helpers;
using API.Models;
using API.Models.FinancialItem;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NLog;

namespace API.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class FinancialItemController : ControllerBase
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private readonly ITokenGetUserHelper _TokenGetUserHelper;
        private TempleDbContext _context;
        public FinancialItemController(TempleDbContext context, ITokenGetUserHelper TokenGetUserHelper)
        {
            _context = context;
            _TokenGetUserHelper = TokenGetUserHelper;
        }

        /// <summary>
        /// 查詢財務項目資料
        /// </summary>
  
        /// <returns></returns>
        [HttpGet]
        public async Task<ResultModel> Get([FromQuery]string Name, string Type)
        {
            logger.Info("test");
            var result = new ResultModel();
            if (!String.IsNullOrEmpty(Name))
                Name = Name.Trim();
            if (!String.IsNullOrEmpty(Type)) {
                Type = Type.Trim();
                if (Type != "收入" && Type != "支出")
                {
                    result.IsSuccess = false;
                    result.Message = "Type:收入 or 支出";
                    return result;
                }

            }

            var query = from f in _context.FinancialItems
                        where (String.IsNullOrEmpty(Name) ? true : f.Name.Contains(Name)) &&
                        (String.IsNullOrEmpty(Type) ? true : f.Type == Type)
                        orderby f.Name
                        select f;

            if (query.Count() > 0)
            {
                result.IsSuccess = true;
                result.Data = query;
            }
            else
            {
                result.Message = "查無財務項目";
                result.IsSuccess = false;
            }
            return result;
        }

        [HttpGet("{id}")]
        public async Task<ResultModel> Get(int id)
        {
            var result = new ResultModel();
            //var record = _context.RitualMoneyRecords.FindAsync(id)
            //    .where
            //var friend = _context.Friends.FindAsync(id);

            var query = from f in _context.FinancialItems
                        where id == f.Id
                        select f;

            if (query.Count() != 0)
            {
                result.IsSuccess = true;
                result.Data = query.SingleOrDefault();
            }
            else
            {
                result.Message = "查無財務項目";
                result.IsSuccess = false;
            }

            return result;
        }

        /// <summary>
        /// 查詢財務項目資料給DropdownList
        /// </summary>

        /// <returns></returns>
        [HttpGet("list")]
        public async Task<ResultModel> GetList([FromQuery]string Name, string Type)
        {
            var result = new ResultModel();
            if (!String.IsNullOrEmpty(Name))
                Name = Name.Trim();
            if (!String.IsNullOrEmpty(Type))
            {
                Type = Type.Trim();
                if (Type != "收入" && Type != "支出")
                {
                    result.IsSuccess = false;
                    result.Message = "Type:收入 or 支出";
                    return result;
                }

            }

            var query = from f in _context.FinancialItems
                        where (String.IsNullOrEmpty(Name) ? true : f.Name.Contains(Name)) &&
                        (String.IsNullOrEmpty(Type) ? true : f.Type == Type)
                        orderby f.Name
                        select new SelectListItem
                        {
                            Value = f.Id.ToString(),
                            Text = f.Name
                        };

            if (query.Count() > 0)
            {
                result.IsSuccess = true;
                result.Data = query;
            }
            else
            {
                result.Message = "查無財務項目";
                result.IsSuccess = false;
            }
            return result;
        }

        ////// <summary>
        ///新增財務項目
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> Post(FinancialItem item) {
            var result = new ResultModel();
         
            ///輸入資料檢查
           if (item.Name==null||item.Type==null)
            {
                result.IsSuccess = false;
                result.Message = "輸入資料不足";
                return result;
            }
            if (item.Type != "收入" && item.Type != "支出") {
                result.IsSuccess = false;
                result.Message = "Type:收入 or 支出";
                return result;
            }
            //==============
            var queryItem = from i in _context.FinancialItems
                            where i.Name.ToUpper() == item.Name.ToUpper()
                            select i;
            if (queryItem.Count() > 0) {
                result.IsSuccess = false;
                result.Message = "項目名稱重複";
                return result;
            }

            try
            {

                _context.FinancialItems.Add(item);
                result.IsSuccess = true;
                result.Message = "添加財務項目成功";
                await _context.SaveChangesAsync();
                result.Data = new {
                    item.Id
                };
            }
            catch (Exception e)
            {
                result.IsSuccess = false;
                result.Message = "DB ERROR";
            }

            var accessToken = Request.Headers["Authorization"];
            var user =await _TokenGetUserHelper.GetUser(accessToken);
            logger.Info("userId=" + user.Id + ", username=" + user.UserName + $"\nCreate "+"FinancialItem id= "+item.Id+", name="+item.Name+" successfully.");
            return result;

        }


        /// <summary>
        /// 編輯財務項目資料
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<ResultModel> Put([FromBody]FinancialItem item)
        {
            var result = new ResultModel();
            result.Message = item.Name;
            if (item.Type != "收入" && item.Type != "支出")
            {
                result.IsSuccess = false;
                result.Message = "Type:收入 or 支出";
                return result;
            }

            FinancialItem itemToUpdate = await _context.FinancialItems.FindAsync(item.Id);
            if (itemToUpdate == null)
            {
                result.IsSuccess = false;
                result.Message = "查無財務項目";
                return result;
            }
          
            //檢查名稱是否重複
            var query = from o in _context.FinancialItems
                        where o.Name.ToUpper() == item.Name.ToUpper()
                        select o;
            if (query.Count() > 0)
            {//排除跟自己同名
                if (query.FirstOrDefault().Id != item.Id)
                {
                    result.IsSuccess = false;
                    result.Message = "Name: " + item.Name + " exist.";
                    return result;
                }

            }


            try
            {

                _context.Entry(itemToUpdate).CurrentValues.SetValues(item);

                result.IsSuccess = true;
                result.Message = "編輯財務項目成功";
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                result.IsSuccess = false;
                result.Message = "DB ERROR";
            }
            var accessToken = Request.Headers["Authorization"];
            var user = await _TokenGetUserHelper.GetUser(accessToken);
            logger.Info("userId=" + user.Id + ", username=" + user.UserName + $"\nUpdate " + "FinancialItem id= " + item.Id + ", name=" + item.Name + " successfully.");


            return result;
        }


        /// <summary>
        /// 刪除財務項目資料
        /// </summary>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<ResultModel> Delete(int id)
        {
            var result = new ResultModel();
            FinancialItem item = await _context.FinancialItems.FindAsync(id);
            if (item == null)
            {
                result.IsSuccess = false;
                result.Message = "查無財務項目";
            }
            else
            {
                try
                {
                    _context.Remove(item);
                    await _context.SaveChangesAsync();
                    result.IsSuccess = true;
                    result.Message = "刪除財務項目成功";
                }
                catch (Exception e)
                {
                    result.IsSuccess = false;
                    if (e.InnerException.Message.Contains("FOREIGN KEY constraint failed"))
                        result.Message = "項目使用中";
                    else
                    result.Message = "DB error.";
                }
            }

            var accessToken = Request.Headers["Authorization"];
            var user = await _TokenGetUserHelper.GetUser(accessToken);
            logger.Info("userId=" + user.Id + ", username=" + user.UserName + $"\n Delete " + "FinancialItem id= " + item.Id + ", name=" + item.Name + " successfully.");
            return result;
        }

    }
}