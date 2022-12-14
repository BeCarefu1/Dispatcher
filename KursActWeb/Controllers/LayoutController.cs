using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GIPManager;
using Newtonsoft.Json;

namespace ActGIPelectroWeb.Controllers
{
    public class LayoutController : Controller
    {
        public StoreContext db = new StoreContext();

        public DateTime checkDate;
        public int userId;

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

        public List<NotificationListViewModal> NotificationList { get; set; }

        [Authorize]

        public string GetNotification()
        {
            try
            {
                int id_worker = db.Users.Find(User.Identity.Name).Id;
                NotificationList = (from a in db.NotificationTable
                                    join b in db.Users on a.FromId equals b.Id
                                    join c in db.ApplicationTable on a.Id_App equals c.Id
                                    join d in db.TypeAlert on a.Id_Type equals d.Id
                                    where a.WhitchId == id_worker
                                    select new NotificationListViewModal
                                    {
                                        id = a.Id,
                                        typeId = d.Id,
                                        type = d.Name,
                                        fromId = b.Id,
                                        fromName = b.Name,
                                        date = a.Date,
                                        appId = c.Id
                                    }).ToList();

                var html = @"<table class='zebra-table'>
                    <tr>
                        <th>ТИП</th>
                        <th>ДАТА</th>
                        <th>ОТ</th>
                    </tr>";

                foreach (var item in NotificationList)
                {
                    html += "<tr><td>" + item.type + "</td>" +
                    "<td>" + item.date + "</td>" +
                    "<td>" + item.fromName + "</td></tr>";
                }
                html += "</table>";

                return JsonConvert.SerializeObject(html.ToString());
            }
            catch (Exception ex) { return JsonConvert.SerializeObject(ex.Message); }
        }








        public string GetTakeAShift()
        {
            try
            {
                string NameUser = db.Users.LastOrDefault(s => s.Id == db.ShiftSchedule.LastOrDefault().Id_Dispatcher).Name;
                //string Comment = db.ShiftSchedule.LastOrDefault().Comment;
                string html = "<h5 class='col' align='center' style='color:black'>Комментарий от " + NameUser + ":</h5>" +
                    "<div style='width: 100%; background: #ffffff; box-shadow: 0 0 10px #A9A9A9; border-radius: 5px;'>" +
                    "<h6 style='color:black; margin: 10px 10px 10px 10px;'></h6>" +
                    //"<h6 style='color:black; margin: 10px 10px 10px 10px;'>" + Comment + "</h6>" +
                    "</div><br /><h6 class='col' align='center'>Комментарий</h6><textarea id='acomment' name='comment' cols='40' rows='9' placeholder='Комментарий'></textarea>" +
                    "<div class='modal-footer'><button class='btn btn-primary' style='width:100%' onclick='Accept()'>Принять смену</button></div>";
                return JsonConvert.SerializeObject(html);
            }
            catch (Exception ex) { return JsonConvert.SerializeObject(ex.Message); }
        }


        public IActionResult Index()
        {
            return View();
        }

    }
}
