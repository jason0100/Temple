using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace temple.Models.FriendsController
{
    public class PostViewModel
    {

        public Friend friend { get; set; }

        public IEnumerable<SelectListItem> CityList { get; set; }
        public IEnumerable<SelectListItem> TownShipList { get; set; }
    }
}
