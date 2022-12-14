using GIPManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ActGIPelectroWeb.Models
{
    public class RegionAndMaster
    {
        private readonly StoreContext db;
        public RegionAndMaster(StoreContext context)
        {
            db = context;
            RegList = (from a in db.NetRegions select new RegAndMasterList { idReg = a.Id, nameReg = a.Name }).ToList();
            MasterList = (from a in db.Users where a.RoleId == 15 select new RegAndMasterList { idMaster = a.Id, nameMaster = a.Name }).ToList();
        }
        public class RegAndMasterList
        {
            public int idReg { get; set; }
            public string nameReg { get; set; }
            public int idMaster { get; set; }
            public string nameMaster { get; set; }
        }

        public List<RegAndMasterList> RegList { get; set; }
        public List<RegAndMasterList> MasterList { get; set; }
        public void Select()
        {
            RegList = (from a in db.NetRegions select new RegAndMasterList { idReg = a.Id, nameReg = a.Name }).ToList();
            MasterList = (from a in db.Users where a.RoleId == 15 select new RegAndMasterList { idMaster = a.Id, nameMaster = a.Name }).ToList();
        }

    }
}
