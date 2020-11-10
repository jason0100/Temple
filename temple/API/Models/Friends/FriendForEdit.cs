using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace API.Models.Friends
{
    public class FriendForEdit
    {
        [Display(Name = "宮廟編號")]
        public int Id { get; set; }
        
       
        [Display(Name = "宮廟名稱")]
        public string Name { get; set; }

        [Display(Name = "主祀神明")]
        public string MainGod { get; set; }

        [Display(Name = "聖誕日期")]
        [RegularExpression(@"\d{2}-\d{2}", ErrorMessage = "請輸入農曆日期ex:01-01")]
        public string BirthDate { get; set; }

        [Display(Name = "活動日期")]
        [RegularExpression(@"\d{4}-\d{2}-\d{2}", ErrorMessage = "ex:1999-01-01")]
        public string ActivityDate { get; set; }

        [Display(Name = "備註")]
        public string Notes { get; set; }

        [Display(Name = "聯絡人")]
        public string ContactName { get; set; }

        [RegularExpression(@"0\d{1,2}-\d{6,8}", ErrorMessage = "ex:02-123456")]
        [Display(Name = "市話")]
        public string Phone { get; set; }

        [RegularExpression(@"^09\d{2}-\d{6}$", ErrorMessage = "ex:0912-123456")]
        [Display(Name = "手機")]
        public string CellPhone { get; set; }

       
        public int? CityId { get; set; }

      
        public int? TownshipId { get; set; }

        public string Address { get; set; }

        public City city { get; set; }
        public TownShip townShip { get; set; }

    }
}
