using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GIPManager;

namespace ActGIPelectroWeb.ViewModels
{
    public class RegionCardViewModel
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public int CountSubstations { get; private set; }   //Количество подстанций в регионе
        public int CountRegPoints { get; private set; }     //Количество точек учета в регионе
        public int CountForImportConsumer { get; private set; }     //Количество точек учета которые требуют данных о потребителе
        public string ColorClass { get {
                int colorIdx = Id % 10;
                if (colorIdx == 0) return "bg-primary";
                if (colorIdx == 1) return "bg-secondary";
                if (colorIdx == 2) return "bg-success";
                if (colorIdx == 3) return "bg-danger";
                if (colorIdx == 4) return "bg-info";
                if (colorIdx == 5) return "bg-warning";
                if (colorIdx == 6) return "bg-primary";
                if (colorIdx == 7) return "bg-dark";
                if (colorIdx == 8) return "bg-info";
                if (colorIdx == 9) return "bg-primary";
                return "bg-info";
            } }

        public RegionCardViewModel(int regionId)
        {
            using (StoreContext db = new StoreContext())
            {
                var region = db.NetRegions.Find(regionId);
                if (region != null)
                {
                    Id = region.Id;
                    Name = region.Name;
                    //Количество подстанций в регионе
                    CountSubstations = db.Substations.Count(s => s.NetRegionId == regionId);
                    //Количество точек учета в регионе
                    CountRegPoints = db.RegPoints.Count(rp => rp.Substation.NetRegionId == Id && rp.Status == RegPointStatus.Default);
                    CountForImportConsumer = db.RegPoints.Count(r => r.Substation.NetRegionId == Id && !r.RegPointFlags.ImportConsummerData);
                }
                else
                    throw new Exception("Нулевая ссылка на объект NetRegion при создании модели RegionViewModel");
            }
                
        }
    }
}
