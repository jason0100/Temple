using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using temple.Models;

namespace temple.Helpers
{
    public class CookieHelper : ICookieHelper
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IConfiguration _config;
        public CookieHelper(IHttpContextAccessor httpContextAccessor, IConfiguration config)
        {
            this.httpContextAccessor = httpContextAccessor;
            _config = config;
        }


        public ResultModel Set(string key, string value, CookieOptions options = null)
        {
            var result = new ResultModel();
            if (options == null)
            {
                options = new CookieOptions();
                options.Expires = DateTime.Now.AddMinutes(Convert.ToDouble(_config["LoginExpireMinute"]));
            }
            if (String.IsNullOrEmpty(key) || String.IsNullOrEmpty(value))
            {
                result.IsSuccess = false;
                result.Message = "Parameter required.";
                return result;
            }
            try
            {
                this.httpContextAccessor.HttpContext.Response.Cookies.Append(key, this.Encrypt(value), options);
                //this.httpContextAccessor.HttpContext.Response.Cookies.Append(key, (value), options);
                result.IsSuccess = true;
            }
            catch (Exception e)
            {
                result.IsSuccess = false;
                result.Message = e.Message;
            }
            return result;
        }

        public ResultModel Get(string key)
        {
            var result = new ResultModel();
            if (String.IsNullOrEmpty(key))
            {
                result.IsSuccess = false;
                result.Message = "Cookie name required.";
                return result;
            }
            var data = this.httpContextAccessor.HttpContext.Request.Cookies[key];
            if (data == null)
            {
                result.IsSuccess = false;
                result.Message = "Has no value.";
            }
            else
            {
                result = this.Decrypt(this.httpContextAccessor.HttpContext.Request.Cookies[key]);
                //result.IsSuccess = true;
                //result.Data = (this.httpContextAccessor.HttpContext.Request.Cookies[key]);
            }
            return result;
        }

        public ResultModel Remove(string key)
        {
            var result = new ResultModel();
            try
            {
                this.httpContextAccessor.HttpContext.Response.Cookies.Delete(key);
                result.IsSuccess = true;
            }
            catch (Exception e)
            {
                result.IsSuccess = false;
                result.Message = e.Message;
            }
            return result;
        }

        public string Encrypt(string value)
        {
            string base64 = Convert.ToBase64String(Encoding.Default.GetBytes(value));
            var result = new ResultModel();

            //這裡使用Aes創建編譯物件
            using (Aes aes = System.Security.Cryptography.Aes.Create())
            {
                var CryptoKey = _config["key"];
                MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
                SHA256CryptoServiceProvider sha256 = new SHA256CryptoServiceProvider();
                byte[] key = sha256.ComputeHash(Encoding.UTF8.GetBytes(CryptoKey));
                byte[] iv = md5.ComputeHash(Encoding.UTF8.GetBytes(CryptoKey));
                aes.Key = key;//加密金鑰(32 Byte)
                aes.IV = iv;//初始向量(Initial Vector, iv) 
                //aes.Mode = CipherMode.CBC;
                //aes.Padding = PaddingMode.Zeros;

                //創建密碼編譯轉換運算
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                //使用記憶體串流來儲存結果，
                //執行加密
                byte[] cryptTextBytes = Encoding.Unicode.GetBytes(base64);

                byte[] cryptedText = encryptor.TransformFinalBlock(cryptTextBytes, 0, cryptTextBytes.Length);
                byte[] array64 = new byte[(int)Math.Ceiling(cryptedText.Length / 6.0) * 6];//新增byte array 長度為加密字串的6的倍數
                Array.Copy(cryptedText, array64, cryptedText.Length);
                result.IsSuccess = true;
                result.Message = Convert.ToBase64String(cryptedText);


            }
            return result.Message;
        }

        public ResultModel Decrypt(string CipherText64)
        {
            var CryptoKey = _config["key"];
            var result = new ResultModel();
            try
            {
                //檢查參數
                if (CipherText64 == null || CipherText64.Length <= 0)
                    throw new ArgumentNullException("沒有要解密的內容");
                using (Aes aes = System.Security.Cryptography.Aes.Create())
                {//這裡使用Aes創建編譯物件
                    MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
                    SHA256CryptoServiceProvider sha256 = new SHA256CryptoServiceProvider();
                    byte[] key = sha256.ComputeHash(Encoding.UTF8.GetBytes(CryptoKey));//加密金鑰用sha256產生
                    byte[] iv = md5.ComputeHash(Encoding.UTF8.GetBytes(CryptoKey));//初始向量用md5產生
                    aes.Key = key; //加密金鑰(32 Byte)
                    aes.IV = iv; //初始向量(Initial Vector, iv) 
                                 //aes.Mode = CipherMode.CBC;
                                 //aes.Padding = PaddingMode.Zeros;

                    byte[] encryptTextBytes = (Convert.FromBase64String(CipherText64));//密文處理 轉成byte array

                    //創建密碼解譯轉換運算  //加密器
                    ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                    byte[] decryptedText = decryptor.TransformFinalBlock(encryptTextBytes, 0, encryptTextBytes.Length);
                    result.IsSuccess = true;
                    result.Data = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(Encoding.Unicode.GetString(decryptedText)));
                    result.Message = "Decryption Success";
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                result.IsSuccess = false;
                result.Message = e.Message;
            }
            return result;
        }
    }
}
