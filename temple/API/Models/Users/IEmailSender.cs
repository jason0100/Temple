using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Models.Users
{
    public interface IEmailSender
    {
        Task<ResultModel> SendEmailAsync(string email, string subject, string message);
        
    }
}
