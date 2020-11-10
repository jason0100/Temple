using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using temple.Models.MemberData;

namespace temple.Models.RitualMoneyRecord
{
    public class RitualMoneyRecord
    {
        [Display(Name = "借用單編號")]
        public int RitualMoneyRecordId { get; set; }

        //[Display(Name = "姓名")]
        //public string Name { get; set; }

        //[Display(Name = "身分證字號")]
        //public string Identity { get; set; }

        //[RegularExpression(@"\d{4}/\d{2}/\d{2}", ErrorMessage = "ex:1999/01/01")]
        [Display(Name = "借用日期")]
        public string BorrowDate { get; set; }

        [RegularExpression(@"\+?[1-9][0-9]*", ErrorMessage = "請輸入非零正整數金額")]
        [DisplayFormat(DataFormatString = "{0:N0}", ApplyFormatInEditMode = true)]
        [Display(Name = "借取金額")]
        [Required(ErrorMessage = "請輸入借取金額")]
        public decimal? BorrowAmount { get; set; }

        //[RegularExpression(@"\d{4}/\d{2}/\d{2}", ErrorMessage = "ex:1999/01/01")]
        [Display(Name = "還願日期")]
        public string ReturnDate { get; set; }

        [RegularExpression(@"\+?[1-9][0-9]*", ErrorMessage = "請輸入非零正整數金額")]
        [Display(Name = "還願金額")]
        //[Required(ErrorMessage = "請輸入還願金額")]
        [DisplayFormat(DataFormatString = "{0:N0}", ApplyFormatInEditMode = true)]
        public decimal? ReturnAmount { get; set; }

        public bool IsReturn { get; set; } = false;

        /// <summary>
        /// Foreign Key
        /// </summary>
        public int MemberId { get; set; }
        public member Member { get; set; }
    }
}
