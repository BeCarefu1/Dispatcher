using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ActGIPelectroWeb.ViewModels;
using GIPManager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

namespace ActGIPelectroWeb.Pages
{
    [Authorize]
    public class ApplicationsBasePageModel : PageModel
    {
        private readonly StoreContext db = new StoreContext();
        private readonly IConfiguration Configuration;

        public ApplicationsBasePageModel(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public string NameSort { get; set; }
        public string CurrentFilter { get; set; }
        public string CurrentSort { get; set; }
        public int PageNumb { get; set; }
        public int PageCount { get; set; }
        public int Count { get; set; }


        public List<OrderListViewModel> NetRegionList { get; set; }
        public List<OrderListViewModel> SubstationList { get; set; }
        public List<OrderListViewModel> CategoryList { get; set; }
        public List<OrderListViewModel> RepairList { get; set; }
        public List<OrderListViewModel> AGList { get; set; }
        public List<OrderListViewModel> StatusList { get; set; }
        public List<OrderListViewModel> ObjectList { get; set; }
        public List<OrderListViewModel> EquipmentList { get; set; }
        public List<OrderListViewModel> SecurityList { get; set; }
        public List<ApplicationListViewModel> OrderList { get; set; }
        public List<ApplicationListViewModel> ExampleDescription { get; set; }
        public List<ApplicationListViewModel> SecurityMeasures { get; set; }

        public class NetRegList
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class DateList
        {
            public int Id { get; set; } 
            public string Date { get; set; }
        }

        public List<NetRegList> NetRegs { get; set; }
        public List<DateList> Dates { get; set; }

        public List<string> DateArr = new List<string>();

        public int? AppId { get; set; } 
        public int UserId { get; set; }
        //public int Count { get; set; }

        public string GetColor(int id)
        {
            switch (id)
            {
                case 4:
                    return "#d4ffc5"; //"#e7ffe3";//"#98FB98"; //"#C3FBD8";
                    break;
            }
            return "";
        }

        public string IdSort { get; set; }
        public string CategorySort { get; set; }
        public string NetSort { get; set; }
        public string DateSort { get; set; }
        public string ObjectSort { get; set; }
        public string EquipmentSort { get; set; }
        public string DescriptionSort { get; set; }
        public string ReqTimeSort { get; set; }
        public string MasterSort { get; set; }
        public string StatusSort { get; set; }


        public async Task OnGet(int? id, string sortOrder, string filterOrder)
        {
            CurrentSort = sortOrder;
            DateSort = String.IsNullOrEmpty(sortOrder) ? "Date_desc" : "";
            IdSort = sortOrder == "AscendingId" ? "Id_desc" : "AscendingId";
            CategorySort = sortOrder == "AscendingCategory" ? "Category_desc" : "AscendingCategory";
            NetSort = sortOrder == "AscendingNet" ? "Net_desc" : "AscendingNet";
            ObjectSort = sortOrder == "AscendingObject" ? "Object_desc" : "AscendingObject";
            EquipmentSort = sortOrder == "AscendingEquipment" ? "Equipment_desc" : "AscendingEquipment";
            DescriptionSort = sortOrder == "AscendingDescription" ? "Description_desc" : "AscendingDescription";
            ReqTimeSort = sortOrder == "AscendingReqTime" ? "ReqTime_desc" : "AscendingReqTime";
            MasterSort = sortOrder == "AscendingMaster" ? "Master_desc" : "AscendingMaster";
            StatusSort = sortOrder == "AscendingStatus" ? "Status_desc" : "AscendingStatus";

            AppId = id;
            UserId = db.Users.FirstOrDefault(s => s.Login == User.Identity.Name).Id;
            CurrentFilter = filterOrder;

            IQueryable<ApplicationListViewModel> OrderListIQ;

            if (User.IsInRole("master"))
            {
                OrderListIQ = (from a in db.ApplicationTable
                               join b in db.ApplicationAG on a.Id_AG equals b.Id
                               join d in db.ApplicationCategory on a.Id_Category equals d.Id
                               join e in db.ApplicationRepair on a.Id_Repair equals e.Id
                               //join f in db.ApplicationState on a.Id_State equals f.Id
                               join g in db.NetRegions on a.Id_NetRegion equals g.Id
                               //join h in db.Substations on g.Id equals h.NetRegionId
                               //join i in db.EquipmentTable on a.Id_Equipment equals i.Id
                               join j in db.AppStatus on a.Id_Status equals j.Id
                               join k in db.Users on a.Id_User equals k.Id
                               where (a.Id_Status == 1 || a.Id_Status == 2 || a.Id_Status == 4 || a.Id_Status == 6 || a.Id_Status == 9 || a.Id_Status == 10)
                               //where a.Id_User == db.Users.FirstOrDefault(s => s.Login == User.Identity.Name).Id
                               orderby a.Id_User == db.Users.FirstOrDefault(s => s.Login == User.Identity.Name).Id descending
                               //orderby a.Date descending
                               select new ApplicationListViewModel
                               {
                                   Id = a.Id,
                                   Date = a.Date,
                                   NetRegion = g.Name,
                                   Category = d.Name,
                                   Repair = e.Name,
                                   Ag = b.Time,
                                   //state = f.Name,
                                   ObjectName = a.ObjectName,
                                   Equipment = a.Equipment,
                                   Description = a.Description.Replace("\n", "<br>"),
                                   RequestedDate = a.StartReqTime,
                                   RequestedStartTime = a.StartReqTime,
                                   RequestedEndTime = a.EndReqTime,
                                   ActualDate = a.StartActualTime,
                                   ActualStartTime = a.StartActualTime,
                                   ActualEndTime = a.EndActualTime,
                                   OpenTime = a.OpenTime,
                                   CloseTime = a.CloseTime,
                                   Security = a.SecurityDescription.Replace("\n", "<br>"),
                                   StatusId = a.Id_Status,
                                   Status = j.Name,
                                   Color = GetColor(a.Id_Status),
                                   NambId = a.NumbApp,
                                   NameMaster = Models.ShortNameModel.ShortName(k.Name),
                                   UserId = a.Id_User
                               });

                

                int netregId = Convert.ToInt32((from a in db.Users
                                                join b in db.Workers on a.WorkerID equals b.Id
                                                where a.Login == User.Identity.Name
                                                select b.ID_NetRegion).First());

                NetRegionList = (from a in db.NetRegions
                                 where a.Id == netregId
                                 select new OrderListViewModel
                                 {
                                     Id = a.Id,
                                     Name = a.Name
                                 }).ToList();

                SubstationList = (from a in db.Substations
                                  where a.NetRegionId == netregId
                                  select new OrderListViewModel
                                  {
                                      Id = a.Id,
                                      Name = a.Name
                                  }).ToList();

                ObjectList = (from a in db.ObjectTable
                              join b in db.NetRegions on a.IdNetRegion equals b.Id
                              where b.Id == netregId
                              select new OrderListViewModel
                              {
                                  Id = a.Id,
                                  Name = a.Name
                              }).ToList();

                EquipmentList = (from a in db.EquipmentTable
                                 join b in db.NetRegions on a.IdNetRegion equals b.Id
                                 where b.Id == netregId
                                 select new OrderListViewModel
                                 {
                                     Id = a.Id,
                                     Name = a.Name
                                 }).ToList();

                CategoryList = (from a in db.ApplicationCategory
                                select new OrderListViewModel
                                {
                                    Id = a.Id,
                                    Name = a.Name
                                }).ToList();

                RepairList = (from a in db.ApplicationRepair
                              select new OrderListViewModel
                              {
                                  Id = a.Id,
                                  Name = a.Name
                              }).ToList();

                AGList = (from a in db.ApplicationAG
                          select new OrderListViewModel
                          {
                              Id = a.Id,
                              Name = a.Time
                          }).ToList();

                StatusList = (from a in db.AppStatus
                              select new OrderListViewModel
                              {
                                  Id = a.Id,
                                  Name = a.Name
                              }).ToList();

            }
            else if(User.IsInRole("dispatcher") || User.IsInRole("administrator"))
            {
                OrderListIQ = (from a in db.ApplicationTable
                               join b in db.ApplicationAG on a.Id_AG equals b.Id
                               join d in db.ApplicationCategory on a.Id_Category equals d.Id
                               join e in db.ApplicationRepair on a.Id_Repair equals e.Id
                               //join f in db.ApplicationState on a.Id_State equals f.Id
                               join g in db.NetRegions on a.Id_NetRegion equals g.Id
                               //join h in db.Substations on g.Id equals h.NetRegionId
                               //join i in db.EquipmentTable on a.Id_Equipment equals i.Id
                               join j in db.AppStatus on a.Id_Status equals j.Id
                               join k in db.Users on a.Id_User equals k.Id
                               where a.Id_Status == 1 || a.Id_Status == 2 || a.Id_Status == 4 || a.Id_Status == 6 || a.Id_Status == 9 || a.Id_Status == 10
                               orderby a.Date descending
                               select new ApplicationListViewModel
                               {
                                   Id = a.Id,
                                   Date = a.Date,
                                   NetRegion = g.Name,
                                   Category = d.Name,
                                   Repair = e.Name,
                                   Ag = b.Time,
                                   //state = f.Name,
                                   ObjectName = a.ObjectName,
                                   Equipment = a.Equipment,
                                   Description = a.Description,
                                   RequestedDate = a.StartReqTime,
                                   RequestedStartTime = a.StartReqTime,
                                   RequestedEndTime = a.EndReqTime,
                                   ActualDate = a.StartActualTime,
                                   ActualStartTime = a.StartActualTime,
                                   ActualEndTime = a.EndActualTime,
                                   OpenTime = a.OpenTime,
                                   CloseTime = a.CloseTime,
                                   Security = a.SecurityDescription,
                                   StatusId = a.Id_Status,
                                   Status = j.Name,
                                   Color = GetColor(a.Id_Status),
                                   NambId = a.NumbApp,
                                   NameMaster = Models.ShortNameModel.ShortName(k.Name),
                                   UserId = a.Id_User


                               });

                NetRegionList = (from a in db.NetRegions
                                 select new OrderListViewModel
                                 {
                                     Id = a.Id,
                                     Name = a.Name
                                 }).ToList();

                SubstationList = (from a in db.Substations
                                  select new OrderListViewModel
                                  {
                                      Id = a.Id,
                                      Name = a.Name
                                  }).ToList();

                ObjectList = (from a in db.ObjectTable
                              join b in db.NetRegions on a.IdNetRegion equals b.Id
                              select new OrderListViewModel
                              {
                                  Id = a.Id,
                                  Name = a.Name
                              }).ToList();

                EquipmentList = (from a in db.EquipmentTable
                                 join b in db.NetRegions on a.IdNetRegion equals b.Id
                                 select new OrderListViewModel
                                 {
                                     Id = a.Id,
                                     Name = a.Name
                                 }).ToList();

                CategoryList = (from a in db.ApplicationCategory
                                select new OrderListViewModel
                                {
                                    Id = a.Id,
                                    Name = a.Name
                                }).ToList();

                RepairList = (from a in db.ApplicationRepair
                              select new OrderListViewModel
                              {
                                  Id = a.Id,
                                  Name = a.Name
                              }).ToList();

                AGList = (from a in db.ApplicationAG
                          select new OrderListViewModel
                          {
                              Id = a.Id,
                              Name = a.Time
                          }).ToList();

                StatusList = (from a in db.AppStatus
                              select new OrderListViewModel
                              {
                                  Id = a.Id,
                                  Name = a.Name
                              }).ToList();
            }
            else
            {
                OrderListIQ = (from a in db.ApplicationTable
                               join b in db.ApplicationAG on a.Id_AG equals b.Id
                               join d in db.ApplicationCategory on a.Id_Category equals d.Id
                               join e in db.ApplicationRepair on a.Id_Repair equals e.Id
                               //join f in db.ApplicationState on a.Id_State equals f.Id
                               join g in db.NetRegions on a.Id_NetRegion equals g.Id
                               //join h in db.Substations on g.Id equals h.NetRegionId
                               //join i in db.EquipmentTable on a.Id_Equipment equals i.Id
                               join j in db.AppStatus on a.Id_Status equals j.Id
                               join k in db.Users on a.Id_User equals k.Id
                               where a.Id_Status == 1 || a.Id_Status == 2 || a.Id_Status == 4 || a.Id_Status == 6 || a.Id_Status == 9 || a.Id_Status == 10
                               orderby a.Date descending
                               select new ApplicationListViewModel
                               {
                                   Id = a.Id,
                                   Date = a.Date,
                                   NetRegion = g.Name,
                                   Category = d.Name,
                                   Repair = e.Name,
                                   Ag = b.Time,
                                   //state = f.Name,
                                   ObjectName = a.ObjectName,
                                   Equipment = a.Equipment,
                                   Description = a.Description,
                                   RequestedDate = a.StartReqTime,
                                   RequestedStartTime = a.StartReqTime,
                                   RequestedEndTime = a.EndReqTime,
                                   ActualDate = a.StartActualTime,
                                   ActualStartTime = a.StartActualTime,
                                   ActualEndTime = a.EndActualTime,
                                   OpenTime = a.OpenTime,
                                   CloseTime = a.CloseTime,
                                   Security = a.SecurityDescription,
                                   StatusId = a.Id_Status,
                                   Status = j.Name,
                                   Color = GetColor(a.Id_Status),
                                   NambId = a.NumbApp,
                                   NameMaster = Models.ShortNameModel.ShortName(k.Name)
                                   
                               });
            }

            // *** ОП для фильтра ***
            NetRegs = (from a in db.NetRegions
                       select new NetRegList                                                                                                                                                                                                                                        
                       {
                           Id = a.Id,
                           Name = a.Name
                       }).ToList();

            // *** Даты для фильтра ***
            Dates = (from a in db.ApplicationTable
                            orderby a.StartReqTime ascending
                            where a.Id_Status == 1 || a.Id_Status == 2 || a.Id_Status == 4 || a.Id_Status == 6 || a.Id_Status == 9 || a.Id_Status == 10
                            select new DateList
                            {
                                Id = a.Id,
                                Date = a.StartReqTime.ToShortDateString()
                            }).ToList();

            // *** Удаление одинаковых дат ***
            for (int l = 0; l < Dates.Count(); l++)
            {
                for (int i = 0; i < Dates.Count(); i++)
                {
                    for (int j = 0; j < Dates.Count(); j++)
                    {
                        if (Dates[i].Date == Dates[j].Date && Dates[i].Id != Dates[j].Id)
                        {
                            Dates.Remove(Dates[j]);
                        }
                    }
                }
            }

            // Сортировка
            switch (sortOrder)
            {
                case "Date_desc":
                    OrderListIQ = OrderListIQ.OrderBy(s => s.Date);
                    break;
                case "AscendingId":
                    OrderListIQ = OrderListIQ.OrderBy(s => s.NambId);
                    break;
                case "Id_desc":
                    OrderListIQ = OrderListIQ.OrderByDescending(s => s.NambId);
                    break;
                case "AscendingCategory":
                    OrderListIQ = OrderListIQ.OrderBy(s => s.Category);
                    break;
                case "Category_desc":
                    OrderListIQ = OrderListIQ.OrderByDescending(s => s.Category);
                    break;
                case "AscendingNet":
                    OrderListIQ = OrderListIQ.OrderBy(s => s.NetRegion);
                    break;
                case "Net_desc":
                    OrderListIQ = OrderListIQ.OrderByDescending(s => s.NetRegion);
                    break;
                case "AscendingObject":
                    OrderListIQ = OrderListIQ.OrderBy(s => s.ObjectName);
                    break;
                case "Object_desc":
                    OrderListIQ = OrderListIQ.OrderByDescending(s => s.ObjectName);
                    break;
                case "AscendingEquipment":
                    OrderListIQ = OrderListIQ.OrderBy(s => s.Equipment);
                    break;
                case "Equipment_desc":
                    OrderListIQ = OrderListIQ.OrderByDescending(s => s.Equipment);
                    break;
                case "AscendingDescription":
                    OrderListIQ = OrderListIQ.OrderBy(s => s.Description);
                    break;
                case "Description_desc":
                    OrderListIQ = OrderListIQ.OrderByDescending(s => s.Description);
                    break;
                case "AscendingReqTime":
                    OrderListIQ = OrderListIQ.OrderBy(s => s.RequestedStartTime);
                    break;
                case "ReqTime_desc":
                    OrderListIQ = OrderListIQ.OrderByDescending(s => s.RequestedStartTime);
                    break;
                case "AscendingMaster":
                    OrderListIQ = OrderListIQ.OrderBy(s => s.NameMaster);
                    break;
                case "Master_desc":
                    OrderListIQ = OrderListIQ.OrderByDescending(s => s.NameMaster);
                    break;
                case "AscendingStatus":
                    OrderListIQ = OrderListIQ.OrderBy(s => s.Status);
                    break;
                case "Status_desc":
                    OrderListIQ = OrderListIQ.OrderByDescending(s => s.Status);
                    break;
                default:
                    break;
            }

            // Фильтр
            if (!String.IsNullOrEmpty(filterOrder))
            {
                OrderListIQ = OrderListIQ.Where(s => s.NetRegion.Contains(filterOrder) || s.RequestedStartTime.ToShortDateString().Contains(filterOrder) || s.Status.Contains(filterOrder));
            }


            OrderList = new List<ApplicationListViewModel>(OrderListIQ);
            // NetRegionList = (from a in db.NetRegions
            //                  select new OrderListViewModel
            //                  {
            //                      Id = a.Id,
            //                      Name = a.Name
            //                  }).ToList();

            //SecurityList = (from a in db.SecurityTable
            //         select new OrderListViewModel
            //         {
            //             Id = a.Id,
            //             Name = a.Description
            //         }).ToList();

            //switch (sortOrder)
            //{
            //    case "Date_desc":
            //        OrderListIQ = OrderListIQ.OrderBy(s => s.date);
            //        break;
            //    case "RequestedDate":
            //        OrderListIQ = OrderListIQ.OrderBy(s => s.requestedDate);
            //        break;
            //    case "RequestedDate_desc":
            //        OrderListIQ = OrderListIQ.OrderByDescending(s => s.requestedDate);
            //        break;
            //    default:
            //        OrderListIQ = OrderListIQ.OrderByDescending(s => s.date);
            //        break;
            //}
            //
            //OrderList = new List<ApplicationListViewModel>(OrderListIQ);

            //return Page();
        }
    }
}
