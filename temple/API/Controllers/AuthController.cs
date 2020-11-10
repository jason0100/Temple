using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.Dtos;
using API.Helpers;
using API.Models;
using API.Models.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using NLog;

namespace API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _repo;
        private readonly IConfiguration _config;
        private readonly IEmailSender _emailSender;
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private readonly ITokenGetUserHelper _TokenGetUserHelper;

        public AuthController(IAuthRepository repo, IConfiguration config, IEmailSender emailSender, ITokenGetUserHelper TokenGetUserHelper) 
        {
            _repo = repo;
            _config = config;
            _emailSender = emailSender;
            _TokenGetUserHelper = TokenGetUserHelper;
        }
        
        [Authorize]
        [HttpPost("register")]//<host>/api/auth/register
        public async Task<ResultModel> Register([FromBody]UserForRegisterDto userForRegisterDto)
        {
            ResultModel result = new ResultModel();
            userForRegisterDto.Username = userForRegisterDto.Username.ToLower(); //Convert username to lower case before storing in database.

            if (await _repo.UserExists(userForRegisterDto.Username)) {
                result.IsSuccess = false;
                result.Message = "Username is already taken";
                return result;
            }
                //return BadRequest("Username is already taken");

            var userToCreate = new User
            {
                UserName = userForRegisterDto.Username
            };

            var createUser = await _repo.Register(userToCreate, userForRegisterDto.Password);

            //return StatusCode(201);//Create請求成功且新的資源成功被創建，通常用於 POST 或一些 PUT 請求後的回應。
            result.IsSuccess = true;
            result.Message = "Create success";
            return result;
        }

        [HttpPost("login")]
        public async Task<ResultModel> Login([FromBody] UserForLoginDto userForLoginDto)
        {
            ResultModel result = new ResultModel();
            var accessToken = Request.Headers["Authorization"];
            
            var loginUser = await _repo.Login(userForLoginDto.Username.ToLower(), userForLoginDto.Password);
            if (loginUser == null) //User login failed
            {
                result.IsSuccess = false;
                result.Message = "Username or Password wrong.";
                return result;
            }
         
            result.IsSuccess = true;
            result.Message = "Login success.";
            result.Data = loginUser.token;

            logger.Info("userId=" + loginUser.Id + ", username=" + loginUser.UserName + $"\nLogin at "+DateTime.Now.ToLongDateString()+' '+ result.Message);
            return result;
        }

        //}
        [HttpPost("ChangePassword")]
        public async Task<ResultModel> ChangePassword([FromBody]UserForChangePassword userForChangePassword)
        {
            ResultModel result = new ResultModel();
            var accessToken = Request.Headers["Authorization"];
            //var user = await _TokenGetUserHelper.GetUser(accessToken);

            result = await _repo.ChangePassword(userForChangePassword);

            logger.Info("username=" + userForChangePassword.Username + "\nChangePassword " + result.Message);
            return result;
        }

        [HttpPost("refreshtoken")]
        public async Task<ResultModel> RefreshToken([FromBody]Token token)
        {
            ResultModel result = new ResultModel();
            var accessToken = Request.Headers["Authorization"];
            var user = await _TokenGetUserHelper.GetUser(accessToken);
            result = await _repo.RefreshToken(token);
            logger.Info("userId=" + user.Id + ", username=" + user.UserName + $"\nRefreshToken " + result.Message);
            return result;
        }

        [HttpPost("RecoverPassword")]
        public async Task<ResultModel> RecoverPassword([FromBody]UserForRecoverPW user)
        { 
            var result = new ResultModel();
            result = await _repo.RecoverPassword(user);
            if (result.IsSuccess)
            {//set guid success
                //user = JsonConvert.DeserializeObject<UserForRecoverPW>(result.Data.ToString());
                var url = _config["web"];
                string message = url + "/User/ResetPassword?token=" + user.token + "&id=" + user.id;
                result = await _emailSender.SendEmailAsync(user.username, "Recover Password", message);
            }
          
            logger.Info("userId=" + user.id + ", username=" + user.username + $"\nRecoverPassword " + result.Message);
            return result;
        }

        [HttpPost("ResetPassword")]
        public async Task<ResultModel> ResetPassword([FromBody]UserForRecoverPW user)
        {
            var result = new ResultModel();
            if (String.IsNullOrEmpty(user.newPassword)) {
                result.IsSuccess = false;
                result.Message = "New password required.";
                return result;
            }
            
            result = await _repo.ResetPassword(user);
            logger.Info("userId=" + user.id + ", username=" + user.username + $"\nResetPassword " + result.Message);
            return result;
        }
    }
}