using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ActGIPelectroWeb.ViewModels
{
    public class SwitchListViewModel
    {
        public class ContentOperationList
        {
            public int Id { get; set; }
            public string Content { get; set; }
            public int IdSwitchForm { get; set; }
        }

        public int Id { get; set; }
        public int IdNetRegion { get; set; }
        public string NetRegion { get; set; }
        public int IdObject { get; set; }
        public string ObjectName { get; set; }
        public string Job { get; set; }
        public int IdApp { get; set; }
        public int NumbApp { get; set; }
        public int IdDisp { get; set; }
        public string NameMaster { get; set; }
        public string RoleMaster { get; set; }
        public string NameDisp { get; set; }
        public string RoleDisp { get; set; }
        public int IdStatusApp { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string SourceSchema { get; set; }
        public string SwitchingProducesUser { get; set; }
        public List<ContentOperationList> Contents { get; set; }
    }
}
