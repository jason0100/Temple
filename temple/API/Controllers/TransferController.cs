using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using API.Data;
using API.Helpers;
using API.Models;
using API.Models.MemberData;
using API.Models.Transfer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace API.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class TransferController : ControllerBase
    {
        private TempleDbContext _context;
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private readonly ITokenGetUserHelper _TokenGetUserHelper;
        public TransferController(TempleDbContext context, ITokenGetUserHelper TokenGetUserHelper)
        {
            _context = context;
            _TokenGetUserHelper = TokenGetUserHelper;
        }

        ////// <summary>
        ///新增轉帳紀錄
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> Post(TransferRecord record)
        {
            var result = new ResultModel();
            try
            {
                record.CreateDate = DateTime.Now.ToLocalTime();
                _context.TransferRecords.Add(record);
                result.IsSuccess = true;
                result.Message = "添加轉帳紀錄成功.";
                await _context.SaveChangesAsync();
                result.Data = new { record.Id};
            }
            catch (Exception e)
            {
                result.IsSuccess = false;
                result.Message = "DB ERROR";
            }

            var accessToken = Request.Headers["Authorization"];
            var user = await _TokenGetUserHelper.GetUser(accessToken);
            logger.Info("userId=" + user.Id + ", username=" + user.UserName + $"\nCreate " + "TransferRecord id= " + record.Id + " successfully.");

            return result;
        }

        /// <summary>
        /// 查詢轉帳紀錄
        /// </summary>

        /// <returns></returns>
        [HttpGet]
        public async Task<ResultModel> Get([FromQuery]QueryModel model)
        {
            var result = new ResultModel();
            if (!String.IsNullOrEmpty(model.KeyWord))
                model.KeyWord = model.KeyWord.Trim();
          

            var query = from f in _context.TransferRecords
                            
                        where ((String.IsNullOrEmpty(model.KeyWord) ? true : f.eventName.Contains(model.KeyWord)) ||
                              (String.IsNullOrEmpty(model.KeyWord) ? true : f.BankName.Contains(model.KeyWord))
                            || (String.IsNullOrEmpty(model.KeyWord) ? true : f.TransferType.Contains(model.KeyWord)))
                               && (model.Year == null ? true: f.CreateDate.Year.Equals(model.Year))
                            && (model.Month == null ? true: f.CreateDate.Month.Equals(model.Month))
                        orderby f.CreateDate
                        select f;
            query = query.OrderByDescending(e => e.CreateDate);//日期最新優先排序
            result.Data = query.ToList();
            if (query.Count() == 0)
            {
                result.Message = "查無轉帳紀錄";
                result.IsSuccess = true;
            }
            else
            {
                result.IsSuccess = true;
            }
            return result;
        }

        [HttpGet("{id}")]
        public async Task<ResultModel> Get(int id)
        {
            var result = new ResultModel();
            var query = _context.TransferRecords.FindAsync(id);
                
            result.IsSuccess = true;
            if (query == null)
            {
                result.Message = "查無轉帳紀錄";
                return result;
            }
            result.Data = query.Result;
            return result;
        }


        /// <summary>
        /// 修改轉帳紀錄
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<ResultModel> Put([FromBody]TransferRecord item)
        {
            var result = new ResultModel();
            TransferRecord itemToUpdate = await _context.TransferRecords.FindAsync(item.Id);
            //檢查是否存在
            if (itemToUpdate == null)
            {
                result.IsSuccess = false;
                result.Message = "查無轉帳紀錄";
                return result;
            }
            
            if (item.eventName != null)
                itemToUpdate.eventName = item.eventName.Trim();
            if (item.BankName != null)
                itemToUpdate.BankName = item.BankName.Trim();
            if (item.BankAccount != null)
                itemToUpdate.BankAccount = item.BankAccount.Trim();
            if (item.TransferType != null)
                itemToUpdate.TransferType = item.TransferType;
            if (item.Notes != null)
                itemToUpdate.Notes = item.Notes;
            if (item.Amount != null)
                itemToUpdate.Amount = item.Amount;
           
            try
            {
                result.IsSuccess = true;
                result.Message = "編輯成功";
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                result.IsSuccess = false;
                result.Message = "DB ERROR";
            }

            var accessToken = Request.Headers["Authorization"];
            var user = await _TokenGetUserHelper.GetUser(accessToken);
            logger.Info("userId=" + user.Id + ", username=" + user.UserName + $"\nEdit " + "TransferRecord id= " + item.Id + " successfully.");
            return result;
        }

        /// <summary>
        /// 刪除轉帳紀錄
        /// </summary>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<ResultModel> Delete(int id)
        {
            var result = new ResultModel();
            TransferRecord item = await _context.TransferRecords.FindAsync(id);
            if (item == null)
            {
                result.IsSuccess = false;
                result.Message = "查無轉帳紀錄.";
            }
            else
            {
                try
                {
                    _context.Remove(item);
                    await _context.SaveChangesAsync();
                    result.IsSuccess = true;
                    result.Message = "刪除成功";
                }
                catch (Exception e)
                {
                    result.IsSuccess = false;
                    result.Message = "DB error.";
                }
            }

            var accessToken = Request.Headers["Authorization"];
            var user = await _TokenGetUserHelper.GetUser(accessToken);
            logger.Info("userId=" + user.Id + ", username=" + user.UserName + $"\nDelete " + "TransferRecord id= " + id + " successfully.");
            return result;
        }
    }
}