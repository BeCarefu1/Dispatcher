using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GIPManager;
using StoreGIPManager;

namespace ActGIPelectroWeb.Pages
{
  
    public class SubstationMaterialsModel : PageModel
    {
        readonly StoreContext _db;
        public SubstationMaterialsModel(StoreContext db)
        {
            _db = db;
        }
        public Substation Substation { get; set; }
        public IEnumerable<IGrouping<string, Device>> DeviceTypes { get; set; }
        public List<IGrouping<string, Device>> SubstationDevices { get; set; }
        public List<IGrouping<string, KDE>> KdeTypesMounters { get; set; }
        public KdeTypesStore KdeTypesStore { get; set; }
        public IEnumerable<SubstationMaterial> Materials { get; set; }
        public List<SubstationMaterial> Sets { get; set; }
        public List<SubstationMaterial> SetsMaterials { get; set; }
        public List<SubstationMaterial> SBMaterials { get; set; }
        public List<SubstationMaterial> GetTT { get; set; }


        public Contract Contract { get; set; }
        public NetRegion NetRegion { get; set; }

        public void OnGet(int id)
        {
            Substation = _db.Substations.Find(id);
            Contract = _db.Contracts.Find(Substation.NetRegion.ContractId);
            NetRegion = _db.NetRegions.Find(Substation.NetRegionId);
            
            DeviceTypes = MaterialsByObject.GetDeiceTypes(id).OrderByDescending(x=> x.Key);
            SubstationDevices = MaterialsByObject.GetSubstationDevices(id);
            KdeTypesMounters = MaterialsByObject.GetMountersKdeTypes(id);
            KdeTypesStore = MaterialsByObject.GetKdeTypesStore(id);
            Materials = MaterialsByObject.GetMaterials(id);
            Sets = MaterialsByObject.GetSubstationSets(id);
            SBMaterials = MaterialsByObject.GetSBMaterials(id);
            GetTT = MaterialsByObject.GetSBTT(id);

        }

       


    }
}