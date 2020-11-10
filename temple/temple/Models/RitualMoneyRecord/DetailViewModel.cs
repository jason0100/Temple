using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
 
namespace temple.Models.RitualMoneyRecord
{
    public class DetailViewModel
    {
        [Display(Name ="姓名")]
        public string Name { get; set; }

        [Display(Name = "身分證字號")]
        public string Identity { get; set; }

        public RitualMoneyRecord Record { get; set; }
        
    }
}
