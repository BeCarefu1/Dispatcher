using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GIPManager;
using ActGIPelectroWeb.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ActGIPelectroWeb.Pages
{
    [Authorize]
    public class ContractModel : PageModel
    {
        StoreContext db;
        public ContractModel(StoreContext context)
        {
            db = context;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<RegionCardViewModel> RegionsCardList { get; set; }
        //Статистика по ТУ
        public int CountSubstations { get; set; }
        public int CountRegPoints { get; set; }
        public int CountLinkOk { get; set; }
        public int CountAscueChecked { get; set; }
        public int CountAscueOk { get; set; }
        public int CountOther { get; set; }
        //Проценты
        public int PercentLinkOk { get; set; }
        public int PercentAscueChecked { get; set; }
        public int PercentAscueOk { get; set; }
        public int PercentOther { get; set; }
        //
        public int IdWorker;
        public int IdNetWork;

        public string ColorClass
        {
            get
            {
                int colorIdx = Id % 10;
                if (colorIdx == 0) return "bg-primary";
                if (colorIdx == 1) return "bg-info";
                if (colorIdx == 2) return "bg-success";
                if (colorIdx == 3) return "bg-secondary";
                if (colorIdx == 4) return "bg-danger";
                if (colorIdx == 5) return "bg-warning";
                if (colorIdx == 6) return "bg-light";
                if (colorIdx == 7) return "bg-dark";
                if (colorIdx == 8) return "bg-info";
                if (colorIdx == 9) return "bg-primary";
                return "bg-info";
            }
        }
        
        public IActionResult OnGet(int? id)
        {
            if (id == null) return NotFound();
            var contract = db.Contracts.Find(id);
            if (contract == null) return NotFound();

            Id = contract.Id;
            Name = contract.Name;
            Description = contract.Description;
            //Карточки района
            IdNetWork = (from a in db.NetRegions
                         join b in db.Workers on a.Id equals b.ID_NetRegion
                         join c in db.Users on b.Id equals c.WorkerID
                         where User.Identity.Name == c.Login
                         select a.Id).FirstOrDefault();
            /*IdWorker = (from a in db.Workers
                       join b in db.Users on a.Id equals b.WorkerID
                       where User.Identity.Name == b.Login
                       select a.Id).FirstOrDefault();*/
            RegionsCardList = new List<RegionCardViewModel>();
            foreach (var item in contract.NetRegions)
            {
                if (User.IsInRole("master") || User.IsInRole("worker"))
                {
                    if (item.Id == IdNetWork)
                        RegionsCardList.Add(new RegionCardViewModel(item.Id));
                }
                //if (!User.IsInRole("master"))
                else
                    RegionsCardList.Add(new RegionCardViewModel(item.Id));

            }
                

            //Всего объектов и точек учета
            CountSubstations = db.Substations.Count(s => s.NetRegion.ContractId == Id);
            CountRegPoints = db.RegPoints.Count(rp => rp.Substation.NetRegion.ContractId == Id && rp.Status == RegPointStatus.Default);
            //Статистика по ТУ
            CountLinkOk = db.RegPoints.Count(rp => 
                rp.Status == RegPointStatus.Default && 
                rp.Substation.NetRegion.ContractId == Id && 
                rp.RegPointFlags.IsLinkOk && 
                !rp.RegPointFlags.IsAscueChecked && 
                !rp.RegPointFlags.IsAscueOk
            );
            CountAscueChecked = db.RegPoints.Count(rp => 
                rp.Status == RegPointStatus.Default && 
                rp.Substation.NetRegion.ContractId == Id && 
                rp.RegPointFlags.IsAscueChecked && 
                !rp.RegPointFlags.IsAscueOk
            );
            CountAscueOk = db.RegPoints.Count(rp => 
                rp.Status == RegPointStatus.Default && 
                rp.Substation.NetRegion.ContractId == Id && 
                rp.RegPointFlags.IsAscueOk
            );
            CountOther = CountRegPoints - (CountLinkOk + CountAscueChecked + CountAscueOk);
            //Статистика Проценты
            if (CountRegPoints > 0)
            {
                PercentLinkOk = CountLinkOk * 100 / CountRegPoints;
                PercentAscueChecked = CountAscueChecked * 100 / CountRegPoints;
                PercentAscueOk = CountAscueOk * 100 / CountRegPoints;
                PercentOther = 100 - (PercentLinkOk + PercentAscueChecked + PercentAscueOk);
            }

            return Page();
        }
    }
}