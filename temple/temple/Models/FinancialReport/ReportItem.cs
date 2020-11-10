using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace temple.Models.FinancialReport
{
    public class ReportItem
    {
        public string Name { get; set; }
        public decimal[] Subtotal = new decimal[12];
             
    }
}
