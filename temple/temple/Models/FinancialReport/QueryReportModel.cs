using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace temple.Models.FinancialReport
{
    public class QueryReportModel
    {
        public string Name { get; set; }
        public int Year { get; set; }
        public Report report { get; set; }
        public string Type { get; set; }
    }
}
