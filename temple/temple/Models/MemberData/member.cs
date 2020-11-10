using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace temple.Models.MemberData
{

    public class member
    {
        [Display(Name = "信徒編號")]
        public int MemberId { get; set; }

        [Display(Name = "姓名")]
        [Required(ErrorMessage = "請輸入姓名")]
        public string Name { get; set; }

        [RegularExpression(@"^[a-zA-Z][0-9]{9}$", ErrorMessage = "ex:A123456789")]
        [Required(ErrorMessage = "請輸入身分證字號")]
        [Display(Name = "身分證字號")]
        public string Identity { get; set; }

        [Required(ErrorMessage = "請選擇性別")]
        [Display(Name = "性別")]
        public string Gender { get; set; }
        
        [RegularExpression(@"\d{4}-\d{2}-\d{2}", ErrorMessage ="ex:1999-01-01")]
        [Display(Name = "生辰(國曆)")]
        public string Birth { get; set; }

        [RegularExpression(@"\d{4}-\d{2}-\d{2}", ErrorMessage = "ex:1999-01-01")]
        [Display(Name = "生辰(農曆)")]
        public string LunarBirth { get; set; }

        [Display(Name = "註冊日期")]
        [DataType(DataType.Date)]
        public string CreateDate { get; set; }

        [Display(Name = "時辰")]
        public string TimeOfLunarBirth { get; set; }

        [Display(Name = "生肖")]
        public string ZodiacAnimal { get; set; }

        [RegularExpression(@"0\d{1,2}\d{6,8}", ErrorMessage = "ex:02123456")]
        [Display(Name = "市話")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "請輸入電話")]
        [RegularExpression(@"^09\d{8}$", ErrorMessage = "ex:09xxxxxxxx")]
        [Display(Name = "手機")]
        public string CellPhone { get; set; }

        [Display(Name = "縣市")]
        [Required(ErrorMessage = "請選擇縣市")]
        public int? CityId { get; set; }

        [Required(ErrorMessage = "請選擇鄉鎮")]
        [Display(Name = "鄉鎮")]
        public int? TownshipId { get; set; }

        [RegularExpression(@"^\d{3}$", ErrorMessage = "ex:722")]
        [Display(Name = "郵遞區號")]
        public string Zip { get; set; }
                
        [Display(Name = "地址")]
        public string Address { get; set; }

        public City city { get; set; }
        public TownShip townShip { get; set; }
    }
}
