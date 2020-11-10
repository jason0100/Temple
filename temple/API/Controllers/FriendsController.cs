using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using API.Data;
using API.Helpers;
using API.Models;
using API.Models.Friends;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using NLog;

namespace API.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class FriendsController : ControllerBase
    {
        private TempleDbContext _context;
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private readonly ITokenGetUserHelper _TokenGetUserHelper;
        public FriendsController(TempleDbContext context, ITokenGetUserHelper TokenGetUserHelper)
        {
            _context = context;
            _TokenGetUserHelper = TokenGetUserHelper;
        }

        /// <summary>
        /// 查詢友宮資料
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ResultModel> Get([FromQuery]string Name)
        {
            var result = new ResultModel();
            if (!String.IsNullOrEmpty(Name))
                Name = Name.Trim();
            var query = from f in _context.Friends
                        .Include(c => c.city)
                        .Include(t => t.townShip)
                        where (String.IsNullOrEmpty(Name) ? true : f.Name.Contains(Name))
                        
                        //orderby f.Name
                        select f;

            if (query.Count() > 0)
            {
                result.IsSuccess = true;
                result.Data = query;
            }
            else
            {
                result.Message = "查無友宮";
                result.IsSuccess = false;
            }
            return result;
        }

        [HttpGet("{id}")]
        public async Task<ResultModel> Get(int id)
        {
            var result = new ResultModel();
      

            var query = from f in _context.Friends
                      .Include(c => c.city)
                      .Include(t => t.townShip)
                        where id==f.Id
                        select f;

            if (query.Count()!= 0)
            {
                result.IsSuccess = true;
                result.Data = query.SingleOrDefault();
            }
            else
            {
                result.Message = "查無友宮";
                result.IsSuccess = false;
            }

            return result;
        }

        /// <summary>
        /// 新增友宮資料
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> Post([FromBody]Friend f)
        {
            var result = new ResultModel();
            result.Message = f.Name;
                    
            var query = from o in _context.Friends
                        where o.Name == f.Name
                        select o;
            if (query.Count() > 0) {
                result.IsSuccess = false;
                result.Message = f.Name + " exist.";
                return result;
            }
            var queryCity = _context.Cities.Include(c=>c.TownShips).FirstOrDefault(c => c.Id == f.CityId);
            if (queryCity == null) {
                result.IsSuccess = false;
                result.Message = "CityId not exist.";
                return result;
            }
            var queryTownships = queryCity.TownShips.FirstOrDefault(t => t.Id == f.TownshipId);
            if (queryTownships == null)
            {
                result.IsSuccess = false;
                result.Message = "TownshipId not exist.";
                return result;
            }

            try
            {
                await _context.Friends.AddAsync(f);
                result.IsSuccess = true;
                result.Message = "添加友宮成功";
                await _context.SaveChangesAsync();
                result.Data = new {
                    Id = f.Id
                };
            }
            catch (Exception e)
            {
                result.IsSuccess = false;
                result.Message = "DB ERROR";
            }
            var accessToken = Request.Headers["Authorization"];
            var user = await _TokenGetUserHelper.GetUser(accessToken);
            logger.Info("userId=" + user.Id + ", username=" + user.UserName + $"\n Create " + "Friends id= " + f.Id +", name="+f.Name+ " successfully.");
            return result;
        }


        /// <summary>
        /// 編輯友宮資料
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<ResultModel> Put([FromBody]FriendForEdit f)
        {
            var result = new ResultModel();
            result.Message = f.Name;
           
            Friend friendToUpdate = await _context.Friends.FindAsync(f.Id);
            if (friendToUpdate == null) {
                result.IsSuccess = false;
                result.Message = "查無友宮";
                return result;
            }
           
            //檢查名稱是否重複
            var query = from o in _context.Friends
                        where o.Name == f.Name
                        select o;
            if (query.Count() > 0)
            {//排除跟自己同名
                if (query.FirstOrDefault().Id != f.Id) {
                    result.IsSuccess = false;
                    result.Message ="Name: "+ f.Name + " exist.";
                    return result;
                }
            }
              
            try
            {

                _context.Entry(friendToUpdate).CurrentValues.SetValues(f);
                result.IsSuccess = true;
                result.Message = "編輯友宮資料成功";
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                result.IsSuccess = false;
                result.Message = "DB ERROR";
            }

            var accessToken = Request.Headers["Authorization"];
            var user = await _TokenGetUserHelper.GetUser(accessToken);
            logger.Info("userId=" + user.Id + ", username=" + user.UserName + $"\n Edit " + "Friends id= " + f.Id + ", name=" + f.Name + " successfully.");
            return result;
        }


        /// <summary>
        /// 檢查友宮資料
        /// </summary>
        /// <returns></returns>
        public ResultModel CheckFriend(FriendForEdit f)
        {
            var result = new ResultModel();
            result.IsSuccess = true;
            DateTime dateValue;
            
            ///輸入資料檢查
            ///
            foreach (PropertyInfo property in f.GetType().GetProperties())
            {
                if (property.GetValue(f) != null)
                {
                    result.IsSuccess = true;
                    break;
                }
            }
            if (!result.IsSuccess) {
                result.Message = "輸入資料為空";
                return result;
            }
            if (!string.IsNullOrEmpty(f.Name))
                f.Name = f.Name.Trim();
         
            if (!string.IsNullOrEmpty(f.ActivityDate))
            {

                f.ActivityDate = f.ActivityDate.Trim();
                if (!DateTime.TryParse(f.ActivityDate, out dateValue))
                {
                    result.IsSuccess = false;
                    result.Message += "活動日期不存在";
                }
            }
            if (!string.IsNullOrEmpty(f.Notes))
                f.Notes = f.Notes.Trim();
            if (!string.IsNullOrEmpty(f.ContactName))
                f.ContactName = f.ContactName.Trim();

            //if (!string.IsNullOrEmpty(f.Phone)) {
            //    f.Phone = f.Phone.Trim();
            //    string PhonePattern = @"0\d{1,2}-\d{6,8}";
            //    if (!Regex.IsMatch(f.Phone, PhonePattern)) {
            //        result.IsSuccess = false;
            //        result.Message += "請輸正確市話號碼, ex:02-123456.";
            //    }
            //}
            //if (!string.IsNullOrEmpty(f.CellPhone))
            //{
            //    f.CellPhone = f.CellPhone.Trim();
            //    string CellPhonePattern = @"^09\d{2}-\d{6}$";
            //    if (!Regex.IsMatch(f.CellPhone, CellPhonePattern))
            //    {
            //        result.IsSuccess = false;
            //        result.Message += "請輸正確手機號碼, ex:0912-123456.";
            //    }
            //}

            return result;

        }
        /// <summary>
        /// 刪除友宮資料
        /// </summary>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task< ResultModel >Delete(int id)
        {
            var result = new ResultModel();
            Friend friend = await _context.Friends.FindAsync(id);
            if (friend == null)
            {
                result.IsSuccess = false;
                result.Message = "查無資料";
            }
            else {
                try {
                    _context.Remove(friend);
                    await _context.SaveChangesAsync();
                    result.IsSuccess = true;
                    result.Message = "刪除成功";
                }
                catch (Exception e) {
                    result.IsSuccess = false;
                    result.Message = "DB error.";
                }
             }

            var accessToken = Request.Headers["Authorization"];
            var user = await _TokenGetUserHelper.GetUser(accessToken);
            logger.Info("userId=" + user.Id + ", username=" + user.UserName + $"\n Delete " + "Friends id= " + id +  " successfully.");
            return result;
        }


    }
}