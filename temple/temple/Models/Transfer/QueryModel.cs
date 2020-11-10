using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace temple.Models.Transfer
{
    public class QueryViewModel
    {
        public string KeyWord { get; set; }
        public int? Year { get; set; }
        public int? Month { get; set; }

        public List<TransferRecord> TransferRecords { get; set; }
       
    }
}
