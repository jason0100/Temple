using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.Models;
using API.Models.Notification;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class NotifyController : ControllerBase
    {

        private TempleDbContext _context;
        public NotifyController(TempleDbContext context)
        {
            _context = context;
        }


        [HttpGet("count")]
        public async Task<ResultModel> Get()
        {
            ResultModel result = new ResultModel();

            var query = _context.Notification.Where(a=>a.IsRead==false).Count();


            result.IsSuccess = true;
            result.Data = query;


            return result;
        }


        //取得資料
        [HttpGet("GetData")]
        public async Task<ResultModel> GetData()
        {
            ResultModel result = new ResultModel();

            var query = _context.Notification.Where(a=>a.IsRead==false).ToList();

            if (query.Count() != 0)
            {
                result.IsSuccess = true;
                result.Message = "Get data success";
                result.Data = query;
            }
            else
            {
                result.IsSuccess = false;
                result.Message = "No data.";
            }

            return result;
        }


        //設為已讀
        [HttpGet("SetRead")]
        public async Task<ResultModel> SetRead()
        {
            ResultModel result = new ResultModel();

            var query = _context.Notification;

            foreach (var i in query)
            {
                i.IsRead = true;
            }
            _context.SaveChanges();
            result.IsSuccess = true;
            result.Message = "通知已讀";
            return result;
        }


    }
}