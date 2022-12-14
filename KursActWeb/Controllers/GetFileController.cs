using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GIPManager;
using System.IO;
using ActGIPelectroWeb.Models;
using Microsoft.AspNetCore.Hosting;
using System.Globalization;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;

namespace ActGIPelectroWeb.Controllers
{
    public class GetFileController : Controller
    {
        StoreContext db;
        readonly IHostingEnvironment _env;
        public GetFileController(StoreContext context, IHostingEnvironment env)
        {
            db = context;
            _env = env;
        }

        [Authorize]
        public FileResult GetExcel_PointsInSubstation(int? Id)
        {
            var substation = db.Substations.Find(Id);
            if (substation == null) return null;
            ExcelManager em = new ExcelManager();
            return File(em.PointsInSubstationPNR(substation), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", substation.Name + "_" + DateTime.Now.ToShortDateString() + ".xlsx");
        }

        [Authorize]
        public FileResult GetExcel_ConsumersInESKBFile(int? Id)
        {
            var substation = db.Substations.Find(Id);
            if (substation == null) return null;
            ExcelManager em = new ExcelManager();
            return File(em.ConsumersInESKBFile(substation), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", substation.Name + "_с_договорами_" + DateTime.Now.ToShortDateString() + ".xlsx");
        }

        [Authorize]
        public FileResult GetExcel_NoConsumersInESKBFile(int? Id)
        {
            var substation = db.Substations.Find(Id);
            if (substation == null) return null;
            ExcelManager em = new ExcelManager();
            return File(em.NoConsumersInESKBFile(substation), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", substation.Name + "_без_договоров_" + DateTime.Now.ToShortDateString() + ".xlsx");
        }

        //Скачать набор актов допуска на дату одним файлом
        [Authorize]
        public FileResult GetExcel_Act(int Id)
        {
            ExcelManager em = new ExcelManager(_env);
            return File(em.Act(Id).Data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Акт допуска " + Id.ToString() + ".xlsx");
        }

        //Скачать набор актов допуска на дату одним файлом
        [Authorize]
        public FileResult GetExcel_Acts(string inviteDate)
        {
            ExcelManager em = new ExcelManager(_env);
            var date = DateTime.ParseExact(inviteDate, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            var file = em.Acts(date);
            return File(file.Data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", file.Name);
        }

        //Скачать набор писем на дату одним файлом
        [Authorize]
        public FileResult GetExcel_Letters(string inviteDate)
        {
            ExcelManager em = new ExcelManager(_env);
            var date = DateTime.ParseExact(inviteDate, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            var file = em.Letters(date);
            return File(file.Data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",file.Name );
        }

        //Скачать одно письмо приглашение
        [Authorize]
        public FileResult GetExcel_Letter(int id)
        {
            ExcelManager em = new ExcelManager(_env);
            var file = em.Letter(id);
            return File(file.Data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", file.Name);
        }

        [Authorize]
        public FileResult GetExcel_LettersReestrToPostSite(string inviteDate)
        {
            ExcelManager em = new ExcelManager(_env);
            var date = DateTime.ParseExact(inviteDate, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            var file = em.LettersReestrToPostSite(date);
            return File(file.Data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", file.Name);
        }

        [Authorize]
        public FileResult GetExcel_RequestReestrToInsertPu(string inviteDate)
        {
            ExcelManager em = new ExcelManager(_env);
            var date = DateTime.ParseExact(inviteDate, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            var file = em.RequestReestrToInsertPu(date);
            return File(file.Data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", file.Name);
        }

        [Authorize]
        public FileResult GetExcel_TransferredReestrActsAdmission(string inviteDate)
        {
            ExcelManager em = new ExcelManager(_env);
            var date = DateTime.ParseExact(inviteDate, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            var file = em.TransferredReestrActsAdmission(date);
            return File(file.Data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", file.Name);
        }

        [Authorize]
        public FileResult GetExcel_PaymentReport(int id)
        {
            ExcelManager em = new ExcelManager(_env);
            var file = em.PaymentReport(id);
            return File(file.Data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", file.Name);
        }

        [Authorize]
        public FileResult GetExcel_UspdExport(int id)
        {
            ExcelManager em = new ExcelManager(_env);
            var file = em.ExportReportForUSPD(id);
            return File(file.Data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", file.Name);
        }

        [Authorize]
        public FileResult GetExcel_DutySchedule()
        {
            ExcelManager em = new ExcelManager(_env);
            var file = em.ExportDutySchedule();
            return File(file.Data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", file.Name);
        }

        [Authorize]
        public string SetExcel_DutySchedule(string path)
        {
            //ExcelManager em = new ExcelManager(_env);
            //var f = em.SetDutySchedule(path);
            //byte file = Convert.ToByte(f);
            byte[] file = System.IO.File.ReadAllBytes(path);
            return JsonConvert.SerializeObject("");
        }

        [HttpPost]
        public void DutySchedulePage(IFormFile uploadFile)
        {
            if (ModelState.IsValid && uploadFile != null)
            {
                byte[] imageData = null;
                // считываем переданный файл в массив байтов
                //using (var binaryReader = new BinaryReader(uploadImage.InputStream))
                //{
                //    imageData = binaryReader.ReadBytes(uploadImage.ContentLength);
                //}
                //// установка массива байтов
                //pic.Image = imageData;
                //
                //db.Pictures.Add(pic);
                //db.SaveChanges();
                //return RedirectToAction("Index");
            }
        }

    }
}