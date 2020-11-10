
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;

using System.Net.Http;
using System.Net.Http.Headers;
using temple.Models.Upload;
using temple.Models;
using System.Runtime.Versioning;

namespace temple.Helpers
{
    public class httpclient : Ihttpclient
    {
        private readonly IConfiguration _config;
        private readonly ICookieHelper _cookieHelper;
        public httpclient(IConfiguration config, ICookieHelper cookieHelper)
        {
            _config = config;
            _cookieHelper = cookieHelper;
        }
        public async Task<ResultModel> CallApi(List<IFormFile> files, string url, string folder)
        {
            var result = new ResultModel();

            var Get_access_token = new ResultModel();
            Get_access_token = _cookieHelper.Get("access_token");
            if (Get_access_token.IsSuccess)
            {


                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization
                             = new AuthenticationHeaderValue("Access-Control-Allow-Origin", _config["api"]);
                    httpClient.DefaultRequestHeaders.Authorization
                             = new AuthenticationHeaderValue("Bearer", Get_access_token.Data.ToString());
                    using (var form = new MultipartFormDataContent())
                    {

                        foreach (var i in files)
                        {

                            var fs = i.OpenReadStream();
                            var streamContent = new StreamContent(fs);
                            var fileContent = new ByteArrayContent(await streamContent.ReadAsByteArrayAsync());
                            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
                            form.Add(fileContent, "files", i.FileName);
                        }
                        form.Add(new StringContent(folder, Encoding.UTF8), "folder");
                        try
                        {
                            HttpResponseMessage response = await httpClient.PostAsync(url, form);
                            string strResult = await response.Content.ReadAsStringAsync();
                            result = JsonConvert.DeserializeObject<ResultModel>(strResult.ToString());
                        }
                        catch (Exception e)
                        {
                            var i = e;
                        }
                    }

                }
            }
            else {
                result.IsSuccess = false;
                result.Message = "請重新登入";
            }
            return result;
        }


    }
}
