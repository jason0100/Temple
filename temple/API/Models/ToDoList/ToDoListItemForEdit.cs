using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace API.Models.ToDoList
{
    public class ToDoListItemForEdit
    {
        [Required]
        public int Id { get; set; }
        
        public string Subject { get; set; }
        
        public bool? IsDone { get; set; }

    }
}
