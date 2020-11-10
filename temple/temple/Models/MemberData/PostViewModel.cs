using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace temple.Models.MemberData
{
    public class PostViewModel
    {
        public member member { get; set; }

        public IEnumerable<SelectListItem> CityList { get; set; }
        public IEnumerable<SelectListItem> TownShipList { get; set; }
    }
}
