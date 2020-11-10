using API.Models.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
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
using Microsoft.AspNetCore.Identity;
using temple.Models;

namespace temple.Helpers
{
    public class CallApi:ICallApi
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICookieHelper _cookieHelper;
        private readonly IConfiguration _config;
        public CallApi(IHttpContextAccessor httpContextAccessor, IConfiguration config, ICookieHelper cookieHelper) {
            _httpContextAccessor = httpContextAccessor;
           _config = config;
            _cookieHelper = cookieHelper;
        }
           
        public async Task<ResultModel> CallAPI(string data, Uri url, string method)
        {
            HttpWebRequest request;
            request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = method;
            request.ContentType = "application/json";

            if (method == "POST" || method == "PUT")
            {
                request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = method;
                request.ContentType = "application/json";
                //要發送的字串轉為byte[]
                byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(data);

                using (Stream reqStream = request.GetRequestStream())
                {
                    reqStream.Write(byteArray, 0, byteArray.Length);
                }
            }
            var cookieHelper = new CookieHelper(_httpContextAccessor, _config);
            var Get_access_token = new ResultModel();
            Get_access_token = _cookieHelper.Get("access_token");
            if (Get_access_token.IsSuccess) {
                request.Headers.Add("Authorization", "Bearer " + Get_access_token.Data);
            }
            
            ResultModel result = new ResultModel();
            string responseStr = "test";
            //發出Request
            //接收回應
            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                    {
                        responseStr = reader.ReadToEnd();
                        result = JsonConvert.DeserializeObject<ResultModel>(responseStr);
                    }
                }
            }
            catch (WebException e)
            {
                result.IsSuccess = false;
                using (HttpWebResponse response = e.Response as HttpWebResponse)
                {
                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        result.Message = "401";
                        var RefreshTokenResult = new ResultModel();
                        RefreshTokenResult = await RefreshMyToken();
                     
                        if (RefreshTokenResult.IsSuccess)
                        {
                            result.IsSuccess=false;
                            result.Message = "Token已更新，請重新整理頁面";
                            //result = await CallAPI(data, url, method);//會抓到舊cookie 有bug
                        }
                        else
                        {
                            result = RefreshTokenResult;
                            //if(result.Data!=null)
                                if (result.Message.ToString().Contains("IDX10000")) {
                                    result.Message="Token過期,請重新登入，或是勾選保持登入";
                                }
                            
                        }
                        
                    }

                    else
                    {
                        using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                        {
                            responseStr = reader.ReadToEnd();
                            result.IsSuccess = false;
                            result.Message = responseStr;

                        }
                    }
                }
            }
            return result;
        }

        public async Task<ResultModel> RefreshMyToken()
        {
          
            var token = new Token();
            var result = new ResultModel();
            try
            {
                //從Cookie取出token
                var Get_access_token = new ResultModel();
                Get_access_token = _cookieHelper.Get("access_token");
                if (Get_access_token.IsSuccess) {
                    token.access_token = Get_access_token.Data.ToString();
                }
                var Get_refresh_token = new ResultModel();
                Get_refresh_token = _cookieHelper.Get("refresh_token");
                if (Get_refresh_token.IsSuccess)
                {
                    token.refresh_token = Get_refresh_token.Data.ToString();
                }
                var tokenHandler = new JwtSecurityTokenHandler();
              
                var jwtToken = tokenHandler.ReadToken(token.access_token) as JwtSecurityToken; // 將字符串token解碼成token對象
                result.IsSuccess = false;
                result.Message = "Token unauthorized.";
                var tokenExpireTime = jwtToken.ValidTo.ToLocalTime();
                if (tokenExpireTime < DateTime.Now)
                {
                    var data = JsonConvert.SerializeObject(token);
                    result = await CallAPI(data, new Uri(_config["api"].ToString() + "/auth/RefreshToken"), "POST");
                    if (!result.IsSuccess)
                    {
                        return result;
                    }
                    else
                    {
                        var newToken = JsonConvert.DeserializeObject<Token>(result.Data.ToString());
                        //建立Cookie
                        double LoginExpireMinute = Convert.ToDouble(_config["LoginExpireMinute"]);
                        var Remove_access_token = new ResultModel();
                        Remove_access_token = _cookieHelper.Remove("access_token");
                        var Remove_refresh_token = new ResultModel();
                        Remove_refresh_token = _cookieHelper.Remove("refresh_token");
                        if (Remove_access_token.IsSuccess == false || Remove_refresh_token.IsSuccess == false) {
                            result.IsSuccess = false;
                            result.Message = "Delete cookies fail.";
                            return result;
                        }
                        _cookieHelper.Remove("refresh_token");
                        CookieOptions cookieOptions = new CookieOptions();
                        cookieOptions.Expires = new DateTimeOffset(DateTime.Now.AddMinutes(LoginExpireMinute));
                        var Set_access_token = new ResultModel();
                        Set_access_token = _cookieHelper.Set("access_token", newToken.access_token, cookieOptions);
                        var Set_refresh_token = new ResultModel();
                        Set_refresh_token = _cookieHelper.Set("refresh_token", newToken.refresh_token, cookieOptions);
                        if (!Set_access_token.IsSuccess || !Set_refresh_token.IsSuccess)
                        {
                            throw new Exception("Write cookies error.");
                        }
                        
                        
                    }

                }

            }
            catch (Exception e) {
                result.Message = e.Message;

            }
           
            return result;
        }
    }
}
