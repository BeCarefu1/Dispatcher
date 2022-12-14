using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading.Tasks;
using GIPManager;
using Hangfire.Storage.Monitoring;
using ActGIPelectroWeb.Models;
using ActGIPelectroWeb.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ActGIPelectroWeb.Pages
{
    [Authorize]
    public class SubstationModel : PageModel
    {
        private readonly StoreContext db;
        public SubstationModel(StoreContext context)
        {
            db = context;
            Comments = new List<CommentViewModel>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string State { get; set; }
        public List<RegPointRowViewModel> RegPointsList { get; set; }
        public string RegionName { get; set; }
        public int RegionId { get; set; }
        public string ContractName { get; set; }
        public int ContractId { get; set; }
        //Статистика по ТУ
        public int Count { get; set; }
        public int CountLinkOk { get; set; }
        public int CountAscueChecked { get; set; }
        public int CountAscueOk { get; set; }
        public int CountOther { get; set; }
        //Проценты
        public int PercentLinkOk { get; set; }
        public int PercentAscueChecked { get; set; }
        public int PercentAscueOk { get; set; }
        public int PercentOther { get; set; }

        //Флаги подстанции
        public bool IsInstallationDone { get; set; }
        public bool IsPropSchemeDone { get; set; }
        public bool IsBalanceDone { get; set; }
        public bool IsKS2Done { get; set; }

        //Выпадающий список с ФИО работников
        public SelectList WorkersList { get; set; }

        //Комментарии
        public List<CommentViewModel> Comments { get; set; }
        public User CurrentUser { get; set; }

        //Сводная статистика
        public List<SubstationStatisticsModel> Statistics { get; set; }

        //Номер подстанции
        public string PhoneNumber { get; set; }

        public IActionResult OnGet(int? id)

        {
            if (id == null) return NotFound();
            var substation = db.Substations.Find(id);
            if (substation == null) return NotFound();

            Id = substation.Id;
            Name = substation.Name;
            State = substation.SubstationState.Name;
            RegionName = substation.NetRegion.Name;
            RegionId = substation.NetRegionId;
            ContractName = substation.NetRegion.Contract.Name;
            ContractId = substation.NetRegion.ContractId;
            //Флаги
            IsInstallationDone = substation.IsInstallationDone;
            IsPropSchemeDone = substation.IsPropSchemeDone;
            IsBalanceDone = substation.IsBalanceDone;
            IsKS2Done = substation.IsKS2Done;

            //Точки
            RegPointsList = (from p in db.RegPoints
                             let device = db.Devices.FirstOrDefault(d => d.Id == p.DeviceId)
                             join linkD in db.Links on device.Id equals linkD.DeviceId into lDs
                             from linkD in lDs.DefaultIfEmpty()   //Link привязанные к устройствам
                             where p.SubstationId == Id
                             where p.Status == RegPointStatus.Default
                             select new RegPointRowViewModel()
                             {
                                 Id = p.Id,
                                 Address = RegPointRowViewModel.FormatAddress(
                                            p.Consumer.O_Local,
                                            p.Consumer.O_Local_Secondary,
                                            p.Consumer.O_Street,
                                            p.Consumer.O_House,
                                            p.Consumer.O_Build,
                                            p.Consumer.O_Flat
                                            ),
                                 InstallPlace = p.InstallAct.InstallPlaceType.Name + p.InstallAct.InstallPlaceNumber,
                                 TypePU = p.Device.DeviceType.Description,
                                 SerialNum = p.Device.SerialNumber,
                                 PhoneNum = (linkD != null) ? linkD.PhoneNumber : "-",
                                 //Flags
                                 IsLinkOk = p.RegPointFlags.IsLinkOk,
                                 IsAscueChecked = p.RegPointFlags.IsAscueChecked,
                                 IsAscueOk = p.RegPointFlags.IsAscueOk
                             }).ToList();

            //Статистика по ТУ
            Count = db.RegPoints.Count(rp => rp.Status == RegPointStatus.Default && rp.SubstationId == Id);
            CountLinkOk = db.RegPoints.Count(rp => 
                rp.Status == RegPointStatus.Default && 
                rp.SubstationId == Id && 
                rp.RegPointFlags.IsLinkOk && 
                !rp.RegPointFlags.IsAscueChecked && 
                !rp.RegPointFlags.IsAscueOk
            );
            CountAscueChecked = db.RegPoints.Count(rp => 
                rp.Status == RegPointStatus.Default && 
                rp.SubstationId == Id && 
                rp.RegPointFlags.IsAscueChecked && 
                !rp.RegPointFlags.IsAscueOk
            );
            CountAscueOk = db.RegPoints.Count(rp => 
                rp.Status == RegPointStatus.Default && 
                rp.SubstationId == Id && 
                rp.RegPointFlags.IsAscueOk
            );
            CountOther = Count - (CountLinkOk + CountAscueChecked + CountAscueOk);

            //Сводная статистика
            Statistics = (from regpoint in db.RegPoints
                          where regpoint.SubstationId == Id
                          where regpoint.Status == RegPointStatus.Default
                          group regpoint by regpoint.Device.DeviceType.Description into g
                          select new SubstationStatisticsModel()
                          {
                              Name = g.Key,
                              CountIsLinkOk = g.Count(u => u.RegPointFlags.IsLinkOk),
                              CountRegPoints = g.Count(),
                              CountIsAscueChecked = g.Count(u => u.RegPointFlags.IsAscueChecked),
                              CountIsAscueOk = g.Count(u => u.RegPointFlags.IsAscueOk)
                          }).Where(u=>u.Name != "УСПД").ToList();

            //Проценты
            if (Count > 0)
            {
                PercentLinkOk = CountLinkOk * 100 / Count;
                PercentAscueChecked = CountAscueChecked * 100 / Count;
                PercentAscueOk = CountAscueOk * 100 / Count;
                PercentOther = 100 - (PercentLinkOk + PercentAscueChecked + PercentAscueOk);
            }

            //Выпадающий список с ФИО работников
            /*List<dynamic> workersList = new List<dynamic>();
            workersList.Add(new { Id = 0, Name = "" }); //Одна пустая строка
            foreach (var w in db.Workers)
                if (w.IsWorkerType(WorkerTypeName.Mounter) || w.IsWorkerType(WorkerTypeName.PNR))
                    workersList.Add(new
                    {
                        w.Id,
                        Name = w.Surname + " " + w.Name + " " + w.MIddlename
                    });
            WorkersList = new SelectList(workersList, "Id", "Name");*/

            //Комментарии
            CurrentUser = db.Users.FirstOrDefault(u => u.Login == User.Identity.Name);
            Comments = CommentsManager.GetSubstationComments((int)id, db, CurrentUser);

            //Номер Сим подстанции
            SubstationLink substationLink = db.SubstationLinks.LastOrDefault(l => l.SubstationId == Id);
            if (substationLink != null)
                PhoneNumber = substationLink.PhoneNumber;
            else PhoneNumber = "";

            return Page();
        }
    }
    // Model for storing statistical info for substation
    // TODO: Get rid of it xD
    public class SubstationStatisticsModel
    {
        public string Name { get; set; }
        public int CountIsLinkOk { get; set; }
        public int CountIsAscueChecked { get; set; }
        public int CountIsAscueOk { get; set; }
        public int CountRegPoints { get; set; }
        private double percentage;

        public double Percentage
        {
            get
            {
                percentage = (double)CountIsAscueOk * 100 / (double)CountRegPoints;
                return percentage;
            }
        }
    }
}