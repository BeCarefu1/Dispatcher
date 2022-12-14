using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ActGIPelectroWeb.Models;
using ActGIPelectroWeb.ViewModels;
using GIPManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PagedList.Mvc;


namespace ActGIPelectroWeb.Pages
{
    public class OperLogPageModel : PageModel
    {
        public StoreContext db = new StoreContext();
        private readonly IConfiguration Configuration;

        public OperLogPageModel(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public string NameSort { get; set; }
        public string DateSort { get; set; }
        public string CurrentFilter { get; set; }
        public string CurrentSort { get; set; }
        public int PageNumb { get; set; }
        public int PageCount { get; set; }

        //public IList<OperationLogViewModel> operations { get; set; }

        public PaginatedList<OperationLogViewModel> Operations { get; set; }

        public async Task OnGetAsync(string sortOrder, string currentFilter, string searchString, int? pageIndex)
        {
            CurrentSort = sortOrder;
            NameSort = String.IsNullOrEmpty(sortOrder) ? "RecoilTime_desc" : "";
            DateSort = sortOrder == "TineOfApplication" ? "TineOfApplication_desc" : "TineOfApplication";

            if(searchString != null)
            {
                pageIndex = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            CurrentFilter = searchString;

            IQueryable<OperationLogViewModel> operationIQ = (from a in db.OperationalLog
                                                             orderby a.RecoilTime descending
                                                             select new OperationLogViewModel
                                                             {
                                                                 Id = a.Id,
                                                                 RecoilTime = a.RecoilTime,
                                                                 Description = a.Description.Replace("\n", "<br>"),
                                                                 TineOfApplication = a.TineOfApplication,
                                                                 Remarks = a.Remarks.Replace("\n", "<br>"),
                                                                 VisaUserName = Models.ShortNameModel.ShortName(db.Users.FirstOrDefault(s => s.Id == a.VisaUserId).Name),
                                                                 Numb = a.Numb,
                                                                 Name = Models.ShortNameModel.ShortName(db.Users.SingleOrDefault(s => s.Id == a.UserId).Name)
                                                             });

           
            // Сортировка
            switch (sortOrder)
            {
                case "RecoilTime_desc":
                    operationIQ = operationIQ.OrderBy(s => s.RecoilTime);
                    break;
                case "TineOfApplication":
                    operationIQ = operationIQ.OrderBy(s => s.TineOfApplication);
                    break;
                case "TineOfApplication_desc":
                    operationIQ = operationIQ.OrderByDescending(s => s.TineOfApplication);
                    break;
                default:
                    operationIQ = operationIQ.OrderByDescending(s => s.RecoilTime);
                    break;
            }

            // Поиск
            if (!String.IsNullOrEmpty(searchString))
            {
                operationIQ = operationIQ.Where(s => s.RecoilTime.ToString().Contains(searchString) || s.Description.Contains(searchString)
                                       || s.Remarks.Contains(searchString) || s.TineOfApplication.ToString().Contains(searchString));
            }

            var pageSize = Configuration.GetValue("PageSize", 17);
            Operations = await PaginatedList<OperationLogViewModel>.CreateAsync(operationIQ.AsNoTracking(), pageIndex ?? 1, pageSize);


            if (pageIndex != null)
            {
                PageNumb = (int)pageIndex;
            }
            else
            {
                PageNumb = 1;
            }
            if (operationIQ.Count() % 17 == 0)
            {
                PageCount = operationIQ.Count() / 17;
            }
            else
            {
                PageCount = operationIQ.Count() / 17 + 1;
            }

        }
        
    }
}
