using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace temple.Models.MemberData
{
    public class QueryMemberViewModel
    {
        public ResultModel ResultModel { get; set; }
        public List<member> members { get; set; }
      
        [Display(Name = "名字")]
        public string Name { get; set; }

        [RegularExpression(@"^[a-zA-Z][0-9]{9}$", ErrorMessage = "ex:A123456789")]
        [Display(Name = "身分證字號")]
        public string Identity { get; set; }

        [Display(Name = "性別")]
        public string Gender { get; set; }

    }
}
