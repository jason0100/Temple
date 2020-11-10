using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Models.FinancialReport
{
    public class ReportItem
    {
        public string Name { get; set; }
        public int[] Subtotal = new int[12];
             
    }
}
