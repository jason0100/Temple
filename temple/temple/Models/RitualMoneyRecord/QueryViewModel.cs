using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace temple.Models.RitualMoneyRecord
{
    public class QueryViewModel
    {
        public string Name { get; set; }

        [RegularExpression(@"^[a-zA-Z][0-9]{9}$", ErrorMessage = "ex:A123456789")]
        public string Identity { get; set; }
        public int? Year { get; set; }
        public int? Month { get; set; }
        
        public List<RitualMoneyRecord> RitualMoneyRecords { get; set; }
    }
}
