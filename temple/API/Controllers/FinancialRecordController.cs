using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.Helpers;
using API.Models;
using API.Models.FinancialRecord;
using API.Models.Notification;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using NLog;


namespace API.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class FinancialRecordController : ControllerBase
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();
        private readonly ITokenGetUserHelper _TokenGetUserHelper;
        private TempleDbContext _context;
        public FinancialRecordController(TempleDbContext context, ITokenGetUserHelper TokenGetUserHelper)
        {
            _context = context;
            _TokenGetUserHelper = TokenGetUserHelper;
        }


        /// <summary>
        /// 查詢財務紀錄
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ResultModel> Get([FromQuery]QueryModel model)
        {
            var result = new ResultModel();

            var query = from f in _context.FinancialRecords
                            .Include(f => f.FinancialItem)
                        where (model.ItemId == null ? true : f.FinancialItem.Id.Equals(model.ItemId)) &&
                            (f.FinancialItem.Type == model.Type) &&
                            (model.Year == null ? true : f.CreateDate.Year.Equals(model.Year)) &&
                            (model.Month == null ? true : f.CreateDate.Month.Equals(model.Month))
                        orderby f.CreateDate
                        select f;
            query = query.OrderByDescending(e => e.CreateDate);//日期最新優先排序
            result.Data = query.ToList();
            if (query.Count() == 0)
            {
                result.Message = "查無資料";
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


            var query = await _context.FinancialRecords
                .Include(m => m.FinancialItem)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (query == null)
            {
                result.Message = "查無資料";
                result.IsSuccess = false;

            }
            else
            {
                result.IsSuccess = true;
                result.Data = query;
            }

            return result;
        }

        ////// <summary>
        ///新增財務紀錄
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> Post(FinancialRecord record)
        {
            var result = new ResultModel();
            var existFinancialItem = _context.FinancialItems.FirstOrDefault(a => a.Id == record.FinancialItemId);
            if (record.FinancialItemId == 57 || record.FinancialItemId == 75)
            {
                //計算xx燈剩餘位子數量
                int max_Light = 666;
                if (record.Position > max_Light / 2)
                {
                    result.IsSuccess = false;
                    result.Message = "位置最多為" + max_Light / 2;
                    return result;
                }

                var existPosition = _context.FinancialRecords
                    .Where(a => a.FinancialItemId == record.FinancialItemId)
                    .Where(a => a.Position == record.Position)
                    .Where(a => a.DueDate > DateTime.Now);
                if (existPosition.Count() > 0)
                {
                    result.IsSuccess = false;
                    result.Message = "位置已佔用,請重新選擇";
                    return result;
                }
            }
            try
            {
                NotifyModel notify = new NotifyModel();
                notify.CreateDate = DateTime.Now;
                notify.IsRead = false;
                notify.ItemName = existFinancialItem.Name;
                notify.MemberName = record.CustomerName;
                _context.Notification.Add(notify);

                record.CreateDate = DateTime.Now.ToLocalTime();

                _context.FinancialRecords.Add(record);
                result.IsSuccess = true;
                result.Message = "添加紀錄成功.";
                await _context.SaveChangesAsync();
                result.Data = new
                {
                    Id = record.Id
                };
            }
            catch (Exception e)
            {
                result.IsSuccess = false;
                result.Message = "DB ERROR";
            }

            var accessToken = Request.Headers["Authorization"];
            var user = await _TokenGetUserHelper.GetUser(accessToken);
            logger.Info("userId=" + user.Id + ", username=" + user.UserName + $"\n Create " + "FinancialRecord id= " + record.Id + " successfully.");


            return result;

        }


        /// <summary>
        /// 修改財務紀錄
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<ResultModel> Put([FromBody]FinancialRecordToPut item)
        {
            var result = new ResultModel();
            FinancialRecord itemToUpdate = await _context.FinancialRecords.FindAsync(item.Id);
            //檢查是否存在
            if (itemToUpdate == null)
            {
                result.IsSuccess = false;
                result.Message = "查無資料.";
                return result;
            }
            if (item.FinancialItemId != null)
            {
                var existItem = _context.FinancialItems.FirstOrDefault(a => a.Id == item.FinancialItemId);
                if (existItem == null)
                {
                    result.IsSuccess = false;
                    result.Message = "財務項目不存在.";
                    return result;
                }
                itemToUpdate.FinancialItemId = item.FinancialItemId;
                if (item.FinancialItemId == 57 || item.FinancialItemId == 75)
                {
                    //計算xx燈剩餘位子數量
                    int max_Light = 666;
                    if (item.Position > max_Light / 2)
                    {
                        result.IsSuccess = false;
                        result.Message = "位置最多為" + max_Light / 2;
                        return result;
                    }

                    var existPosition = _context.FinancialRecords
                        .Where(a => a.FinancialItemId == item.FinancialItemId)
                        .Where(a => a.Position == item.Position)
                        .Where(a=>a.DueDate>DateTime.Now);

                    if (existPosition.Count() > 0)
                    {
                        if (existPosition.FirstOrDefault().Id != item.Id)
                        {
                            result.IsSuccess = false;
                            result.Message = "位置已佔用,請重新選擇";
                            return result;
                        }
                    }
                }
            }

            if (item.Amount != null)
                itemToUpdate.Amount = item.Amount;

            if (item.CustomerName != null)
                itemToUpdate.CustomerName = item.CustomerName;
            if (item.CustomerPhone != null)
                itemToUpdate.CustomerPhone = item.CustomerPhone;
            if (item.LandPhone != null)
                itemToUpdate.LandPhone = item.LandPhone;
            if (item.DueDate != null)
                itemToUpdate.DueDate = Convert.ToDateTime(item.DueDate);
            if (item.FinancialItemId != null)
                itemToUpdate.FinancialItemId = item.FinancialItemId;
            if (item.Quantity != null)
                itemToUpdate.Quantity = item.Quantity;
            if (item.Notes != null)
                itemToUpdate.Notes = item.Notes;

            itemToUpdate.Position = item.Position;
            if (item.ReturnDate != null)
            {
                //if (item.ReturnDate != "") {
                //DateTime temp;
                //if (!DateTime.TryParse(item.ReturnDate.ToString(), out temp)) {
                //    result.IsSuccess = false;
                //    result.Message = "還願日期有誤";
                //    return result;
                //}
                //itemToUpdate.ReturnDate = item.ReturnDate;
                //}
                //if (item.ReturnDate == "")
                //itemToUpdate.ReturnDate = "";
                itemToUpdate.ReturnDate = Convert.ToDateTime(item.ReturnDate);
            }


            itemToUpdate.Place = item.Place;
            if (string.IsNullOrEmpty(item.Place))
                itemToUpdate.DueDate = null;

            if (item.PayType != null)
                itemToUpdate.PayType = item.PayType;
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
            //logger.Info("userId=" + user.Id + ", username=" + user.UserName + $"\n Edit " + "FinancialRecord id= " + item.Id + " successfully.");

            return result;
        }

        /// <summary>
        /// 刪除財務紀錄
        /// </summary>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<ResultModel> Delete(int id)
        {
            var result = new ResultModel();
            FinancialRecord item = await _context.FinancialRecords.FindAsync(id);
            if (item == null)
            {
                result.IsSuccess = false;
                result.Message = "查無資料";
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
            logger.Info("userId=" + user.Id + ", username=" + user.UserName + $"\n Deltete " + "FinancialRecord id= " + id + " successfully.");
            return result;
        }


        [HttpGet("CalLightCount")]
        public async Task<ResultModel> CalLightCount([FromQuery]string lightType)
        {

            var result = new ResultModel();
            if (lightType != "光明燈" && lightType != "財利燈")
            {
                result.IsSuccess = false;
                result.Message = "lightId value is not valid.";
                return result;
            }
            var typeId = _context.FinancialItems.FirstOrDefault(a => a.Name == lightType);
            var today = DateTime.Now;

            //計算xx燈剩餘位子數量
            int max_Light = 666;

            var Light = _context.FinancialRecords
                .Where(a => a.FinancialItemId == typeId.Id)
                .Where(a => a.DueDate == null ? true : a.DueDate > today)
                .ToList();
            var Light_dragon_side_count = Light.Where(a => a.Place == "龍").Count();
            int Light_dragon_side_next = 0;//無剩餘位置
            if (Light_dragon_side_count < max_Light / 2)//有剩餘位置
            {
                var query = Light.Where(a => a.Place == "龍").OrderByDescending(a => a.Position);
                var Light_dragon_side_current = Light.Where(a => a.Place == "龍").OrderByDescending(a => a.Position).FirstOrDefault();//最後一筆
                if (Light_dragon_side_current != null)
                {
                    if (Light_dragon_side_current.Position + 1 < max_Light)
                        Light_dragon_side_next = Convert.ToInt32(Light_dragon_side_current.Position) + 1;
                    else
                    {//從頭找
                        for (int i = 1; i <= max_Light / 2; i++)
                        {
                            bool isOccupy = Light.Where(a => a.Place == "龍").Any(a => a.Position == i);
                            if (!isOccupy)
                            {
                                Light_dragon_side_next = i;
                                break;
                            }
                        }
                    }
                }
                else
                {//從頭找
                    for (int i = 1; i <= max_Light / 2; i++)
                    {
                        bool isOccupy = Light.Where(a => a.Place == "龍").Any(a => a.Position == i);
                        if (!isOccupy)
                        {
                            Light_dragon_side_next = i;
                            break;
                        }

                    }
                }

            }
            var Light_tiger_side_count = Light.Where(a => a.Place == "虎").Count();
            int Light_tiger_side_next = -1;//無剩餘位置
            if (Light_tiger_side_count < max_Light / 2)//有剩餘位置
            {
                var query = Light.Where(a => a.Place == "虎").OrderByDescending(a => a.Position);
                var Light_tiger_side_current = Light.Where(a => a.Place == "虎").OrderByDescending(a => a.Position).FirstOrDefault();//最後一筆
                if (Light_tiger_side_current != null)
                {
                    if (Light_tiger_side_current.Position + 1 < max_Light)
                        Light_tiger_side_next = Convert.ToInt32(Light_tiger_side_current.Position) + 1;
                    else
                    {//從頭找
                        for (int i = 1; i < max_Light; i++)
                        {
                            bool isOccupy = Light.Where(a => a.Place == "虎").Any(a => a.Position == i);
                            if (!isOccupy)
                            {
                                Light_tiger_side_next = i;
                                break;
                            }
                        }
                    }
                }
                else
                {//從頭找
                    for (int i = 1; i < max_Light; i++)
                    {
                        bool isOccupy = Light.Where(a => a.Place == "虎").Any(a => a.Position == i);
                        if (!isOccupy)
                        {
                            Light_tiger_side_next = i;
                            break;
                        }
                    }
                }

            }


            result.IsSuccess = true;
            result.Data = new
            {
                //Light_dragon_side_count,
                //Light_tiger_side_count,
                Light_dragon_side_next,
                Light_tiger_side_next

            };

            return result;
        }

        /// <summary>
        /// home/index 光明燈到期提醒（一個月前）
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetToBeExpired")]
        public async Task<ResultModel> GetToBeExpired()
        {
            var result = new ResultModel();
            var expiredDate = DateTime.Now.AddMonths(1);
            var query = _context.FinancialRecords
                .Where(a => a.FinancialItemId == 57)
                .Where(a => a.DueDate == null ? true : a.DueDate < expiredDate)
                .ToList();
            result.IsSuccess = true;
            result.Data = query;

            return result;
        }

    }
}