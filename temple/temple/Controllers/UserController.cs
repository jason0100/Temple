using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using temple.Helpers;
using temple.Models;
using temple.Models.Users;

namespace temple.Controllers
{
    public class UserController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _config;
        private readonly ICallApi _callApi;
        private readonly ICookieHelper _cookieHelper;
        /// <summary>
        /// 讀取組態用
        /// </summary>
     
        public UserController(IConfiguration config, ICallApi callApi, IHttpContextAccessor httpContextAccessor, ICookieHelper cookieHelper) {
            _config = config;
            _callApi = callApi;
            _httpContextAccessor = httpContextAccessor;
            _cookieHelper = cookieHelper;
        }

        [Authorize]
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Register(UserForRegister user, string ReturnUrl)
        {
            if (!ModelState.IsValid) {
                return View(user);
            }
            ResultModel result = new ResultModel();
            var data = JsonConvert.SerializeObject(user);
            result = await _callApi.CallAPI(data, new Uri(_config["api"].ToString() + "/auth/register"),"POST");


            TempData["IsSuccess"] = result.IsSuccess;
            TempData["msg"] = result.Message;
            if (!result.IsSuccess)
                return View(user);
            if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
            {
                return Redirect(ReturnUrl);//導到原始要求網址
            }

            return RedirectToAction(nameof(Register));
        }


        [HttpGet]
        public async Task<IActionResult> Login()
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(UserForLogin user, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(user);
            }
          
            ResultModel result = new ResultModel();
         
            var data = JsonConvert.SerializeObject(user);
            result = await _callApi.CallAPI(data, new Uri(_config["api"].ToString() + "/auth/login"), "POST");

            TempData["IsSuccess"] = result.IsSuccess;
            TempData["msg"] = result.Message;

            if (result.IsSuccess) {
                var LoginUser = new LoginUser();
                LoginUser = JsonConvert.DeserializeObject<LoginUser>(result.Data.ToString());
                double LoginExpireMinute = Convert.ToDouble(_config["LoginExpireMinute"]);
                if (user.keepLogin) {
                    LoginExpireMinute = 43200;
                }
                var splitToken = LoginUser.access_token.Split('.');
                var payloadBase64 = splitToken[1];
                payloadBase64 = payloadBase64.PadRight(payloadBase64.Length + (4 - payloadBase64.Length % 4) % 4, '=');
                var payloadText = Encoding.UTF8.GetString(Convert.FromBase64String(payloadBase64));

                var payload = new Payload();
                payload = JsonConvert.DeserializeObject<Payload>(payloadText);

                var Set_access_token = new ResultModel();
                var Set_refresh_token = new ResultModel();
                var Cookieoptions = new CookieOptions();
                Cookieoptions.Expires = DateTime.Now.AddMinutes(LoginExpireMinute);
                Cookieoptions.SameSite = SameSiteMode.Strict;
                Cookieoptions.HttpOnly = true;

                Set_access_token = _cookieHelper.Set("access_token", LoginUser.access_token, Cookieoptions);
                Set_refresh_token= _cookieHelper.Set("refresh_token", LoginUser.refresh_token, Cookieoptions);
                if (!Set_access_token.IsSuccess || !Set_refresh_token.IsSuccess)
                {
                    TempData["msg"] = "Write cookies error.";
                    return View();
                }

                //建立 Claim，也就是要寫到 Cookie 的內容
                var claims = new[] { new Claim("UserId", payload.nameid.ToString()),
                                       new Claim("Name", payload.unique_name),

                };
             
                //建立證件，類似你的駕照或護照
                ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                //將 ClaimsIdentity 設定給 ClaimsPrincipal (持有者) 
                ClaimsPrincipal principal = new ClaimsPrincipal(claimsIdentity);
            
                //登入動作
                await HttpContext.SignInAsync(principal, new AuthenticationProperties()
                {
                    //是否可以被刷新
                    AllowRefresh = true,
                    // 設置了一個 1 天 有效期的持久化 cookie
                    IsPersistent = user.keepLogin, //IsPersistent = false，瀏覽器關閉即刻登出
                    //用戶頁面停留太久，逾期時間，在此設定的話會覆蓋Startup.cs裡的逾期設定
                    ExpiresUtc = DateTime.Now.AddMinutes(LoginExpireMinute),
                    
                    
                });
            }
            
       
            if (!result.IsSuccess)
                return View(user);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);//導到原始要求網址
            }
            else
            {
                return RedirectToAction("Index", "Home");//到登入後的第一頁，自行決定
            }
        }


        [HttpGet]
        public async Task<IActionResult> Logout()
        {
          
            await HttpContext.SignOutAsync();
            foreach (var cookieKey in Request.Cookies.Keys)
                HttpContext.Response.Cookies.Delete(cookieKey);
            return RedirectToAction(nameof(Login));

        }

        [Authorize]
        public async Task<IActionResult> GetClaims() {

            var model = new GetClaimViewModel();
                       var identity = HttpContext.User.Identity as ClaimsIdentity;

            //從Cookie取出token
            var Get_access_token = new ResultModel();
            Get_access_token = _cookieHelper.Get("access_token");
            if (Get_access_token.IsSuccess)
            {
                TempData["access_token"] = Get_access_token.Data.ToString();
            }
            var Get_refresh_token = new ResultModel();
            Get_refresh_token = _cookieHelper.Get("refresh_token");
            if (Get_refresh_token.IsSuccess)
            {
                TempData["refresh_token"] = Get_refresh_token.Data.ToString();
            }
            

            if (identity != null)
            {
                model.claims = identity.Claims;
             }
           
            return View(model);
        }

        [Authorize]
        public async Task<IActionResult> ChangePassword()
        {
            return View();
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(UserForChangePassword model)
        {
            if (!ModelState.IsValid) {
                return View(model);
            }

            ResultModel result = new ResultModel();
            var data = JsonConvert.SerializeObject(model);
            result = await _callApi.CallAPI(data, new Uri(_config["api"].ToString() + "/auth/ChangePassword"), "POST");
            TempData["IsSuccess"] = result.IsSuccess;
            TempData["msg"] = result.Message;
            if (result.IsSuccess) {
                await HttpContext.SignOutAsync();
                return RedirectToAction(nameof(Login));
            }
           
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> RecoverPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecoverPassword(UserForRecoverPW model)
        {
            ResultModel result = new ResultModel();
            var data = JsonConvert.SerializeObject(model);
            result = await _callApi.CallAPI(data, new Uri(_config["api"].ToString() + "/auth/RecoverPassword"), "POST");
            TempData["IsSuccess"] = result.IsSuccess;
            TempData["msg"] = result.Message;
            if (result.IsSuccess == true)
            {
                TempData["SendMailSuccess"] = "重設密碼郵件已寄出，請於15分鐘內點擊郵件內的連結。ps.請留意垃圾信件匣";
            }

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ResetPassword(UserForRecoverPW model,string url)
        {
            if (String.IsNullOrEmpty(model.token) || model.id == 0) {
                return RedirectToAction(nameof(Login));
            }
            
            
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(UserForRecoverPW model)
        {
            ResultModel result = new ResultModel();
            if (String.IsNullOrEmpty(model.newPassword) || String.IsNullOrEmpty(model.ConfirmNewPassword))
            {
                TempData["IsSuccess"] = false;
                TempData["msg"] = "請確認密碼欄位均輸入";
                return View(model);
            }
            else
            {
                if (model.newPassword.Trim() != model.ConfirmNewPassword.Trim())
                {
                    TempData["IsSuccess"] = false;
                    TempData["msg"] = "兩次密碼輸入不相同";
                    return View(model);
                }
              
                var data = JsonConvert.SerializeObject(model);
                result = await _callApi.CallAPI(data, new Uri(_config["api"].ToString() + "/auth/ResetPassword"), "POST");
                if (result.IsSuccess == true)
                {
                    TempData["SendMailSuccess"] = "重設密碼郵件已寄出，請於15分鐘內點擊郵件內的連結。ps.請留意垃圾信件匣";
                }
                TempData["IsSuccess"] = result.IsSuccess;
                TempData["msg"] = "重設密碼成功";
                return RedirectToAction(nameof(Login));
            }
            
        }

    }
}