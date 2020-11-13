using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using API.Data;
using API.Helpers;
using API.Models;
using API.Models.MemberData;
using API.Models.RitualMoney;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NLog;

namespace API.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class RitualMoneyController : ControllerBase
    {
        private TempleDbContext _context;
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private readonly ITokenGetUserHelper _TokenGetUserHelper;
        public RitualMoneyController(TempleDbContext context, ITokenGetUserHelper TokenGetUserHelper)
        {
            _context = context;
            _TokenGetUserHelper = TokenGetUserHelper;
        }

        /// <summary>
        /// 查詢發財金
        /// 
        /// </summary>
        /// <returns></returns>

        [HttpGet("{id}")]
        public ResultModel Get(int id)
        {
            var result = new ResultModel();
            //var record = _context.RitualMoneyRecords.FindAsync(id)
            //    .where
            var record = from o in _context.RitualMoneyRecords
                          where o.RitualMoneyRecordId == id
                         select new { Name=o.Member.Name,Identity=o.Member.Identity, Record=o};

            if (record.Count()!=0)
            {
                result.IsSuccess = true;
                result.Data = record.FirstOrDefault();
            }
            else
            {
                result.Message = "查無發財金紀錄";
                result.IsSuccess = false;
            }

            return result;
        }

        [HttpGet]
        public ResultModel Get([FromQuery]QueryRitualMoney q) {
            ResultModel result = new ResultModel();
            if (!string.IsNullOrEmpty(q.Name))
            {
                q.Name = q.Name.Trim().ToUpper();
            }
            var query = from m in _context.members
                        where (string.IsNullOrEmpty(q.Name) ? true : m.Name.ToUpper().Contains(q.Name)) &&
                            (string.IsNullOrEmpty(q.Identity) ? true : m.Identity == q.Identity)
                        select m;
            //select new { m.Name, m.Identity };

            if (query.Count() == 0)
            {
                result.IsSuccess = true;
                result.Message = "查無資料";
                return result;
            }

            foreach (var i in query)
            {
                var records = _context.Entry(i)
                    .Collection(q => q.RitualMoneyRecords)
                    .Query()
                    .ToList();

                q.RitualMoneyRecords.AddRange(records);
          
            }

            if (q.Year != null)
            {
                //if (q.Year < 2019 || q.Year > 2029)
                //{
                //    result.IsSuccess = false;
                //    result.Message = "Input Year should be in range of 2019 to 2029.";
                //    return result;
                //}

                var copyList = new List<RitualMoneyRecord>( q.RitualMoneyRecords);
                //foreach (var j in q.RitualMoneyRecords) {
                //     if (DateTime.Parse(j.BorrowDate).Year != q.Year) {
                //         copyList.Remove(j);
                //     }
                // }
                foreach (var j in q.RitualMoneyRecords)
                {
                    if (j.BorrowDate.Year != q.Year)
                    {
                        copyList.Remove(j);
                    }
                }
                q.RitualMoneyRecords = copyList;
                
                    
            }
            if (q.Month != null)
            {
                if (q.Month < 1 || q.Month > 12)
                {
                    result.IsSuccess = false;
                    result.Message = "月份請輸入1~12月.";
                    return result;
                }
                var copyList = new List<RitualMoneyRecord>(q.RitualMoneyRecords);
                foreach (var j in q.RitualMoneyRecords)
                {
                    //if (DateTime.Parse(j.BorrowDate).Month != q.Month)
                    //{
                    //    copyList.Remove(j);
                    //}
                    if (j.BorrowDate.Month != q.Month)
                    {
                        copyList.Remove(j);
                    }
                }
                q.RitualMoneyRecords = copyList;

            }

            result.IsSuccess = true;
            result.Data = q.RitualMoneyRecords.OrderByDescending(o=>o.RitualMoneyRecordId);
          
            return result;
        }
      

        /// <summary>
        /// 輸入借用發財金
        /// 檢查姓名+身分證字號
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> Post([FromBody]BorrowViewModel b)
        {
            ResultModel result = new ResultModel();
            RitualMoneyRecord record = new RitualMoneyRecord();
            if (!string.IsNullOrEmpty(b.Name)) {
                b.Name = b.Name.Trim().ToUpper();
            }
            var query = from m in _context.members
                        where (string.IsNullOrEmpty(b.Name) ? true : m.Name.ToUpper()==(b.Name)) &&
                              (string.IsNullOrEmpty(b.Identity) ? true : m.Identity == b.Identity)
                              select m;

            //查無會員
            if (query.Count() ==0)
            {
                result.IsSuccess = false;
                result.Message = "查無資料.";
                return result;
            }
            //查詢到多筆同名會員
            if (query.Count() > 1) {
                result.IsSuccess = false;
                result.Message= "查詢到多筆同名會員"; 
                return result;
            }
            //
            var recordQuery = from o in _context.RitualMoneyRecords
                              where (o.MemberId == query.SingleOrDefault().MemberId)&&
                              (o.IsReturn==false)
                              select o;
            if (recordQuery.Count() > 0) {
                result.IsSuccess = false;
                result.Message = "尚有未還願的借用紀錄";
                return result;
            }
            try
            {
               
                var member = query.SingleOrDefault();
                record.MemberId = member.MemberId;
                record.BorrowDate = DateTime.Now.ToLocalTime();
                record.BorrowAmount = b.BorrowAmount;
                //_context.RitualMoneyRecords.Add(record);
                await _context.RitualMoneyRecords.AddAsync(record);
                await _context.SaveChangesAsync();
                result.IsSuccess = true;
                result.Message = "借用發財金成功.";

            }
            catch (Exception e)
            {
                result.IsSuccess = false;
                result.Message = "DB error.";
            }
            var accessToken = Request.Headers["Authorization"];
            var user = await _TokenGetUserHelper.GetUser(accessToken);
            //logger.Info("userId=" + user.Id + ", username=" + user.UserName + $"\nCreate " + "RitualMoney id= " + record.RitualMoneyRecordId +" successfully.");

            return result;
        }
        

        /// <summary>
        /// 輸入還願發財金
        /// 檢查姓名+身分證字號
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        public async Task<ResultModel> Put([FromBody]ReturnViewModel r)
        {
            ResultModel result = new ResultModel();
            ///檢查輸入會員 是否存在於DB
            if (!string.IsNullOrEmpty(r.Name))
            {
                r.Name = r.Name.Trim().ToUpper();
            }
            var query = from m in _context.members
                        where (string.IsNullOrEmpty(r.Name) ? true : m.Name.ToUpper().Equals(r.Name))
                        && (string.IsNullOrEmpty(r.Identity) ? true : m.Identity == r.Identity)
                        select m;

            if (query.Count() == 0)
            {
                result.IsSuccess = false;
                result.Message = "查無會員";
                return result;
            }
            if (query.Count() > 1)
            {
                result.IsSuccess = false;
                result.Message = "查詢到多筆同名會員";
                return result;
            }

         

            RitualMoneyRecord recordToUpdate = new RitualMoneyRecord();
          
            ///撈出該會員所有未還記錄
            var records = _context.Entry(query.SingleOrDefault())
                .Collection(q => q.RitualMoneyRecords)
                .Query()
                .Where(r => r.IsReturn == false)
                .ToList();

            //無紀錄
            if (records.Count() == 0)
            {
                result.IsSuccess = false;
                result.Message = "查無尚未還款的借用紀錄";
            }
            else { 
                 try {
                    recordToUpdate = records.FirstOrDefault();
                    recordToUpdate.IsReturn = true;
                    recordToUpdate.ReturnAmount = r.ReturnAmount;
                    recordToUpdate.ReturnDate = DateTime.Now.ToLocalTime();
                    await _context.SaveChangesAsync();
                    result.IsSuccess = true;
                    result.Message = "還願成功";
                }
                catch (Exception e) {
                    result.IsSuccess = false;
                    result.Message = "DB error.";
                }
            }

            var accessToken = Request.Headers["Authorization"];
            var user = await _TokenGetUserHelper.GetUser(accessToken);
            //logger.Info("userId=" + user.Id + ", username=" + user.UserName + $"\nEdit Return " + "RitualMoney id= " + recordToUpdate.RitualMoneyRecordId + " successfully.");

            return result;
        }

        ///  <summary>
        ///修改紀錄
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpPut("{id}")]
        public async Task<ResultModel> Put(int id,[FromBody] EditRecord editRecord)
        {
            var result = new ResultModel();
            bool check = false;
            //檢查是否空請求
            foreach (PropertyInfo property in editRecord.GetType().GetProperties()) {
                if (property.GetValue(editRecord) != null)
                {
                    check = true;
                    break;
                }
            }
            if (check == false) {
                result.IsSuccess = false;
                result.Message = "請輸入資料";
                return result;
            }
            //================
            var recordToUpdate = _context.RitualMoneyRecords.FirstOrDefault(r => r.RitualMoneyRecordId == id);

            if (recordToUpdate!=null)
            {
                //限制未還款的紀錄先去還款頁面處理
                if (recordToUpdate.IsReturn == false && editRecord.ReturnAmount != null) {
                    result.IsSuccess = false;
                    result.Message = "尚有未還願的借用紀錄";
                    return result;
                }
                if (editRecord.BorrowAmount!=null)
                    recordToUpdate.BorrowAmount = editRecord.BorrowAmount;
                if (editRecord.ReturnAmount != null)
                    recordToUpdate.ReturnAmount = editRecord.ReturnAmount;
                try
                {
                    await _context.SaveChangesAsync();
                    result.IsSuccess = true;
                    result.Message = "編輯成功";

                }
                catch (Exception e) {
                    result.IsSuccess = false;
                    result.Message = "DB error.";
                }
                
            }
            else
            {
                result.Message = "查無發財金紀錄";
                result.IsSuccess = false;
            }

            var accessToken = Request.Headers["Authorization"];
            var user = await _TokenGetUserHelper.GetUser(accessToken);
            //logger.Info("userId=" + user.Id + ", username=" + user.UserName + $"\nEdit " + "RitualMoney id= " + recordToUpdate.RitualMoneyRecordId + " successfully.");


            return result;

        }

        ///  <summary>
        ///刪除借還款資料
        /// </summary>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<ResultModel> Delete(int id) {
            var result = new ResultModel();
            try {
                RitualMoneyRecord recordToDelete = await _context.RitualMoneyRecords.FindAsync(id);
                if (recordToDelete == null) {
                    result.IsSuccess = false;
                    result.Message = "查無發財金紀錄.";
                    return result;
                }
                else {
                    _context.RitualMoneyRecords.Remove(recordToDelete);
                    await _context.SaveChangesAsync();
                    result.IsSuccess = true;
                    result.Message = "刪除紀錄成功.";
                }

            }
            catch (Exception e)
            {
                result.IsSuccess = false;
                result.Message = "DB error.";
            }

            var accessToken = Request.Headers["Authorization"];
            var user = await _TokenGetUserHelper.GetUser(accessToken);
            //logger.Info("userId=" + user.Id + ", username=" + user.UserName + $"\nDelete " + "RitualMoney id= " + id + " successfully.");

            return result;
        }

    }
}