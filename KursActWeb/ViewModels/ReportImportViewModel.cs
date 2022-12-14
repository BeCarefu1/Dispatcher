using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ActGIPelectroWeb.ViewModels
{
    public class ReportImportViewModel
    {

    }

    public class Report
    {
        public int Id { get; set; }
        public string NetRegion { get; set; }
        public string Substation { get; set; }
        public int Mounted { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; }
        public bool ClosedSMR { get; set; }

    }

    public class DropDownItem
    {
        public string Name { get; set; }
        public int Id { get; set; }
    }
}

