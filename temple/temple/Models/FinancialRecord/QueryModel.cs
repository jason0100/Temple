using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace temple.Models.FinancialRecord
{
    public class QueryModel
    {
        public int? ItemId { get; set; }
        public int? Year { get; set; }
        public int? Month { get; set; }
        public string Type { get; set; }
        public List<FinancialRecord> FinancialRecords { get; set; }
    }
}
