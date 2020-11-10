using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace API.Models.FinancialRecord
{
    public class FinancialRecordToPut
    {
        [Required]
        public int Id { get; set; }

        //[Required]
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "請輸入電話")]
        [RegularExpression(@"^09\d{8}$", ErrorMessage = "ex:09xxxxxxxx")]
        public string CustomerPhone { get; set; }
        public string LandPhone { get; set; }

        //[Required]
        [Range(1, int.MaxValue, ErrorMessage = "請輸入正整數")]
        public int? Amount { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "請輸入正整數")]
        //[Required]
        public int? Quantity { get; set; }

        
        public DateTime? CreateDate { get; set; }
        public string PayType { get; set; }
      
        //[RegularExpression(@"\d{4}-\d{2}-\d{2}", ErrorMessage = "ex:2020-01-01")]
        public DateTime? ReturnDate { get; set; }
        //[RegularExpression(@"\d{4}-\d{2}-\d{2}", ErrorMessage = "ex:2020-01-01")]
        public DateTime? DueDate { get; set; }
        public string Place { get; set; }
        public int? Position { get; set; }
        public string Notes { get; set; }
        //[Required]
        public int? FinancialItemId { get; set; }
        
        public FinancialItem.FinancialItem FinancialItem { get; set; }
    }
}
