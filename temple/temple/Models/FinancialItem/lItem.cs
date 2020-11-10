using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using temple.Models.FinancialRecord;

namespace temple.Models.FinancialItem
{
    public class FinancialItem
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "請輸入名稱")]
        [Display(Name = "名稱")]
        public string Name { get; set; }

        [Required(ErrorMessage = "請選擇類別")]
        [Display(Name = "類別")]
        public string Type { get; set; }

        public List<FinancialRecord.FinancialRecord> FinancialRecords { get; set; }
    }
}
