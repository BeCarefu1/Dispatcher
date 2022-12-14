using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Linq;
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
    public class HistoryPageModel : PageModel
    {
        public readonly StoreContext db = new StoreContext();
        private readonly IConfiguration Configuration;

        public HistoryPageModel(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public class HistoryList
        {
            public int Id { get; set; }
            public string Event { get; set; }
            public DateTime Date { get; set; }
            public int UserId { get; set; }
            public string Name { get; set; }
            //public string IpAddress { get; set; }
            //public string NetRegion { get; set; }
        }

        public string DateSort { get; set; }
        public string CurrentFilter { get; set; }
        public string CurrentSort { get; set; }

        public int PageNumb { get; set; }
        public int PageCount { get; set; }


        public PaginatedList<HistoryList> Historys { get; set; }

        //public List<HistoryList> Historys { get; set; }

        

        public async Task OnGetAsync(string sortOrder, string currentFilter, string searchString, int? pageIndex)
        {
            CurrentSort = sortOrder;
            DateSort = String.IsNullOrEmpty(sortOrder) ? "Date_desc" : "";

            //int netregId = Convert.ToInt32((from a in db.Users
            //                                join b in db.Workers on a.WorkerID equals b.Id
            //                                where a.Login == User.Identity.Name
            //                                select b.ID_NetRegion).First());

            if (searchString != null)
            {
                pageIndex = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            CurrentFilter = searchString;


            IQueryable<HistoryList> HistorysIQ = (from a in db.EventHistory
                                                join b in db.Users on a.UserId equals b.Id
                                                select new HistoryList
                                                {
                                                    Id = a.Id,
                                                    Event = a.Event,
                                                    Date = a.Date,
                                                    UserId = b.Id,
                                                    Name = Models.ShortNameModel.ShortName(b.Name)
                                                    //IpAddress = a.IpAddress
                                                    //NetRegion = db.NetRegions.Find(netregId).Name
                                                });


            // Сортировка
            switch (sortOrder)
            {
                case "Date_desc":
                    HistorysIQ = HistorysIQ.OrderBy(s => s.Date);
                    break;
                default:
                    HistorysIQ = HistorysIQ.OrderByDescending(s => s.Date);
                    break;
            }

            // Поиск
            if (!String.IsNullOrEmpty(searchString))
            {
                HistorysIQ = HistorysIQ.Where(s => s.Event.Contains(searchString) || s.Date.ToShortDateString().Contains(searchString));
            }

            var pageSize = Configuration.GetValue("PageSize", 17);
            Historys = await PaginatedList<HistoryList>.CreateAsync(
                HistorysIQ.AsNoTracking(), pageIndex ?? 1, pageSize);


            if (pageIndex != null)
            {
                PageNumb = (int)pageIndex;
            }
            else
            {
                PageNumb = 1;
            }
            if (HistorysIQ.Count() % 17 == 0 || HistorysIQ.Count() < 0)
            {
                PageCount = HistorysIQ.Count() / 17;
            }
            else
            {
                PageCount = HistorysIQ.Count() / 17 + 1;
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
