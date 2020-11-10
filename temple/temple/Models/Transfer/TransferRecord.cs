using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace temple.Models.Transfer
{
    public class TransferRecord
    {
        public int Id { get; set; }
        public DateTime CreateDate { get; set; }

        [Required(ErrorMessage ="請輸入項目名稱")]
        public string eventName { get; set; }

        [Required(ErrorMessage = "請輸入匯款金額")]
        [RegularExpression(@"^[1-9][0-9]*$", ErrorMessage = "請輸入非零正整數金額")]
        //[DisplayFormat(DataFormatString = "{0:N0}", ApplyFormatInEditMode = true)]
        public int? Amount { get; set; }

        [Required(ErrorMessage = "請選擇匯款方式")]
        public string TransferType { get; set; }

        [Required(ErrorMessage = "請輸入對方匯款銀行名稱")]
        public string BankName { get; set; }

        [Required(ErrorMessage = "請輸入對方匯款帳號")]
        public string BankAccount { get; set; }

        public string Notes { get; set; }
    }
}
