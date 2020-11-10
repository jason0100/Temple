using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace API.Models.MemberData
{
    public class QueryMemberModel
    {
        

        [Display(Name = "名字")]
        public string Name { get; set; }

        //[RegularExpression(@"^[a-zA-Z][0-9]{9}$", ErrorMessage = "ex:A123456789")]
        [Display(Name = "身分證字號")]
        public string Identity { get; set; }

        [Display(Name = "性別")]
        public string Gender { get; set; }

        //public QueryMemberModel(string? inputName, string? inputIdentity, string? inputGender) {
        //    Name = inputName;
        //    Identity = inputIdentity;
        //    Gender = inputGender;
        //}
    }
}
