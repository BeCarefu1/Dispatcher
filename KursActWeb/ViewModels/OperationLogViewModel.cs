using GIPManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ActGIPelectroWeb.ViewModels
{
    public class OperationLogViewModel
    {
        public int Id { get; set; }
        public DateTime RecoilTime { get; set; }
        public string Description { get; set; }
        public DateTime TineOfApplication { get; set; }
        public string Remarks { get; set; }
        public string RemarksForTextArea { get; set; }
        public int Numb { get; set; }
        public string Name { get; set; }
        public string VisaUserName { get; set; }
    }
}
