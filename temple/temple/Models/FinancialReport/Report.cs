using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace temple.Models.FinancialReport
{
    public class Report
    {
        public List<ReportItem> ReportItems = new List<ReportItem>();
        public decimal[] MonthSubtotal = new decimal[12];
    }
}
