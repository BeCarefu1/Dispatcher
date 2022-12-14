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
    public class RegionModel : PageModel
    {
        private readonly StoreContext db;
        public RegionModel(StoreContext context)
        {
            db = context;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public string ContractName { get; set; }
        public int ContractId { get; set; }
        public List<SubstationRowViewModel> SubstationList { get; set; }
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

        public IActionResult OnGet(int? id)
        {
            if (id == null) return NotFound();
            var netRegion = db.NetRegions.Find(id);
            if (netRegion == null) return NotFound();
            
            Id = netRegion.Id;
            Name = netRegion.Name;
            Description = netRegion.FullName +
                ", Контактное лицо: " + netRegion.ContactName +
                ", тел.: " + netRegion.PhoneNumber +
                ", " + netRegion.Email;
            ContractName = netRegion.Contract.Name;
            ContractId = netRegion.ContractId;


            //Точки
            //SubstationList = (from s in db.Substations
            //                  join state in db.SubstationStates on s.SubstationStateId equals state.Id
            //                  join linkS in db.SubstationLinks on s.Id equals linkS.SubstationId into lSs
            //                  from linkS in lSs.DefaultIfEmpty() //Link привязанные к подстанции
            //                  join rp in db.RegPoints on s.Id equals rp.SubstationId into rps
            //                  from rp in rps.DefaultIfEmpty() //Доделать!!!
            //                  join rpFlags in db.RegPointFlags on rp.Id equals rpFlags.RegPointId into flags
            //                  from rpFlags in flags.DefaultIfEmpty()
            //                  join a in db.SubstationActions on s.Id equals a.SubstationId into actions
            //                  from a in actions.DefaultIfEmpty()
            //                  where s.NetRegionId == (int)Id
            //                  orderby a.Date descending
            //                  select new
            //                  {
            //                      Id = s.Id,
            //                      Name = s.Name,
            //                      StateName = state.Name,
            //                      rp = rp ?? new RegPoint(),
            //                      rpFlags = rpFlags ?? new RegPointFlags(),
            //                      PhoneNumber = linkS.PhoneNumber,
            //                      LastChanges = a.Date,
            //                      IsInstallationDone = s.IsInstallationDone,
            //                      IsPropSchemeDone = s.IsPropSchemeDone,
            //                      IsBalanceDone = s.IsBalanceDone
            //                  })
            //                  .GroupBy(row => row.rp.Id).Select(row => row.First()) //фильтруем уникальные точки учета
            //                  .GroupBy(row => row.Id)
            //                  .Select(gr => 
            //                  new SubstationRowViewModel()
            //                  {
            //                      Id = gr.First().Id,
            //                      Name = gr.First().Name,
            //                      StateName = gr.First().StateName,
            //                      CountRegPoints = gr.Count(p=> p.rp.Id != 0 && p.rp.Status == RegPointStatus.Default),
            //                      CountUSPD = gr.Count(p=> p.rp.Id != 0 && p.rp.Status == RegPointStatus.USPD),
            //                      CountForImportConsumer = gr.Count(p => p.rp.Id != 0 && !p.rpFlags.ImportConsummerData),
            //                      PhoneNumber = gr.First().PhoneNumber,
            //                      LastChanges = gr.First().LastChanges,
            //                      IsInstallationDone = gr.First().IsInstallationDone,
            //                      IsPropSchemeDone = gr.First().IsPropSchemeDone,
            //                      IsBalanceDone = gr.First().IsBalanceDone
            //                  })
            //                  .ToList();
            //Всего объектов и точек учета
            CountSubstations = db.Substations.Count(s => s.NetRegionId == Id);
            CountRegPoints = db.RegPoints.Count(rp => rp.Status == RegPointStatus.Default && rp.Substation.NetRegionId == Id);
            //Статистика по ТУ
            CountLinkOk = db.RegPoints.Count(rp => 
                rp.Status == RegPointStatus.Default && 
                rp.Substation.NetRegionId == Id && 
                rp.RegPointFlags.IsLinkOk && 
                !rp.RegPointFlags.IsAscueChecked && 
                !rp.RegPointFlags.IsAscueOk
            );
            CountAscueChecked = db.RegPoints.Count(rp =>
                rp.Status == RegPointStatus.Default &&
                rp.Substation.NetRegionId == Id &&
                rp.RegPointFlags.IsAscueChecked &&
                !rp.RegPointFlags.IsAscueOk
            );
            CountAscueOk = db.RegPoints.Count(rp => 
                rp.Status == RegPointStatus.Default && 
                rp.Substation.NetRegionId == Id && 
                rp.RegPointFlags.IsAscueOk
            );
            CountOther = CountRegPoints - (CountLinkOk + CountAscueChecked + CountAscueOk);
            //Статистика по ТУ
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