using API.Dtos;
using API.Models;
using API.Models.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.IdentityModel.Tokens;
using System.Collections.Specialized;
using System.Security.Cryptography;

namespace API.Data
{
    public class AuthRepository:IAuthRepository
    {
        private readonly TempleDbContext _context;
        private readonly IConfiguration _config;
        public AuthRepository(TempleDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }
        public async Task<LoginUser> Login(string username, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.UserName == username); //Get user from database.
            if (user == null)
                return null; // User does not exist.

            if (!VerifyPassword(password, user.PasswordHash, user.PasswordSalt))
                return null;

            var loginUser = new LoginUser();
            loginUser.Id = user.Id;
            loginUser.UserName = user.UserName;
            loginUser.token = new Token();

            //===========generate token=================
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_config["TokenKey"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]{
                    new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.UserName)
                }),
                Expires = DateTime.Now.AddMinutes(Convert.ToInt32(_config["TokenExpireTime"])),

                // 建立一組對稱式加密的金鑰，主要用於 JWT 簽章之用
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature)
            };
             
            loginUser.token.access_token = tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
            
            loginUser.token.refresh_token = GenerateRefreshToken();
            loginUser.token.expires_in = Convert.ToInt32(_config["TokenExpireTime"]);

            user.refresh_token = loginUser.token.refresh_token;
            //===========END generate token=================
            _context.Entry(user).CurrentValues.SetValues(user);
            await _context.SaveChangesAsync(); // Save changes to database.
            return loginUser;
        }
        private bool VerifyPassword(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password)); // Create hash using password salt.
                for (int i = 0; i < computedHash.Length; i++)
                { // Loop through the byte array
                    if (computedHash[i] != passwordHash[i]) return false; // if mismatch
                }
            }
            return true; //if no mismatches.
        }

        public async Task<User> Register(User user, string password)
        {
            byte[] passwordHash, passwordSalt;
            CreatePasswordHash(password, out passwordHash, out passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            await _context.Users.AddAsync(user); // Adding the user to context of users.
            await _context.SaveChangesAsync(); // Save changes to database.

            return user;
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }
        public async Task<bool> UserExists(string username)
        {
            if (await _context.Users.AnyAsync(x => x.UserName == username))
                return true;
            return false;
        }

        public async Task<ResultModel> ChangePassword(UserForChangePassword user)
        {
            var result = new ResultModel();
            var existuser = await _context.Users.FirstOrDefaultAsync(x => x.UserName == user.Username); //Get user from database.
            if (existuser == null) {
                result.IsSuccess = false;
                result.Message = "User does not exist";
                return result;
            }


            if (!VerifyPassword(user.Password, existuser.PasswordHash, existuser.PasswordSalt)) {
                result.IsSuccess = false;
                result.Message = "Password error.";
                return result;
            }
                

            byte[] passwordHash, passwordSalt;
            CreatePasswordHash(user.NewPassword, out passwordHash, out passwordSalt);
 
            existuser.PasswordHash = passwordHash;
            existuser.PasswordSalt = passwordSalt;
            try
            {
                _context.Entry(existuser).CurrentValues.SetValues(existuser);
                result.IsSuccess = true;
                result.Message = "Change password success, please login again.";
            }
            catch (Exception e) {
                result.IsSuccess = false;
                result.Message = "DB error";
            }
         
            
            await _context.SaveChangesAsync(); // Save changes to database.

            return result;
        }

        public async Task<ResultModel> RefreshToken(Token token)
        {
            var result = new ResultModel();
            //================驗證token signature===========
            var tokenHandler = new JwtSecurityTokenHandler();
          
            var key = Encoding.ASCII.GetBytes(_config["TokenKey"]);
            var validationParameters = new TokenValidationParameters() // 生成驗證token的參數
            {
                RequireExpirationTime = false, // token是否包含有效期
                ValidateLifetime = false,
                ValidateIssuer = false, // 驗證秘鑰發行人，如果要驗證在這裏指定發行人字符串即可
                ValidateAudience = false, // 驗證秘鑰的接受人，如果要驗證在這裏提供接收人字符串即可
                IssuerSigningKey = new SymmetricSecurityKey(key) // 生成token時的安全秘鑰
            };
            SecurityToken securityToken; // 接受解碼後的token對象
            try
            {
                var jwtToken = tokenHandler.ReadToken(token.access_token) as JwtSecurityToken; // 將字符串token解碼成token對象
                var principal = tokenHandler.ValidateToken(token.access_token, validationParameters, out securityToken);
            }
            catch(Exception e) {
                result.IsSuccess = false;
                result.Message = "Token invalid.";
                return result;
            }
          //==============================================
            
            var payloadObj = new payload();
            var access_tokenSplit = token.access_token.Split(".");
            var payloadBase64 = access_tokenSplit[1];
            payloadBase64 = payloadBase64.PadRight(payloadBase64.Length + (4 - payloadBase64.Length % 4) % 4, '=');
            var payloadText = System.Text.Encoding.GetEncoding("utf-8").GetString(Convert.FromBase64String(payloadBase64));
            payloadObj = JsonConvert.DeserializeObject<payload>(payloadText);
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == payloadObj.nameid); //Get user from database.
            var loginUser = new LoginUser();
            loginUser.token = new Token();
            if (user.refresh_token == token.refresh_token)
            {
                //===========generate token=================
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]{
                    new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.UserName)
                }),
                    Expires = DateTime.Now.AddMinutes(Convert.ToInt32(_config["TokenExpireTime"])),
                    // 建立一組對稱式加密的金鑰，主要用於 JWT 簽章之用
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature)
                };

                loginUser.token.access_token = tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
                loginUser.token.refresh_token = GenerateRefreshToken();
                loginUser.token.expires_in = Convert.ToInt32(_config["TokenExpireTime"]);
                user.refresh_token = loginUser.token.refresh_token;
                _context.Entry(user).CurrentValues.SetValues(user);
                await _context.SaveChangesAsync(); // Save changes to database.
                                                   //===========END generate token=================
                result.IsSuccess = true;
                result.Message = "Token generate succeed.";
                result.Data = loginUser.token;
                return result;
            }
            else {
                result.IsSuccess = false;
                result.Message = "Refresh token invalid.";
                return result;
            }
           
        }

        public async Task<ResultModel> RecoverPassword(UserForRecoverPW user)
        {
            var result = new ResultModel();
            var existUser = _context.Users.FirstOrDefault(u=>u.UserName==user.username);
            if (existUser == null) {
                result.IsSuccess = false;
                result.Message = "Username not exist.";
                return result;
            }
            existUser.guid = Guid.NewGuid().ToString();
           
            try
            {
                _context.Entry(existUser).CurrentValues.SetValues(existUser);
                await _context.SaveChangesAsync(); // Save changes to database.
                result.IsSuccess = true;
                result.Message = "Set guid success.";
                
                //generate token
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_config["TokenKey"]);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]{
                    new Claim("GUID", existUser.guid)
                }),
                    Expires = DateTime.Now.AddMinutes(15),

                    // 建立一組對稱式加密的金鑰，主要用於 JWT 簽章之用
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature)
                };
                user.token = tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
                user.id = existUser.Id;
                result.Data = user;
            }
            catch (Exception e) {
                result.IsSuccess = false;
                result.Message = "DB error";
            }
            return result;
        }

        public async Task<ResultModel> ResetPassword(UserForRecoverPW user)
        {
            var result = new ResultModel();
            //================驗證token signature===========
            var tokenHandler = new JwtSecurityTokenHandler();

            var key = Encoding.ASCII.GetBytes(_config["TokenKey"]);
            var validationParameters = new TokenValidationParameters() // 生成驗證token的參數
            {
                RequireExpirationTime = true, // token是否包含有效期
                ValidateLifetime = true,
                ValidateIssuer = false, // 驗證秘鑰發行人，如果要驗證在這裏指定發行人字符串即可
                ValidateAudience = false, // 驗證秘鑰的接受人，如果要驗證在這裏提供接收人字符串即可
                IssuerSigningKey = new SymmetricSecurityKey(key) // 生成token時的安全秘鑰
            };
            SecurityToken securityToken  ; // 接受解碼後的token對象
            JwtSecurityToken jwtToken;
            try
            {
                jwtToken = tokenHandler.ReadToken(user.token) as JwtSecurityToken; // 將字符串token解碼成token對象
                var principal = tokenHandler.ValidateToken(user.token, validationParameters, out securityToken);
            }
            catch (Exception e)
            {
                result.IsSuccess = false;
                result.Message = "Token invalid.";
                return result;
            }

            var id = user.id;
            var guid = jwtToken.Claims.FirstOrDefault(c => c.Type == "GUID");
            if (id == 0 || guid == null) {
                result.IsSuccess = false;
                result.Message = "Token invalid.";
                return result;
            }
            //==============================================
            var existUser = _context.Users.Find(id);
            if (existUser == null)
            {
                result.IsSuccess = false;
                result.Message = "Username not exist.";
                return result;
            }
            if (existUser.guid != guid.Value)
            {
                result.IsSuccess = false;
                result.Message = "Invalid .";
            }
            else {
                if (String.IsNullOrEmpty(user.newPassword)) {
                    result.IsSuccess = false;
                    result.Message = "New password invalid .";
                    return result;
                }
                //變更新密碼
                CreatePasswordHash(user.newPassword, out byte[] passwordHash, out byte[] passwordSalt);
                existUser.PasswordHash = passwordHash;
                existUser.PasswordSalt = passwordSalt;
                //existUser.guid = "";
                await _context.SaveChangesAsync();
            }
            result.IsSuccess = true;
            result.Message = "重設密碼成功 .";
            return result;
        }


        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }
    }
}
