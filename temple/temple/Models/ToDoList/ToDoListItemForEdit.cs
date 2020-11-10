using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace temple.Models.ToDoList
{
    public class ToDoListItemForEdit
    {
        [Required]
        public int Id { get; set; }
        
        public string Subject { get; set; }
        
        public bool? IsDone { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime EditDate { get; set; }
    }
}
