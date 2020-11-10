using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace API.Models
{
   
    public class City {
        public int Id { get; set; }
        public string Name { get; set; }

        public virtual List<TownShip> TownShips {get;set;}
    }
    public class TownShip
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public string Zip { get; set; }

        public int CityId { get; set; }
        [JsonIgnore]
        public City City { get; set; }
    }
}
