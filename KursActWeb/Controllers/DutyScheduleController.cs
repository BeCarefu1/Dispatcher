using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GIPManager;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using ActGIPelectroWeb.ViewModels;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using ActGIPelectroWeb.Models;
using Spire.Xls;

namespace ActGIPelectroWeb.Controllers
{
    public class DutyScheduleController : Controller
    {
        StoreContext db;
        IHostingEnvironment _appEnvironment;
        public DutyScheduleController(StoreContext context, IHostingEnvironment appEnvironment = null)
        {
            db = context;
            _appEnvironment = appEnvironment;
        }


        // GET: DutyScheduleController
        public ActionResult Index()
        {
            return View();
        }

        // GET: DutyScheduleController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: DutyScheduleController/Create
        public ActionResult Create()
        {
            return View();
        }

        //
        [Authorize]
        public string GetMonth()
        {
            string html = "<table><tr><td><img src = '~/images/vector-left.png; width = '20' height = '20' /></td><td><h3 id = 'id_month' style = 'color:#000000; margin:0px 10px 0px 10px;' ></ h3 ></td><td><img src = '~/images/vector-right.png' width = '20' height = '20' /></td></tr></table> ";
            return JsonConvert.SerializeObject(html);
        }


        




        [HttpPost]
        public string DeleteDuty(int id_dispatcher, int col)
        {
            DutyScheduleTable dutyScheduleTable = new DutyScheduleTable();
            int flag = 13;
            DateTime sDate = Convert.ToDateTime(DateTime.Now.Day + "." + DateTime.Now.Month + "." + DateTime.Now.Year + " 08:00");
            DateTime eDate = Convert.ToDateTime(DateTime.Now.Day + "." + DateTime.Now.Month + "." + DateTime.Now.Year + " 19:00");
            for (int i = 0; i < col; i++)
            {
                sDate = eDate;
                if (flag == 13)
                {
                    eDate = eDate.AddHours(flag);
                    flag = 11;
                }
                else
                {
                    eDate = eDate.AddHours(flag);
                    flag = 13;
                }
            }
            dutyScheduleTable = db.DutyScheduleTable.FirstOrDefault(s => s.Id_dispatcher == id_dispatcher && s.StartDate == sDate && s.EndDate == eDate);
            dutyScheduleTable.DeleteDuty();
            return "";
        }

        public class FileList
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public byte[] ExFile { get; set; }
        }

        public async Task AddComment()
        {
            OperationalLog operational = new OperationalLog();
            operational.UserId = db.Users.FirstOrDefault(s => s.Login == User.Identity.Name).Id;
            operational.RecoilTime = DateTime.Now;
            operational.Description = "Обновлен график дежурств";
            operational.Numb = db.OperationalLog.LastOrDefault().Numb + 1;
            operational.AddCommentAsync();
        }

        [HttpPost]
        public async Task<string> AddFile(IFormFile file)
        {
            if (db.ExcelFileTable.FirstOrDefault() == null)
            {
                if (file != null)
                {
                    ExcelFileTable exFile = new ExcelFileTable { Name = file.FileName, Date = DateTime.Now, UserId = db.Users.SingleOrDefault(s => s.Login == User.Identity.Name).Id };
                    byte[] files = null;
                    using (var binaryReader = new BinaryReader(file.OpenReadStream()))
                    {
                        files = binaryReader.ReadBytes((int)file.Length);
                    }
                    exFile.ExFile = files;

                    //Workbook workbook = new Workbook();
                    ////Load an Excel sample document
                    //
                    //workbook.LoadFromFile(file.ContentDisposition);
                    //
                    ////Get the first worksheet of the document
                    //
                    //Worksheet sheet = workbook.Worksheets[0];
                    ////Save the worksheet to HTML
                    ////MemoryStream m = new MemoryStream();
                    ////workbook.SaveToStream(m);
                    //CellRange range = sheet.Range[1, 1, 3, 2];
                    //sheet.SaveToHtml("ExcelToHTML.html");
                    //exFile.HtmlFile = sheet.ExportDataTable(range, true, true);
                    await AddComment();
                    await Models.EventHistoryModel.AddEvent("Обновлен график дежурств", User.Identity.Name);
                    return exFile.AddExFile();
                }
                else
                {
                    return "Ошибка! Не удалось загрузить файл!";
                }
            }
            else
            {
                if (file != null)
                {
                    ExcelFileTable exFile = db.ExcelFileTable.FirstOrDefault();
                    exFile.Name = file.FileName;
                    exFile.Date = DateTime.Now;
                    exFile.UserId = db.Users.SingleOrDefault(s => s.Login == User.Identity.Name).Id;
                    byte[] files = null;
                    using (var binaryReader = new BinaryReader(file.OpenReadStream()))
                    {
                        files = binaryReader.ReadBytes((int)file.Length);
                    }
                    exFile.ExFile = files;
                    await AddComment();
                    await Models.EventHistoryModel.AddEvent("Обновлен график дежурств", User.Identity.Name);
                    return exFile.EditExFile();
                    
                }
                else
                {
                    return "Ошибка! Не удалось загрузить файл!";
                }
            }
        }

        [Authorize]
        public FileResult GetExcelFile()
        {
            var excelFile = db.ExcelFileTable.FirstOrDefault();
            if (excelFile == null) return null;
            return File(excelFile.ExFile, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelFile.Name);

        }



        public string AddDuty(int id_dispatcher, int col)
        {
            DutyScheduleTable dutyScheduleTable = new DutyScheduleTable();
            int flag = 13;
            DateTime sDate = Convert.ToDateTime(DateTime.Now.Day + "." + DateTime.Now.Month + "." + DateTime.Now.Year + " 08:00");
            DateTime eDate = Convert.ToDateTime(DateTime.Now.Day + "." + DateTime.Now.Month + "." + DateTime.Now.Year + " 19:00");
            string color;
            for(int i = 0; i < col; i++)
            {
                sDate = eDate;
                if (flag == 13)
                {
                    eDate = eDate.AddHours(flag);
                    flag = 11;
                }
                else
                {
                    eDate = eDate.AddHours(flag);
                    flag = 13;
                }
            }
            DateTime checkDate = new DateTime(sDate.Year, sDate.Month, sDate.Day, 8, 0, 0);
            if (sDate == checkDate)
            {
                color = "#FFFF00";
            }
            else
            {
                color = "#0033CC";
            }
                
            dutyScheduleTable.AddDuty(id_dispatcher, sDate, eDate, color);
            //return "";
            return color;
        }

        public string GetDutyTable()
        {
            string html = "<table class='new- table' id='myTable'><tr><td align='center' style = 'background: #FFFFFF' colspan = '@Model.DateTimeList.Count()++'><div id='id_div_month'></div><table><tr>";



            return JsonConvert.SerializeObject(html);
        }

        // POST: DutyScheduleController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]


        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: DutyScheduleController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: DutyScheduleController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: DutyScheduleController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: DutyScheduleController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
