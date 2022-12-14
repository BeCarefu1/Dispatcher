using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GIPManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ActGIPelectroWeb.Pages
{
    public class AlertPageModel : PageModel
    {
        private readonly StoreContext db;
        public AlertPageModel(StoreContext context)
        {
            db = context;
        }
        public class NotificationListViewModal
        {
            public int id { get; set; }
            public int typeId { get; set; }
            public string type { get; set; }
            public DateTime date { get; set; }
            public int fromId { get; set; }
            public string fromName { get; set; }
            public int whitchId { get; set; }
            public int appId { get; set; }
            public string link { get; set; }
            public int nambApp { get; set; }
        }

        public string GetLink(int id, int typeId)
        {
            string link = "";
            if (typeId == 1 || typeId == 2 || typeId == 4 || typeId == 6 || typeId == 9 || typeId == 10 || typeId == 11)
            {
                link = "/ApplicationsBasePage/" + id;
            }
            else if (typeId == 3 || typeId == 5 || typeId == 7 || typeId == 8)
            {
                link = "/ApplicationsBaseArchive/" + id;
            }
            else
            {
                link = "/AlertPage";
            }
            return link;
        }

        public List<NotificationListViewModal> NotificationList { get; set; }

        public int MarqueeId { get; set; }

        public void OnGet()
        {
            int id_worker = db.Users.FirstOrDefault(s => s.Login == User.Identity.Name).Id;
            NotificationList = (from a in db.NotificationTable
                                join b in db.Users on a.FromId equals b.Id
                                join c in db.ApplicationTable on a.Id_App equals c.Id
                                join d in db.TypeAlert on a.Id_Type equals d.Id
                                where a.WhitchId == id_worker
                                select new NotificationListViewModal
                                {
                                    id = a.Id,
                                    typeId = d.Id,
                                    type = d.Name.Replace("<nA>", "¹" + c.NumbApp),
                                    fromId = b.Id,
                                    fromName = Models.ShortNameModel.ShortName(b.Name),
                                    date = a.Date,
                                    appId = c.Id,
                                    nambApp = c.NumbApp,
                                    link = GetLink(c.Id, d.Id)
                                }).ToList();
            
            //if(db.MarqueeTable.LastOrDefault(a => a.Date.AddDays(1) >= DateTime.Now) != null)
            //{
            //        MarqueeId = db.MarqueeTable.LastOrDefault(a => a.Date.AddDays(1) >= DateTime.Now).Id;
            //    
            //}
        }
    }
}
