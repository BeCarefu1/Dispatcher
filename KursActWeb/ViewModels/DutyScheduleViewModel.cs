using GIPManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ActGIPelectroWeb.ViewModels
{
    public class DutyScheduleViewModel
    {
        public int Id { get; set; }
        public int Id_dispatcher { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool Check { get; set; }
        public string Color { get; set; }
        public int Id_Dispatcher { get; set; }
        public string Name_Dispatcher { get; set; }
        //public string Period { get; set; }

        public List<DutyScheduleViewModel> DateTimeList { get; set; }
        public List<DutyScheduleViewModel> DutyList { get; set; }
        public List<DutyScheduleViewModel> DispatcherList { get; set; }

        /*public DutyScheduleViewModel()
        {
            using (StoreContext db = new StoreContext())
            {
                //Список диспетчеров
                DispatcherList = (from a in db.Users
                                  where a.RoleId == 14
                                  select new DutyScheduleViewModel()
                                  {
                                      Id_Dispatcher = a.Id,
                                      Name_Dispatcher = a.Name
                                  }).ToList();
                //Список времени
                DateTimeList = new List<DutyScheduleViewModel>();
                DutyList = new List<DutyScheduleViewModel>();
                int flag;
                bool check = false;
                DateTime sdate;
                DateTime edate;

                //




                if (DateTime.Now >= Convert.ToDateTime(DateTime.Now.Day + "." + DateTime.Now.Month + "." + DateTime.Now.Year + " 19:00") && DateTime.Now < Convert.ToDateTime(DateTime.Now.Day + "." + DateTime.Now.Month + "." + DateTime.Now.Year + " 08:00").AddDays(1))
                {
                    flag = 11;
                    sdate = Convert.ToDateTime(DateTime.Now.Day + "." + DateTime.Now.Month + "." + DateTime.Now.Year + " 19:00");
                    edate = Convert.ToDateTime(DateTime.Now.Day + "." + DateTime.Now.Month + "." + DateTime.Now.Year + " 08:00").AddDays(1);
                }
                else
                {
                    flag = 13;
                    sdate = Convert.ToDateTime(DateTime.Now.Day + "." + DateTime.Now.Month + "." + DateTime.Now.Year + " 08:00");
                    edate = Convert.ToDateTime(DateTime.Now.Day + "." + DateTime.Now.Month + "." + DateTime.Now.Year + " 19:00");
                }

                if (db.DutyScheduleTable.Count() == 0)
                    DutyList.Add(new DutyScheduleViewModel() { Id = 0, Id_dispatcher = 0 });
                else
                {
                    DutyList = (from a in db.DutyScheduleTable
                                where (a.StartDate < DateTime.Now && a.EndDate > DateTime.Now) || (DateTime.Now < a.StartDate)
                                select new DutyScheduleViewModel
                                {
                                    Id_dispatcher = a.Id_dispatcher,
                                    Color = a.Color
                                }).ToList();
                }

                if (DutyList.Count() == 0) DutyList.Add(new DutyScheduleViewModel() { Id = 0, Id_dispatcher = 0 });

                DateTimeList.Add(new DutyScheduleViewModel() { Id = 0, StartDate = sdate, EndDate = edate, Check = false });

                for (int i = 1; i < 14; i++)
                {
                    sdate = DateTimeList[i - 1].EndDate;
                    if (flag == 13)
                    {
                        edate = sdate.AddHours(flag);
                        flag = 11;
                        check = true;
                    }
                    else
                    {
                        edate = sdate.AddHours(flag);
                        flag = 13;
                        check = false;
                    }
                    if (db.DutyScheduleTable.Count() != 0)
                    {
                        if (db.DutyScheduleTable.FirstOrDefault(s => s.StartDate == sdate && s.EndDate == edate) == null)
                            DutyList.Add(new DutyScheduleViewModel() { Id = i, Id_dispatcher = 0 });
                        else
                            DutyList.Add(new DutyScheduleViewModel() { Id = i, Id_dispatcher = db.DutyScheduleTable.FirstOrDefault(s => s.StartDate == sdate && s.EndDate == edate).Id_dispatcher });
                    }
                    else
                        DutyList.Add(new DutyScheduleViewModel() { Id = i, Id_dispatcher = 0 });
                    DateTimeList.Add(new DutyScheduleViewModel() { Id = i, StartDate = sdate, EndDate = edate, Check = check });
                }
            }
        }*/
    }
}
