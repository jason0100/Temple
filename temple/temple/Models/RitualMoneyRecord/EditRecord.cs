using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace temple.Models.RitualMoneyRecord
{
    public class EditRecord
    {
        [RegularExpression(@"\+?[1-9][0-9]*", ErrorMessage = "請輸入非零正整數金額")]
        [Display(Name = "借取金額")]
        [Required(ErrorMessage = "請輸入借取金額")]
        public int? BorrowAmount { get; set; }

        [RegularExpression(@"\+?[1-9][0-9]*", ErrorMessage = "請輸入非零正整數金額")]
        [Display(Name = "還願金額")]
        public int? ReturnAmount { get; set; }
    }
}
