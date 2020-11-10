using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using API.Models;
using API.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;

namespace API.Controllers
{
    //[Authorize]
    [Route("[controller]")]
    [ApiController]
    public class CityController : ControllerBase
    {
        private TempleDbContext _context;
        public CityController(TempleDbContext context) {
            _context = context;
        }


        [HttpGet]
        public async Task<ResultModel> Get() {
            ResultModel result = new ResultModel();
            List<City> CityList = new List<City>();
            var queryCity = from o in _context.Cities 
                            .Include(c=>c.TownShips)
                            select o;
            
            if (queryCity.Count() != 0)
            {
                result.IsSuccess = true;
                result.Message = "Get cities list success";
                result.Data = queryCity;
            }
            else {
                result.IsSuccess = false;
                result.Message = "No data.";
            }
            
            return result;
        }

      
    }
}