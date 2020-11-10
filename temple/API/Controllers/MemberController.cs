using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using API.Data;
using API.Helpers;
using API.Models;
using API.Models.MemberData;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace API.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class MemberController : ControllerBase
    {
        private TempleDbContext _context;
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private readonly ITokenGetUserHelper _TokenGetUserHelper;
        public MemberController(TempleDbContext context, ITokenGetUserHelper TokenGetUserHelper) {
            _context = context;
            _TokenGetUserHelper = TokenGetUserHelper;
        }

        /// <summary>
        /// 查詢會員
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>

        [HttpGet("{id}")]
        public async Task<ResultModel> Get(int id)
        {
            var result = new ResultModel();
            result.Data = _context.members.SingleOrDefault(c => c.MemberId == id);
            //result.Data = await _context.members.FindAsync(id);
            if (result.Data != null)
            {
                result.IsSuccess = true;
            }
            else {
                result.Message = "查無會員編號.";
                result.IsSuccess = false;
            }
            
            return result;
        }

        [HttpGet]
        public ResultModel Get([FromQuery]QueryMemberModel q) {
            var result = new ResultModel();
            bool check = true;
            ///檢查是否為查詢
            /////如為查詢則轉大寫
            //bool isQuery = false;
            if (!string.IsNullOrEmpty(q.Name)) {
                q.Name = q.Name.Trim().ToUpper();
            }

            if (!string.IsNullOrEmpty(q.Identity)) {
                q.Identity = q.Identity.Trim().ToUpper();
                ///
                ///檢查身分證字號
                ///
                string IdentityPattern = @"^[a-zA-Z][0-9]{9}$";
                check = Regex.IsMatch(q.Identity, IdentityPattern);
            }

            if (!string.IsNullOrEmpty(q.Gender)) {
                q.Gender = q.Gender.Trim();

            }


            if (!check) {
                result.IsSuccess = false;
                result.Message = "查詢條件錯誤.";
                return result;
            }


            var query = from m in _context.members
                         .Include(c => c.city)
                         .Include(t=>t.townShip)
                        where (string.IsNullOrEmpty(q.Name) ? true : m.Name.ToUpper().Contains(q.Name)) &&
                            (string.IsNullOrEmpty(q.Identity) ? true : m.Identity == q.Identity) &&
                            (string.IsNullOrEmpty(q.Gender) ? true : m.Gender == q.Gender)
                        select m;
            if (query.Count() != 0)
            {
                result.IsSuccess = true;
             
            }
            result.IsSuccess = true;
            result.Data = query;

            return result;
        }

        /// <summary>
        /// 新增會員
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>

        [HttpPost]
        public async Task<ResultModel> Post([FromBody]member n)
        {
            member newMember = new member();
            newMember = n;
            var result = new ResultModel();

            ResultModel checkMemberResult = CheckMember(newMember, "IsDuplicate");

            var accessToken = Request.Headers["Authorization"];
            var user = await _TokenGetUserHelper.GetUser(accessToken);

            if (checkMemberResult.IsSuccess)
            {

                /// <summary>
                ///註冊日期
                ///
                newMember.CreateDate = DateTime.Now.ToLocalTime().ToString();
                try
                {
                    await _context.members.AddAsync(newMember);
                    result.Data = n.MemberId.ToString();
                    result.IsSuccess = true;
                    result.Message = "會員添加成功.";
                    
                    await _context.SaveChangesAsync();
                    result.Data = new
                    {
                        MemberId = newMember.MemberId
                    };
                }
                catch (Exception e)
                {
                    result.IsSuccess = false;
                    result.Message = "DB ERROR";
                    logger.Warn("userId=" + user.Id + ", username=" + user.UserName + $"\nCreate " + "member id= " + newMember.MemberId + ", name=" + n.Name + " " + result.Message);
                    return result;
                }
            }
            else
            {
                //checkMemberResult fail
                result = checkMemberResult;

            }
        
            logger.Info("userId=" + user.Id + ", username=" + user.UserName + $"\nCreate " + "member id= " + newMember.MemberId+ ", name=" + n.Name +" "+ result.Message);
            return result;
        }
        /// <summary>
        /// 修改會員資料
        /// 需要提供MemberId+Identity 缺一不可
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<ResultModel> Put([FromBody]member m)
        {
            var result = new ResultModel();
            if (m.MemberId == 0) {
                result.IsSuccess = false;
                result.Message = "請輸入會員編號.";
                return result;
            }


            member memberToUpdate = await _context.members.FindAsync(m.MemberId);
            if (memberToUpdate != null)
            {
                
                ResultModel checkMemberResult = CheckMember(m, "IsDuplicate");

                if (checkMemberResult.IsSuccess)
                {
                    try
                    {
                        _context.Entry(memberToUpdate).CurrentValues.SetValues(m);
                        await _context.SaveChangesAsync();
                        result.IsSuccess = true;
                        result.Message = "編輯會員資料成功";
                    }
                    catch (Exception e)
                    {
                        result.IsSuccess = false;
                        result.Message = "DB ERROR";
                    }

                }
                else {
                    result = checkMemberResult;
                }

            }
            else {
                result.IsSuccess = false;
                result.Message = "查無資料";
            }

            var accessToken = Request.Headers["Authorization"];
            var user = await _TokenGetUserHelper.GetUser(accessToken);
            logger.Info("userId=" + user.Id + ", username=" + user.UserName + $"\nEdit " + "member id= " + m.MemberId + ", name=" + m.Name + " successfully.");

            return result;
        }
        /// <summary>
        /// 刪除會員資料
        /// 必要條件MemberId
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<ResultModel> Delete(int id) { 
            var result = new ResultModel();
            var accessToken = Request.Headers["Authorization"];
            var user = await _TokenGetUserHelper.GetUser(accessToken);

            try
            {
                member memberToDelete = await _context.members.FindAsync(id);
                if (memberToDelete == null)
                {
                    result.IsSuccess = false;
                    result.Message = "查無資料";
                    logger.Info("userId=" + user.Id + ", username=" + user.UserName + $"\nDelete " + "member id= " + id + " " + result.Message);
                    return result;
                }
                else {
                    _context.members.Remove(memberToDelete);
                    await _context.SaveChangesAsync();
                    result.IsSuccess = true;
                    result.Message = "刪除成功";
                }
            }
            catch {
                result.IsSuccess = false;
                result.Message = "DB ERROR";
            }

           
            logger.Info("userId=" + user.Id + ", username=" + user.UserName + $"\nDelete " + "member id= " + id +  " "+result.Message);

            return result;
        }

        /// <summary>
        /// 檢查會員資料
        /// </summary>
        /// <returns></returns>
        public ResultModel CheckMember(member n, string checkIdentity) {
            var result = new ResultModel();
            result.IsSuccess = true;
            DateTime dateValue;
        

            /// <summary>
            ///字串去頭尾空白
            /// <summary>
            if (!string.IsNullOrEmpty(n.ZodiacAnimal))
                n.ZodiacAnimal = n.ZodiacAnimal.Trim();
            if (!string.IsNullOrEmpty(n.TimeOfLunarBirth))
                n.TimeOfLunarBirth = n.TimeOfLunarBirth.Trim();
          
            if (!string.IsNullOrEmpty(n.Address))
                n.Address = n.Address.Trim();

            /// <summary>
            ///檢查Birth是否正確
            /// <summary>
            if (!String.IsNullOrEmpty(n.Birth))
            {
                if (!DateTime.TryParse(n.Birth, out dateValue))
                {
                    result.IsSuccess = false;
                    result.Message = "生辰(國曆)日期錯誤. ";
                    }
            }
            if (!String.IsNullOrEmpty(n.LunarBirth)) { 
                  if (!DateTime.TryParse(n.LunarBirth, out dateValue))
                {
                    result.IsSuccess = false;
                    result.Message += "生辰(農曆)不存在";
                }
            }


            ///<summary>
            ///檢查生肖是否正確
            ///</summary>
            if (!String.IsNullOrEmpty(n.ZodiacAnimal)) {
                string[] ZodiacAnimal = { "鼠", "牛", "虎", "兔", "龍", "蛇", "馬", "羊", "猴", "雞", "狗", "豬" };
                if (!Array.Exists(ZodiacAnimal, element => element == n.ZodiacAnimal))
                {
                    result.IsSuccess = false;
                    result.Message += "ZodiacAnimal not exist.";
                }
            }

            ///<summary>
            ///檢查時辰是否正確
            /// </summary>
            if (!String.IsNullOrEmpty(n.LunarBirth)) { 
                  string[] TimeOfLunarBirth = { "子", "丑", "寅", "卯", "辰", "巳", "午", "未", "申", "酉", "戌", "亥" };
                if (!Array.Exists(TimeOfLunarBirth, element => element == n.TimeOfLunarBirth))
                {
                    result.IsSuccess = false;
                    result.Message += "TimeOfLunarBirth not exist.";
                }
            }
          

            ///
            ///檢查身分證字號是否重複
            ///
            var query = from i in _context.members
                        where i.Identity == n.Identity
                        select i;
            if (checkIdentity == "IsExist" && query.FirstOrDefault() == null) {
                result.IsSuccess = false;
                result.Message += "查無身分證字號";
            }
            if (query.FirstOrDefault() != null) {
                //排除自己的
                if (query.FirstOrDefault().MemberId != n.MemberId)
                {
                    if (checkIdentity == "IsDuplicate" && query.FirstOrDefault() != null)
                    {
                        result.IsSuccess = false;
                        result.Message += "身分證字號重複.";
                    }
                }
            }
           
            

            return result;
        }
    }
}