using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Models.Users
{
    public class TokenManager
    {
        private static IConfiguration _config;
        public TokenManager(IConfiguration config) {
            _config = config;
        }
        //金鑰，從設定檔或資料庫取得
        public string key = _config["TokenKey"];

        private readonly IHttpContextAccessor _contextAccessor;

        public TokenManager(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        ////產生 Token
        //public Token Create(User user)
        //{
        //    var exp = Convert.ToInt32( _config["TokenExpireTime"]);   //過期時間(分)

        //    //稍微修改 Payload 將使用者資訊和過期時間分開
        //    var payload = new Payload2
        //    {
        //        info = user,
        //        //Unix 時間戳
        //        exp = Convert.ToInt32(
        //            (DateTime.Now.AddMinutes(exp) -
        //             new DateTime(1970, 1, 1)).TotalSeconds)
        //    };

        //    var json = JsonConvert.SerializeObject(payload);
        //    var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
        //    var iv = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 16);

        //    //使用 AES 加密 Payload
        //    var encrypt = TokenCrypto
        //        .AESEncrypt(base64, key.Substring(0, 16), iv);

        //    //取得簽章
        //    var signature = TokenCrypto
        //        .ComputeHMACSHA256(iv + "." + encrypt, key.Substring(0, 64));

        //    return new Token
        //    {
        //        //Token 為 iv + encrypt + signature，並用 . 串聯
        //        access_token = iv + "." + encrypt + "." + signature,
        //        //Refresh Token 使用 Guid 產生
        //        refresh_token = Guid.NewGuid().ToString().Replace("-", ""),
        //        expires_in = exp,
        //    };
        //}

        ////取得使用者資訊
        //public User GetUser()
        //{
        //    string token = _contextAccessor.HttpContext.Request.Headers["Authoriaztion"];
            

        //    var split = token.Split('.');
        //    var iv = split[0];
        //    var encrypt = split[1];
        //    var signature = split[2];

        //    //檢查簽章是否正確
        //    if (signature != TokenCrypto
        //        .ComputeHMACSHA256(iv + "." + encrypt, key.Substring(0, 64)))
        //    {
        //        return null;
        //    }

        //    //使用 AES 解密 Payload
        //    var base64 = TokenCrypto
        //        .AESDecrypt(encrypt, key.Substring(0, 16), iv);
        //    var json = Encoding.UTF8.GetString(Convert.FromBase64String(base64));
        //    var payload = JsonConvert.DeserializeObject<Payload2>(json);

        //    //檢查是否過期
        //    if (payload.exp < Convert.ToInt32(
        //        (DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds))
        //    {
        //        return null;
        //    }

        //    return payload.info;
        //}
    }
}
