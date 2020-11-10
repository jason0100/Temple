using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace API.Models.Users
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _config;
        public EmailSender(IConfiguration config)
        {
            _config = config;
        }
        public async Task<ResultModel> SendEmailAsync(string email, string subject, string content)
        {
            
            var result = new ResultModel();
            
            using (var message = new MailMessage())
            {
                //收件人 
                message.To.Add(new MailAddress(email));
                //From地址很重要。是郵件顯示來自的郵件地址，也是郵件客戶端中點擊回复按鈕時回复的地址。
                message.From = new MailAddress("jasonwaidog2@gmail.com", "福德祠系統管理員");
                //抄送 
                //message.CC.Add(new MailAddress("cc@email.com", "CC Name"));
                //密件抄送
                //message.Bcc.Add(new MailAddress("bcc@email.com", "BCC Name"));
                message.Subject = subject ;
                message.Body = "<a href='"+content+"'>Click to reset password.</a>" ;
                message.IsBodyHtml = true;
                //使用using，因為MailMessage實現了IDisposable接口。
                //Gmial 的 smtp 使用 SSL
               
                using (var client = new SmtpClient("smtp.gmail.com"))
                {
                    client.Port = 587;
                    //gmail帳戶和密碼 
                    client.Credentials = new NetworkCredential(_config["mailSender"], _config["MailSenderPassword"]);
                    client.EnableSsl = true;
                    client.Send(message);
                }
            }

            result.IsSuccess = true;
            result.Message = "Email sended.";
            return result;
        }
    }
}
