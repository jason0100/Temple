using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Models.Notification
{
    public class NotifyModel
    {
        public int Id { get; set; }
        public string ItemName { get; set; }
        public string MemberName{get;set;}
        public bool IsRead { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
