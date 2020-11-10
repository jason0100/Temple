using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using API.Models.FinancialRecord;

namespace API.Models.FinancialItem
{
    public class FinancialItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        [JsonIgnore]
        public List<FinancialRecord.FinancialRecord> FinancialRecords { get; set; }
    }
}
