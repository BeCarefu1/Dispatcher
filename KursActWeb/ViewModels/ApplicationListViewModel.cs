using GIPManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ActGIPelectroWeb.ViewModels
{
    public class DescriptionSecurityViewModal
    {
        public int id { get; set; }
        public string descriptionSecurity { get; set; }
    }
    
    public class ApplicationListViewModel
    {
        public StoreContext db = new StoreContext();

        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int NetRegionId { get; set; }
        public string NetRegion { get; set; }
        public int CategoryId { get; set; }
        public string Category { get; set; }
        public int RepairId { get; set; }
        public string Repair { get; set; }
        public int AgId { get; set; }
        public string Ag { get; set; }
        public int StateId { get; set; }
        public string State { get; set; }
        public int ObjectId { get; set; }
        public string ObjectName { get; set; } //Equipment
        public int EquipmentId { get; set; }
        public string Equipment { get; set; }
        public string Description { get; set; }
        public DateTime RequestedDate { get; set; }
        public DateTime RequestedEndDate { get; set; }
        public DateTime RequestedStartTime { get; set; }
        public DateTime RequestedEndTime { get; set; }
        public DateTime ActualDate { get; set; }
        public DateTime ActualEndDate { get; set; }
        public DateTime ActualStartTime { get; set; }
        public DateTime ActualEndTime { get; set; }
        public DateTime OpenTime { get; set; }
        public DateTime CloseTime { get; set; }
        public List<DescriptionSecurityViewModal> SecurityList { get; set; }
        public string Notes { get; set; }
        public string Security { get; set; }


        public int StatusId { get; set; }
        public string Status { get; set; }

        public string Color { get; set; }

        public int UserId { get; set; }
        public string User { get; set; }
        public int NambId { get; set; }
        public string NameMaster { get; set; }
        public string NamePodpisant { get; set; }
        public string NameDispatcher { get; set; }
        public string RolePodpisant { get; set; }

        public List<SwitchListViewModel> Switches { get; set; }


    }
}
