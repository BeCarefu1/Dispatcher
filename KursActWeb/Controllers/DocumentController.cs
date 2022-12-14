using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
//using DinkToPdf;
//using Syncfusion;
//using Syncfusion.Pdf;
//using Syncfusion.HtmlConverter;
//using Syncfusion.Pdf.Graphics;
//using Syncfusion.Drawing;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using GIPManager;
using ActGIPelectroWeb.ViewModels;
using Microsoft.AspNetCore.Http;
using System.Threading;

namespace ActGIPelectroWeb.Controllers
{
    public class DocumentController : Controller
    {
        StoreContext db = new StoreContext();
        IHostingEnvironment _env;

        public class DispatcherListViewModal
        {
            public int Id_Dispatcher { get; set; }
            public string Name_Dispatcher { get; set; }
        }

        public List<DispatcherListViewModal> DispatcherList = new List<DispatcherListViewModal>();
        public List<DutyScheduleViewModel> DateTimeList = new List<DutyScheduleViewModel>();
        public List<DutyScheduleViewModel> DutyList = new List<DutyScheduleViewModel>();

        public DocumentController(IHostingEnvironment env = null)
        {
            _env = env;
        }


        public IActionResult Index()
        {
            return View();
        }

        void CreateData()
        {

            DispatcherList = (from a in db.Users
                              where a.RoleId == 14
                              select new DispatcherListViewModal()
                              {
                                  Id_Dispatcher = a.Id,
                                  Name_Dispatcher = a.Name
                              }).ToList();

            //Список времени

            int flag;
            bool check = false;
            DateTime sdate;
            DateTime edate;

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
                            orderby a.StartDate ascending
                            select new DutyScheduleViewModel
                            {
                                Id_dispatcher = a.Id_dispatcher,
                                Color = a.Color,
                                StartDate = a.StartDate,
                                EndDate = a.EndDate
                            }).ToList();
            }

                //if (DutyList.Count() == 0) DutyList.Add(new DutyScheduleViewModel() { Id = 0, Id_dispatcher = 0 });

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

               

                //if (db.DutyScheduleTable.Count() != 0)
                //{
                //    if (db.DutyScheduleTable.FirstOrDefault(s => s.StartDate == sdate && s.EndDate == edate) == null)
                //        //DutyList.Add(new DutyScheduleViewModel() { Id = i, Id_dispatcher = 0 });
                //    //else
                //        //DutyList.Add(new DutyScheduleViewModel() { Id = i, Id_dispatcher = db.DutyScheduleTable.FirstOrDefault(s => s.StartDate == sdate && s.EndDate == edate).Id_dispatcher });
                //}
                //else
                //    //DutyList.Add(new DutyScheduleViewModel() { Id = i, Id_dispatcher = 0 });
                DateTimeList.Add(new DutyScheduleViewModel() { Id = i, StartDate = sdate, EndDate = edate, Check = check });
            }
        }

        public string GetFIO()
        {
            try
            {
                string name;// = db.Users.FirstOrDefault(s => s.Login == User.Identity.Name).Name;
                string[] arr = (db.Users.FirstOrDefault(s => s.Login == User.Identity.Name).Name).Split(' ');
                name = arr[0] + " ";
                for (int i = 1; i < arr.Length; i++)
                {
                    name += (arr[i])[0] + ".";
                }
                return name;
            }
            catch (Exception ex) { return ex.Message; }

        }

        public string GetRole()
        {
            if(User.IsInRole("chief_dispatcher"))
            {
                return "Главный диспетчер";
            }
            else
            {
                return "Администратор";
            }

        }
        [Authorize]

        public async Task AddComment(string comment)
        {
            OperationalLog operational = new OperationalLog();
            operational.UserId = db.Users.FirstOrDefault(s => s.Login == User.Identity.Name).Id;
            operational.RecoilTime = DateTime.Now;
            operational.Description = comment;
            operational.Numb = db.OperationalLog.LastOrDefault().Numb + 1;
            operational.AddCommentAsync();
        }

        [Authorize]
        public FileResult GetFile(int id)
        {
            var doc = db.Documents.Find(id);
            if (doc == null) return null;
            return File(doc.Doc, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", doc.Name);
        }

        public async Task<string> AddFile(IFormFile file)
        {
            if (file != null)
            {
                Documents doc = new Documents { Name = file.FileName, Date = DateTime.Now, UserId = db.Users.SingleOrDefault(s => s.Login == User.Identity.Name).Id };
                byte[] files = null;
                using (var binaryReader = new BinaryReader(file.OpenReadStream()))
                {
                    files = binaryReader.ReadBytes((int)file.Length);
                }
                doc.Doc = files;
                await AddComment("Добавлен документ: " + doc.Name);
                await Models.EventHistoryModel.AddEvent("Добавлен документ: " + doc.Name, User.Identity.Name);
                return doc.AddDoc();
            }
            else
            {
                return "Error: Не удалось загрузить файл!";
            }
            
        }

        public async Task<string> AddOperationalScheme(IFormFile file)
        {
            if (file != null)
            {
                OperationalScheme scheme = new OperationalScheme { Name = file.FileName, Date = DateTime.Now, UserId = db.Users.SingleOrDefault(s => s.Login == User.Identity.Name).Id };
                byte[] files = null;
                using (var binaryReader = new BinaryReader(file.OpenReadStream()))
                {
                    files = binaryReader.ReadBytes((int)file.Length);
                }
                scheme.Doc = files;
                await AddComment("Добавлена оперативная схема: " + scheme.Name);
                await Models.EventHistoryModel.AddEvent("Добавлена оперативная схема: " + scheme.Name, User.Identity.Name);

                return scheme.AddOperationalScheme(); 
            }
            else
            {
                return "Error: Не удалось загрузить файл!";
            }
        }

        public FileResult GetOperationalScheme()
        {
            var scheme = db.OperationalScheme.LastOrDefault();
            if (scheme == null) return null;
            return File(scheme.Doc, "application/vnd.visio", scheme.Name);
        }

        public async Task<string> DelFile (int id)
        {
            try
            {
                Documents doc = db.Documents.Find(id);
                await AddComment("Документ удален: " + doc.Name);
                await Models.EventHistoryModel.AddEvent("Документ удален: " + doc.Name, User.Identity.Name);
                return doc.DelDoc();
            }
            catch (Exception ex) { return ex.Message; }
        }

    }
}
