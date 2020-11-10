using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Models.FinancialReport
{
    public class Report
    {
        public List<ReportItem> ReportItems = new List<ReportItem>();
        public int[] MonthSubtotal = new int[12];
    }
}
