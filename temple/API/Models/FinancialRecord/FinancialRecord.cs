using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace API.Models.FinancialRecord
{
    public class FinancialRecord : IValidatableObject
    {
        public int Id { get; set; }

        [Required]
        public string CustomerName { get; set; }

        [RegularExpression(@"^09\d{8}$", ErrorMessage = "ex:09xxxxxxxx")]
        public string CustomerPhone { get; set; }
        public string LandPhone { get; set; }
        [Required]
        [RegularExpression(@"^[1-9][0-9]*\.*$", ErrorMessage = "請輸入非零正整數金額")]
        public int? Amount { get; set; }

        [RegularExpression(@"^[1-9][0-9]*$", ErrorMessage = "請輸入非零正整數數量")]
        [Required]
        public int? Quantity { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime CreateDate { get; set; }

        public string PayType { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? ReturnDate { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? DueDate { get; set; }

        /// <summary>
        /// 龍邊or虎邊
        /// </summary>
        public string Place { get; set; }
        public int? Position { get; set; }
        public string Notes { get; set; }
        [Required]
        public int? FinancialItemId { get; set; }

        public FinancialItem.FinancialItem FinancialItem { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (FinancialItemId == 57 || FinancialItemId == 75)
            {
                if (String.IsNullOrEmpty(Place))
                {
                    yield return new ValidationResult("必填", new string[] { "Place" });
                }
                else
                {
                    if (Place.Trim() != "龍" || Place.Trim() != "虎")
                    {
                        if (string.IsNullOrEmpty(Place))
                            yield return new ValidationResult("必填", new string[] { "Place" });
                        if (Position == null)
                            yield return new ValidationResult("必填", new string[] { "Position" });
                        if (DueDate == null)
                            yield return new ValidationResult("必填", new string[] { "DueDate" });


                    }
                }
            }
        }
    }
}
