using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using API.Models.MemberData;
namespace API.Models.RitualMoney
{
    public class RitualMoneyRecord
    {
        public int RitualMoneyRecordId { get; set; }


        //[RegularExpression(@"\d{4}/\d{2}/\d{2}", ErrorMessage = "ex:1999/01/01")]
        [Display(Name = "借用日期")]
        public DateTime BorrowDate { get; set; }

        [RegularExpression(@"\+?[0-9][0-9]*", ErrorMessage = "請輸入非零正整數金額")]
        [Display(Name = "借用金額")]
        public decimal? BorrowAmount { get; set; }

        //[RegularExpression(@"\d{4}/\d{2}/\d{2}", ErrorMessage = "ex:1999/01/01")]
        [Display(Name = "還願日期")]
        public DateTime? ReturnDate { get; set; }

        //[RegularExpression(@"\+?[0-9][0-9]*", ErrorMessage = "請輸入非零正整數金額")]
        [Display(Name = "還願金額")]
        public decimal? ReturnAmount { get; set; }

        public bool IsReturn { get; set; } = false;

        /// <summary>
        /// Foreign Key
        /// </summary>
        public int MemberId { get; set; }
        public member Member { get; set; }
    }
}
