using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace temple.Models.FinancialRecord
{
    public class CreateViewModel
    {
        public List<FinancialItem.FinancialItem> FinancialItems { get; set; }
        public FinancialRecord Record { get; set; }
    }
}
