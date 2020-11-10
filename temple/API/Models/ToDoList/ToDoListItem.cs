using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace API.Models.ToDoList
{
    public class ToDoListItem
    {
        public int? Id { get; set; }
        [Required]
        public string Subject { get; set; }
       
        public bool IsDone { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? EditDate { get; set; }
    }
}
