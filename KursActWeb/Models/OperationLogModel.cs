using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ActGIPelectroWeb.Models
{
    public class OperationLogModel
    {
        public int id { get; set; }
        public DateTime recoilTime { get; set; }
        public string description { get; set; }
        public DateTime tineOfApplication { get; set; }
        public string remarks { get; set; }
    }
}
