using GIPManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ActGIPelectroWeb.Models
{
    public class EventHistoryModel
    {
        public static readonly StoreContext db = new StoreContext();

        public static async Task AddEvent(string events, string login)
        {
            EventHistory eventHistory = new EventHistory();
            eventHistory.Event = events;
            eventHistory.Date = DateTime.Now;
            eventHistory.UserId = db.Users.FirstOrDefault(s => s.Login == login).Id;
            //eventHistory.IpAddress = new System.Net.WebClient().DownloadString("https://api.ipify.org");
             await Task.Run(async delegate { await eventHistory.AddEventHistoryAsync(); });
        }
    }
}
