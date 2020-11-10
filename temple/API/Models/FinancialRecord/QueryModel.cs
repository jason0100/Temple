using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace API.Models.FinancialRecord
{
    public class QueryModel: IValidatableObject
    {
        public int? ItemId { get; set; }
        public int? Year { get; set; }
        public int? Month { get; set; }
        [Required]
        public string Type { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            
            if (Type != "收入" && Type != "支出")
            {
                yield return new ValidationResult("Type:收入 or 支出");
            }
            if (Year != null) {
                if (Year < 2019 || Year > DateTime.Now.Year)
                {
                    yield return new ValidationResult("Year should be in range of 2019 to " + DateTime.Now.Year);
                }

            }

            if (Month != null) {
                if (Month < 1 || Month > 12)
                {
                    yield return new ValidationResult("月份請輸入1~12月");
                }
            }
            
        }
    }
}
