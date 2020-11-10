using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace temple.Models.FinancialRecord
{
    public class FinancialRecord : IValidatableObject
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "請輸入名稱")]
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "請輸入電話")]
        [RegularExpression(@"^09\d{8}$", ErrorMessage = "ex:09xxxxxxxx")]
        public string CustomerPhone { get; set; }
        public string LandPhone { get; set; }

        [Required(ErrorMessage = "請輸入金額")]
        [RegularExpression(@"^[1-9][0-9]*$", ErrorMessage = "請輸入非零正整數金額")]
        //[DisplayFormat(DataFormatString = "{0:N0}", ApplyFormatInEditMode = true)]
        public int? Amount { get; set; }

        [RegularExpression(@"^[1-9][0-9]*$", ErrorMessage = "請輸入非零正整數數量")]
        [Required(ErrorMessage = "請輸入數量")]
        public int? Quantity { get; set; }


        //[RegularExpression(@"\d{4}-\d{2}-\d{2}", ErrorMessage = "ex:2020-01-01")]
        public string DueDate { get; set; }

        /// <summary>
        /// 龍邊or虎邊
        /// </summary>
        public string Place { get; set; }
        public int? Position { get; set; }

        public string Notes { get; set; }

        public string PayType { get; set; }

        //[DataType(DataType.Date)]
        //[DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
        public string ReturnDate { get; set; }
        public string CreateDate { get; set; }

        public IEnumerable<SelectListItem> FinancialItems { get; set; }

        [Required(ErrorMessage = "請選擇財務項目")]
        public int? FinancialItemId { get; set; }

        public FinancialItem.FinancialItem FinancialItem { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (FinancialItemId == 57 || FinancialItemId == 75)
            {
                if (string.IsNullOrEmpty(Place))
                    yield return new ValidationResult("必填", new string[] { "Place" });
                if (Position == null)
                    yield return new ValidationResult("必填", new string[] { "Position" });
                if (string.IsNullOrEmpty(DueDate))
                    yield return new ValidationResult("必填", new string[] { "DueDate" });
                else {
                    DateTime result;
                    if (!DateTime.TryParse(DueDate, out result))
                    {
                        yield return new ValidationResult("日期錯誤", new string[] { "DueDate" });
                    }
                }

            }

        }
    }
}
