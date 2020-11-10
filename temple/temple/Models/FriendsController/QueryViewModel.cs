using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace temple.Models.FriendsController
{
    public class QueryViewModel
    {
        public string Name { get; set; }
        public ICollection<Friend> Friends { get; set; }
    }
}
