using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ActGIPelectroWeb.Models;
using ActGIPelectroWeb.ViewModels;
using GIPManager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PagedList.Mvc;

namespace ActGIPelectroWeb.Pages
{
    [Authorize]
    public class ApplicationsBaseArchiveModel : PageModel
    {
        private readonly StoreContext db = new StoreContext();
        private readonly IConfiguration Configuration;

        public ApplicationsBaseArchiveModel(IConfiguration configuration)
        {
            Configuration = configuration;
        }

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

        public string NameSort { get; set; }
        public string DateSort { get; set; }
        public string CurrentFilter { get; set; }
        public string CurrentSort { get; set; }
        public int PageNumb { get; set; }
        public int PageCount { get; set; }
        public int Count { get; set; }
        public int UserId { get; set; }
        public PaginatedList<ApplicationListViewModel> OrderList { get; set; }
        public List<NetRegList> NetRegs { get; set; }
        public List<DateList> Dates { get; set; }
        //public List<ApplicationListViewModel> OrderList { get; set; }

        public int? AppId { get; set; }

        public async Task OnGetAsync(int id, string sortOrder, string currentFilter, string searchString, int? pageIndex)
        {
            CurrentSort = sortOrder;
            NameSort = String.IsNullOrEmpty(sortOrder) ? "Date_desc" : "";
            DateSort = sortOrder == "RequestedDate" ? "RequestedDate_desc" : "RequestedDate";
            

            AppId = id;
            UserId = db.Users.FirstOrDefault(s => s.Login == User.Identity.Name).Id;
            Count = db.ApplicationTable.Count();

            if (searchString != null)
            {
                pageIndex = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            CurrentFilter = searchString;


            

            IQueryable<ApplicationListViewModel> OrderListIQ = (from a in db.ApplicationTable
                                                             join b in db.ApplicationAG on a.Id_AG equals b.Id
                                                             join d in db.ApplicationCategory on a.Id_Category equals d.Id
                                                             join e in db.ApplicationRepair on a.Id_Repair equals e.Id
                                                             //join f in db.ApplicationState on a.Id_State equals f.Id
                                                             join g in db.NetRegions on a.Id_NetRegion equals g.Id
                                                             //join h in db.Substations on g.Id equals h.NetRegionId
                                                             //join i in db.EquipmentTable on a.Id_Equipment equals i.Id
                                                             join j in db.AppStatus on a.Id_Status equals j.Id
                                                             join k in db.Users on a.Id_User equals k.Id
                                                             where a.Id_Status == 3 || a.Id_Status == 5 || a.Id_Status == 7 || a.Id_Status == 8
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
                                                                 NambId = a.NumbApp,
                                                                 NameMaster = Models.ShortNameModel.ShortName(k.Name),
                                                                 UserId = a.Id_User

                                                             });//.ToList();

            
            // Сортировка
            switch (sortOrder)
            {
                case "Date_desc":
                    OrderListIQ = OrderListIQ.OrderBy(s => s.Date);
                    break;
                case "RequestedDate":
                    OrderListIQ = OrderListIQ.OrderBy(s => s.RequestedDate);
                    break;
                case "RequestedDate_desc":
                    OrderListIQ = OrderListIQ.OrderByDescending(s => s.RequestedDate);
                    break;
                default:
                    OrderListIQ = OrderListIQ.OrderByDescending(s => s.Date);
                    break;
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
                     where a.Id_Status == 3 || a.Id_Status == 5 || a.Id_Status == 7 || a.Id_Status == 8
                     select new DateList
                     {
                         Id = a.Id,
                         Date = a.StartReqTime.ToShortDateString()
                     }).Take(35).ToList();

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

            // Поиск
            if (!String.IsNullOrEmpty(searchString))
            {
                OrderListIQ = OrderListIQ.Where(s => s.NetRegion.Contains(searchString) || s.NameMaster.Contains(searchString) || s.Description.Contains(searchString) || s.Category.Contains(searchString) || s.Repair.Contains(searchString) || s.ObjectName.Contains(searchString) || s.Equipment.Contains(searchString) || s.Security.Contains(searchString) || s.Status.Contains(searchString) || s.NambId.ToString().Contains(searchString) || s.RequestedStartTime.ToShortDateString().Contains(searchString));
            }

            var pageSize = Configuration.GetValue("PageSize", 17);
            OrderList = await PaginatedList<ApplicationListViewModel>.CreateAsync(
                OrderListIQ.AsNoTracking(), pageIndex ?? 1, pageSize);


            if (pageIndex != null)
            {
                PageNumb = (int)pageIndex;
            }
            else
            {
                PageNumb = 1;
            }
            if (OrderListIQ.Count() % 17 == 0 || OrderListIQ.Count() < 0)
            {
                PageCount = OrderListIQ.Count() / 17;
            }
            else
            {
                PageCount = OrderListIQ.Count() / 17 + 1;
            }

            //if (pageIndex != null)
            //{
            //    PageNumb = (int)pageIndex;
            //}
            //else
            //{
            //    PageNumb = 1;
            //}
            //if (db.ApplicationTable.Count(a => a.Id_Status == 3 || a.Id_Status == 5 || a.Id_Status == 7 || a.Id_Status == 8) % 17 == 0 || db.ApplicationTable.Count(a => a.Id_Status == 3 || a.Id_Status == 5 || a.Id_Status == 7 || a.Id_Status == 8) < 0)
            //{
            //    PageCount = db.ApplicationTable.Count(a => a.Id_Status == 3 || a.Id_Status == 5 || a.Id_Status == 7 || a.Id_Status == 8) / 17;
            //}
            //else
            //{
            //    PageCount = db.ApplicationTable.Count(a => a.Id_Status == 3 || a.Id_Status == 5 || a.Id_Status == 7 || a.Id_Status == 8) / 17 + 1;
            //}
        }
    }
}
