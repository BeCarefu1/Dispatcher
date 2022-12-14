using GIPManager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ActGIPelectroWeb.Controllers
{
    public class NotificationController : Controller
    {
        StoreContext db = new StoreContext();

        public class UserList
        {
            public int id { get; set; }
        }

        public List<UserList> usersList = new List<UserList>();
        [Authorize]


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
        }

        public class MarqueeList
        {
            public int Id { get; set; }
            public string Text { get; set; }
            public string UserName { get; set; }
        }

        //public List<NotificationListViewModal> NotificationList { get; set; }

        [Authorize]

        public string GetCountNotif()
        {
            try
            {
                int id_worker = db.Users.FirstOrDefault(s => s.Login == User.Identity.Name).Id;
                int count = db.NotificationTable.Count(s => s.WhitchId == id_worker);
                return JsonConvert.SerializeObject(count);
            }
            catch (Exception ex) { return JsonConvert.SerializeObject(ex.Message); }
        }

        [Authorize]        
        public string CreateMarquee(string comment, DateTime startDate, DateTime endDate)
        {
            try
            {
                MarqueeTable marquee = new MarqueeTable();
                marquee.Description = comment;
                marquee.StartDate = startDate;
                marquee.EndDate = endDate;
                marquee.UserId = db.Users.SingleOrDefault(u => u.Login == User.Identity.Name).Id;
                
                return JsonConvert.SerializeObject(marquee.AddMarquee());
            }
            catch (Exception ex) { return JsonConvert.SerializeObject(ex.Message); }
        }

        public string GetMarquee()
        {
            string marquee = "";
            if (db.MarqueeTable.LastOrDefault(a => a.StartDate <= DateTime.Now && a.EndDate >= DateTime.Now) != null)
            {
                marquee += "<marquee direction='left' scrollamount='5' style='width: 65vh; color: #ff0000; text-transform: uppercase; font-weight:bold;'>" + db.MarqueeTable.LastOrDefault().Description + "</marquee>";
                if(db.MarqueeTable.LastOrDefault().UserId == db.Users.LastOrDefault(s => s.Login == User.Identity.Name).Id || User.IsInRole("administrator"))
                {
                    marquee += "<button class='btn del-marque'><i class='fa fa-times'></i></button>";
                }
            }
            //
            return JsonConvert.SerializeObject(marquee);
        }

        public string DelMarquee(int id)
        {
            MarqueeTable marquee = db.MarqueeTable.Find(id);
        
        
            return JsonConvert.SerializeObject(marquee.DelMarquee());
        }

        public IActionResult Index()
        {
            return View();
        }

    }
}
