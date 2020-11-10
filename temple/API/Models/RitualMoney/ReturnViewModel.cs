using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace API.Models.RitualMoney
{
    public class ReturnViewModel
    {

        [Display(Name = "姓名")]
        public string Name { get; set; }

        //[RegularExpression(@"^[a-zA-Z][0-9]{9}$", ErrorMessage = "ex:A123456789")]
        [Display(Name = "身分證字號")]
        public string Identity { get; set; }

        //[RegularExpression(@"^[1-9][0-9]*$", ErrorMessage = "請輸入非零正整數金額")]
        [Display(Name = "還願金額")]
        public decimal ReturnAmount { get; set; }
    }
}
