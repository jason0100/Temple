using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace API.Models.Upload
{
    public class uploadFile
    {
        [Required]
        public List<IFormFile> files { get; set; }
        [Required]
        public string folder { get; set; }
    }
  
}
