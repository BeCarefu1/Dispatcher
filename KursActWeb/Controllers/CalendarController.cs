using GIPManager;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using ActGIPelectroWeb.ViewModels;

namespace ActGIPelectroWeb.Controllers
{
    public class CalendarController : Controller
    {
        StoreContext db;

        public CalendarController(StoreContext context)
        {
            db = context;
        }

        public class EventList
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public string Description { get; set; }
            public string Organizer { get; set; }
        }

        public class PhonegramList
        {
            public int Id { get; set; }
            public string Theme { get; set; }
            public string Description { get; set; }
            public string FromNameUser { get; set; }
        }

        public int ColWeek(int startMon, int colDay)
        {
            double colWeek = (startMon + colDay) / 7;
            if (colWeek == 4) return 4;
            else if (colWeek > 4 && colWeek <= 5) return 5;
            else return 6;
        }

        public int ColApp(int day, int month, int year)
        {
            DateTime date = new DateTime(year, month, day);
            //int col = db.OrderTable.Count(s => s.Id_OrderStatus == 8 && s.StartDate.Date == date);
            int col = db.ApplicationTable.Count(s => (s.Id_Status == 4 || s.Id_Status == 6) && (Convert.ToDateTime(s.StartReqTime.ToShortDateString()) <= date) && Convert.ToDateTime(s.EndReqTime.ToShortDateString()) >= date);
            return col;
        }

        public int ColEvent(int day, int month, int year)
        {
            DateTime date = new DateTime(year, month, day);
            //int col = db.OrderTable.Count(s => s.Id_OrderStatus == 8 && s.StartDate.Date == date);
            int col = db.EventTable.Count(s => Convert.ToDateTime(s.StartDate.ToShortDateString()) <= date && Convert.ToDateTime(s.EndDate.ToShortDateString()) >= date);
            return col;
        }

        public int ColPhonegram(int day, int month, int year)
        {
            //where Convert.ToDateTime(a.Date.ToShortDateString()) == date && (a.ToUserId == db.Users.FirstOrDefault(s => s.Login == User.Identity.Name).Id || a.FromUserId == db.Users.FirstOrDefault(s => s.Login == User.Identity.Name).Id)
            //return db.PhonegramTable.Count(s => Convert.ToDateTime(s.Date.ToShortDateString()) == new DateTime(year, month, day));
            return db.PhonegramTable.Count(s => Convert.ToDateTime(s.Date.ToShortDateString()) == new DateTime(year, month, day) && (s.ToUserId == db.Users.FirstOrDefault(u => u.Login == User.Identity.Name).Id || s.FromUserId == db.Users.FirstOrDefault(u => u.Login == User.Identity.Name).Id));
        }

        public List<OrderListViewModel> orderLists = new List<OrderListViewModel>();
        public List<EventList> events = new List<EventList>();
        public List<PhonegramList> phonegrams = new List<PhonegramList>();

        // *** Взять список заявок ***
        public string GetApp(string id)
        {
            try
            {
                int day = Convert.ToInt32((id.Split(' '))[0]);
                int month = Convert.ToInt32((id.Split(' '))[1]);
                int year = Convert.ToInt32((id.Split(' '))[2]);
                DateTime date = new DateTime(year, month, day);
                string div = "<table class='zebra-table'>";
                // </ div > "
                orderLists = (from a in db.ApplicationTable
                              join b in db.NetRegions on a.Id_NetRegion equals b.Id
                              join c in db.AppStatus on a.Id_Status equals c.Id
                              where (a.Id_Status == 4 || a.Id_Status == 6) && (Convert.ToDateTime(a.StartReqTime.ToShortDateString()) <= date) && Convert.ToDateTime(a.EndReqTime.ToShortDateString()) >= date
                              select new OrderListViewModel
                              {
                                  Id = a.Id,
                                  NetRegionName = b.Name,
                                  SubstationName = a.ObjectName,
                                  StartDate = a.StartReqTime,
                                  EndDate = a.EndReqTime,
                                  OrderStatus = c.Name
                                  //DescriptionApplication = a.Description
                              }).ToList();

                foreach (var item in orderLists)
                {
                    div += "<tr><td><a href='/ApplicationsBasePage/" + item.Id + "' style='color:#000000; font-weight:bold; cursor:pointer;' title='" + item.DescriptionApplication + "'>" + item.NetRegionName + " - " + item.SubstationName + " (" + item.StartDate.ToString("HH:mm") + "-" + item.EndDate.ToString("HH:mm") + ") - " + item.OrderStatus + "</td></tr>";
                }
                div += "</table>";
                return JsonConvert.SerializeObject(div);
            }
            catch (Exception ex) { return JsonConvert.SerializeObject(ex.Message); }
        }

        // *** Взять список событий ***
        public string GetEvent(string id)
        {
            try
            {
                int day = Convert.ToInt32((id.Split(' '))[0]);
                int month = Convert.ToInt32((id.Split(' '))[1]);
                int year = Convert.ToInt32((id.Split(' '))[2]);
                DateTime date = new DateTime(year, month, day);
                string div = "<table class='zebra-table'>";
                // </ div > "
                events = (from a in db.EventTable
                          join b in db.Users on a.UserId equals b.Id
                          where Convert.ToDateTime(a.StartDate.ToShortDateString()) <= date && Convert.ToDateTime(a.EndDate.ToShortDateString()) >= date
                          select new EventList
                          {
                              Id = a.Id,
                              Title = a.Title,
                              Description = a.Description,
                              StartDate = a.StartDate,
                              EndDate = a.EndDate

                          }).ToList();

                foreach (var item in events)
                {
                    div += "<tr><td><a href='JavaScript:OpenInfoEventModal(" + item.Id + ")' style='color:#000000; font-weight:bold; cursor:pointer;' title='" + item.Description + "'>" + item.Title + " (" + item.StartDate.ToShortDateString() + " " + item.StartDate.ToShortTimeString() + ")</td></tr>";
                }
                div += "</table>";
                return JsonConvert.SerializeObject(div);
            }
            catch (Exception ex) { return JsonConvert.SerializeObject(ex.Message); }
        }

        // *** Взять список телефонограмм ***
        public string GetPhonegram(string id)
        {
            try
            {
                int day = Convert.ToInt32((id.Split(' '))[0]);
                int month = Convert.ToInt32((id.Split(' '))[1]);
                int year = Convert.ToInt32((id.Split(' '))[2]);
                DateTime date = new DateTime(year, month, day);
                string div = "<table class='zebra-table'>";
                // </ div > "
                phonegrams = (from a in db.PhonegramTable
                          join b in db.Users on a.FromUserId equals b.Id
                          where Convert.ToDateTime(a.Date.ToShortDateString()) == date && (a.ToUserId == db.Users.FirstOrDefault(s => s.Login == User.Identity.Name).Id || a.FromUserId == db.Users.FirstOrDefault(s => s.Login == User.Identity.Name).Id)
                          select new PhonegramList
                          {
                              Id = a.Id,
                              Theme = a.Theme,
                              Description = a.Description,
                              FromNameUser = Models.ShortNameModel.ShortName(db.Users.FirstOrDefault(s => s.Login == User.Identity.Name).Name)

                          }).ToList();

                foreach (var item in phonegrams)
                {
                    div += "<tr><td><a href='JavaScript:OpenPhonegramEventModal(" + item.Id + ")' style='color:#000000; font-weight:bold; cursor:pointer;' title='" + item.Description + "'>От: " + item.FromNameUser + ", Тема:" + item.Theme + " (Сообщение: " + item.Description + ")</td></tr>";
                }
                div += "</table>";
                return JsonConvert.SerializeObject(div);
            }
            catch (Exception ex) { return JsonConvert.SerializeObject(ex.Message); }
        }

        // *** Взять всю информацию о событии ***
        public string GetFullInfoForEvent(int id)
        {
            try
            {
                EventTable eventTable = db.EventTable.Find(id);
                string html = "<table class='app-table' style='width:100%; word-break: break-all; hyphens: auto;'><tr><th colspan='2'>" + eventTable.Title + "</th></tr>";
                html += "<tr><th style='width: 50px;'>ОРГАНИЗАТОР:</th><td style='width:50%;'>" + Models.ShortNameModel.ShortName(db.Users.Find(eventTable.UserId).Name) + "</td></tr>";
                html += "<tr><th style='width: 50px;'>НАЧАЛО:</th><td style='width:50%;'>" + eventTable.StartDate.ToShortDateString() + " " + eventTable.StartDate.ToShortTimeString() + "</td></tr>";
                html += "<tr><th style='width: 50px;'>КОНЕЦ:</th><td style='width:50%;'>" + eventTable.EndDate.ToShortDateString() + " " + eventTable.EndDate.ToShortTimeString() + "</td></tr>";
                html += "<tr><th style='width: 50px;'>ОПИСАНИЕ:</th><td style='width:50%;'>" + eventTable.Description + "</td></tr>";
                if (User.IsInRole("administrator") || db.Users.FirstOrDefault(s => s.Login == User.Identity.Name).Id == eventTable.UserId)
                {
                    html += "<tr><th colspan='2'><button class='btn btn-primary' style='margin: 2px;' onclick='DelEvent(" + eventTable.Id + ")'><i class='fa fa-trash'></i> Удалить событие</button></th></tr>";
                }
                html += "</table>";
                return JsonConvert.SerializeObject(html);
            }
            catch(Exception ex) { return JsonConvert.SerializeObject(ex.Message); }
        }

        public string GetFullInfoForPhonegram(int id)
        {
            try
            {
                PhonegramTable phonegram = db.PhonegramTable.Find(id);
                string html = "<table class='app-table' style='max-width:100%; word-break: break-all; hyphens: auto;'><tr><th colspan='2'>" + phonegram.Theme + "</th></tr>";
                html += "<tr><th style='width: 50px;'>ОРГАНИЗАТОР:</th><td style='width:50%;'>" + Models.ShortNameModel.ShortName(db.Users.Find(phonegram.FromUserId).Name) + "</td></tr>";
                html += "<tr><th style='width: 50px;'>ОРГАНИЗАТОР:</th><td style='width:50%;'>" + Models.ShortNameModel.ShortName(db.Users.Find(phonegram.ToUserId).Name) + "</td></tr>";
                html += "<tr><th style='width: 50px;'>ОРГАНИЗАТОР:</th><td style='width:50%;'>" + phonegram.Description + "</td></tr>";
                if (User.IsInRole("administrator") || db.Users.FirstOrDefault(s => s.Login == User.Identity.Name).Id == phonegram.FromUserId)
                {
                    html += "<tr><th colspan='2'><button class='btn btn-primary' style='margin: 2px;' onclick='DelPhonegram(" + phonegram.Id + ")'><i class='fa fa-trash'></i> Удалить телефонограмму</button></th></tr>";
                }
                html += "</table>";
                return JsonConvert.SerializeObject(html);
            }
            catch (Exception ex) { return JsonConvert.SerializeObject(ex.Message); }
        }

        [Authorize]
        // *** Взять календарь ***
        public string GetCalendar(string id)
        {
            try
            {
                int startMon = Convert.ToInt32((id.Split(' '))[0]);
                int month = Convert.ToInt32((id.Split(' '))[1]);
                int year = Convert.ToInt32((id.Split(' '))[2]);

                DateTime dateNow = DateTime.Now;
                DateTime d = new DateTime(year, month, 1);
                int mon = month - 1;
                string table = "<table id='id_cal' class='calendar_style' align='center'><tr><th align='center'>пн</th><th align='center'>вт</th><th align='center'>ср</th><th align='center'>чт</th><th align='center'>пт</th><th align='center'>сб</th><th align='center'>вс</th></tr><tr>";
                for (int i = 0; i < startMon; i++)
                {
                    table += "<td align='center'></td>";
                }

                int wd = 7 - startMon;
                int colDay = DateTime.DaysInMonth(year, month);
                int colWeek = ColWeek(startMon, colDay);


                for (int i = 1; i <= colDay; i++)
                {
                    table += "<td";
                    if (colWeek == 4) table += " class='row-four' ";
                    else if (colWeek == 5) table += " class='row-five' ";
                    else table += " class='row-six' ";

                    if (i == DateTime.Now.Day && month == DateTime.Now.Month && year == DateTime.Now.Year)
                    {
                        table += " align='left' style='vertical-align: top; background: #ffd220; opacity: 0.7; color:#000000;'>" + i;
                    }
                    else
                    {
                        table += " align='left' style='vertical-align: top; color:#000000;'>" + i;

                    }
                    int col = ColApp(i, month, year);
                    switch (col)
                    {
                        case 0:
                            break;
                        case 1:
                            table += "<div class='app-body' onclick='GetApp(" + i + ")'>" + col + " ЗАЯВКА</div></div>";
                            break;
                        case 2:
                            table += "<div class='app-body' onclick='GetApp(" + i + ")'>" + col + " ЗАЯВКИ</div></div>";
                            break;
                        case 3:
                            table += "<div class='app-body' onclick='GetApp(" + i + ")'>" + col + " ЗАЯВКИ</div></div>";
                            break;
                        case 4:
                            table += "<div class='app-body' onclick='GetApp(" + i + ")'>" + col + " ЗАЯВКИ</div></div>";
                            break;
                        default:
                            table += "<div class='app-body' onclick='GetApp(" + i + ")'>" + col + " ЗАЯВОК</div></div>";
                            break;

                    }

                    col = ColEvent(i, month, year);
                    switch (col)
                    {
                        case 0:
                            break;
                        case 1:
                            table += "<div class='event-body' onclick='GetEvent(" + i + ")'>" + col + " СОБЫТИЕ</div></div>";
                            break;
                        case 2:
                            table += "<div class='event-body' onclick='GetEvent(" + i + ")'>" + col + " СОБЫТИЯ</div></div>";
                            break;
                        case 3:
                            table += "<div class='event-body' onclick='GetEvent(" + i + ")'>" + col + " СОБЫТИЯ</div></div>";
                            break;
                        case 4:
                            table += "<div class='event-body' onclick='GetEvent(" + i + ")'>" + col + " СОБЫТИЯ</div></div>";
                            break;
                        default:
                            table += "<div class='event-body' onclick='GetEvent(" + i + ")'>" + col + " СОБЫТИЙ</div></div>";
                            break;
                    }

                    col = ColPhonegram(i, month, year);
                    switch (col)
                    {
                        case 0:
                            break;
                        case 1:
                            table += "<div class='phonegram-body' onclick='GetPhonegram(" + i + ")'>" + col + " ТЕЛЕФОНОГРАММА</div></div>";
                            break;
                        case 2:
                            table += "<div class='phonegram-body' onclick='GetPhonegram(" + i + ")'>" + col + " ТЕЛЕФОНОГРАММЫ</div></div>";
                            break;
                        case 3:
                            table += "<div class='phonegram-body' onclick='GetPhonegram(" + i + ")'>" + col + " ТЕЛЕФОНОГРАММЫ</div></div>";
                            break;
                        case 4:
                            table += "<div class='phonegram-body' onclick='GetPhonegram(" + i + ")'>" + col + " ТЕЛЕФОНОГРАММ</div></div>";
                            break;
                        default:
                            table += "<div class='phonegram-body' onclick='GetPhonegram(" + i + ")'>" + col + " ТЕЛЕФОНОГРАММ</div></div>";
                            break;
                    }

                    table += "</td>";
                    wd--;
                    if (wd == 0 && i != colDay)
                    {
                        table += "</tr><tr>";
                        wd = 7;
                    }
                }

                while (wd != 0)
                {
                    table += "<td></td>";
                    wd--;
                }
                table += "</tr></table>";
                return JsonConvert.SerializeObject(table);
            }
            catch (Exception ex) { return JsonConvert.SerializeObject(ex.Message); }
        }

        public async Task<string> AddEventAsync(string title, DateTime startDate, DateTime endDate, string description)
        {
            EventTable eventTable = new EventTable();
            eventTable.Title = title;
            eventTable.StartDate = startDate;
            eventTable.EndDate = endDate;
            eventTable.Description = description;
            eventTable.UserId = db.Users.SingleOrDefault(s => s.Login == User.Identity.Name).Id;
            await Models.EventHistoryModel.AddEvent("Создал событие", User.Identity.Name);
            return JsonConvert.SerializeObject(eventTable.AddEvent());
        }

        public async Task<string> DelEventAsync(int Id)
        {
            EventTable eventTable = db.EventTable.Find(Id);
            await Models.EventHistoryModel.AddEvent("Удалил событие", User.Identity.Name);
            return JsonConvert.SerializeObject(eventTable.DelEvent());
        }

        // *** Удалить телефонограмму ***
        public async Task<string> DelPhonegramAsync(int id)
        {
            try
            {
                PhonegramTable phonegram = db.PhonegramTable.Find(id);
                await Models.EventHistoryModel.AddEvent("Удалил телефонограмму", User.Identity.Name);
                return JsonConvert.SerializeObject(phonegram.DelPhonegram());
            }
            catch(Exception ex) { return JsonConvert.SerializeObject(ex.Message); }
        }

        // *** Создание телефонограммы ***
        public async Task<string> CreatePhonegram(int userId, DateTime date, string tema, string text)
        {
            return JsonConvert.SerializeObject(new PhonegramTable()
            {
                FromUserId = db.Users.FirstOrDefault(s => s.Login == User.Identity.Name).Id,
                ToUserId = userId,
                Theme = tema,
                Description = text,
                Date = date
            }.CreatePhonegram());
        }
        // *** Удаление телефонограммы ***
        public async Task<string> DelPhonegram(int id)
        {
            return JsonConvert.SerializeObject(db.PhonegramTable.Find(id).DelPhonegram());
        }

        public IActionResult Index()
        {
            return View();
        }

    }
}
