using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace API.Models.Transfer
{
    public class QueryModel:IValidatableObject
    {
        public string KeyWord { get; set; }
        public int? Year { get; set; }
        public int? Month { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Year != null) { 
                if (Year < 2019 || Year > DateTime.Now.Year)
                    yield return new ValidationResult($"Year should be in range of 2019 to " + DateTime.Now.Year);
            }
            if (Month != null) {
                if(Month<1||Month>12)
                    yield return new ValidationResult($"Month should be in range of 1 to 12 ");
            }

        }
    }
}
