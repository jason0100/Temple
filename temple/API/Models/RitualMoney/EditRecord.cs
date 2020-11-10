using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace API.Models.RitualMoney
{
    public class EditRecord
    {
        [Required]
        [RegularExpression(@"^[1-9][0-9]*$", ErrorMessage="請輸入非零正整數金額")]
        public int? BorrowAmount { get; set; }

    
        [RegularExpression(@"^[1-9][0-9]*$", ErrorMessage = "請輸入非零正整數金額")]
        public int? ReturnAmount { get; set; }
    }
}
