using API.Dtos;
using API.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Models;

namespace API.Data
{
    public interface IAuthRepository
    {
        Task<User> Register(User user, string password);
        Task<LoginUser> Login(string username, string password);
        Task<bool> UserExists(string username);
        Task<ResultModel> ChangePassword(UserForChangePassword userForChangePassword);
        Task<ResultModel> RefreshToken(Token token);
        Task<ResultModel> RecoverPassword(UserForRecoverPW user);
        Task<ResultModel> ResetPassword(UserForRecoverPW user);
    }
}
