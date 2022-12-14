using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GIPManager;
using ActGIPelectroWeb.Models;
using ActGIPelectroWeb.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StoreGIPManager.RegPoint;
using StoreGIPManager.Worker;

namespace ActGIPelectroWeb.Controllers
{
    public class PaymentReportController : Controller
    {
        StoreContext db;
        public PaymentReportController(StoreContext context)
        {
            db = context;
        }

        /// <summary>
        /// Создает открытый для редактирования отчет на оплату СМР или ПНР ПУ
        /// </summary>
        /// <param name="workerId">id работника</param>
        /// <param name="month">Номер месяца отчета</param>
        /// <param name="day">1 или 2 половина месяца (принимает числа 1 или 15)</param>
        /// <returns>Возвращает Id созданного отчета</returns>
        [Authorize]
        [HttpPost]
        public string Create([FromBody] JObject data)
        {
            
            PaymentReport report = new PaymentReport()
            {
                DateCreate = DateTime.Now,
                IsClosed = false,
                WorkerId = data["workerId"].ToObject<int>(),
                DatePeriodStart = new DateTime(data["year"].ToObject<int>(), data["month"].ToObject<int>(), data["day"].ToObject<int>())
            };
            var existReport = db.PaymentReports.FirstOrDefault(er => er.DatePeriodStart == report.DatePeriodStart && er.WorkerId == report.WorkerId);
            if(existReport == null)
            {
                db.PaymentReports.Add(report);
                db.SaveChanges();
                return report.Id.ToString();
            }
            return existReport.Id.ToString();
        }

        /// <summary> Удаление отчета на оплату СМР </summary>
        /// <param name="id">Id отчета на оплату, который нужно удалить</param>
        [Authorize]
        [HttpPost]
        public void Delete(int id)
        {
            var payReport = db.PaymentReports.Find(id);
            //Если акт закрыт, то его нельзя удалять или редактировать
            if(payReport != null && !payReport.IsClosed)
            {
                db.PaymentReports.Remove(payReport);
                db.SaveChanges();
                return;
            }
            throw new Exception("Закрытый отчет удалить нельзя");
        }

        /// <summary> Закрыть отчет </summary>
        /// <param name="id"> Id отчета на оплату, который нужно закрыть </param>
        [Authorize]
        [HttpPost]
        public void Close(int id)
        {
            var payReport = db.PaymentReports.Find(id);
            payReport.IsClosed = true;
            db.SaveChanges();
        }

        /// <summary> Открыть отчет </summary>
        /// <param name="id"> Id отчета на оплату, который нужно открыть </param>
        [Authorize]
        [HttpPost]
        public void Open(int id)
        {
            var payReport = db.PaymentReports.Find(id);
            payReport.IsClosed = false;
            db.SaveChanges();
        }
        /// <summary> Удаление элемета отчета </summary>
        /// <param name="id">Id элемента отчета, который нужно удалить </param>
        [Authorize]
        [HttpPost]
        public void RemovePoint(int reportId, int regPointId)
        {
            var reportPoint = db.PaymentReportRegPoints.FirstOrDefault(p => p.PaymentReportId == reportId && p.RegPointId == regPointId);
            db.PaymentReportRegPoints.Remove(reportPoint);
            db.SaveChanges();
        }
        /// <summary>
        /// Получить список точек учета привязанных к отчету
        /// </summary>
        /// <param name="id">Id отчета на оплату</param>
        /// <returns></returns>
        [Authorize]
        [HttpGet]
        public string GetPoints_Json(int id)
        {
            var pointsInReport = new PaymentReportsManager(db).GetPointsInReport(id);
            return JsonConvert.SerializeObject(pointsInReport);
        }
        /// <summary>
        /// Получить список точек учета привязанных к отчету сгруппированых по типу устройства (1ф PLC, 3ф GSM и тд
        /// </summary>
        /// <param name="id">Id отчета на оплату</param>
        /// <returns></returns>
        [Authorize]
        [HttpGet]
        public string GetPointsGroups_Json(int id)
        {
            var pointsInReport = new PaymentReportsManager(db).GetPointsInReport(id)
                .GroupBy(p => p.DeviceDescription)
                .Select(g => new {
                    TypeDevice = g.Key.Replace(" ", string.Empty),  //Название группы (тип ПУ)
                    Points = g.ToList(),//Массив точек учета со всей инфой
                    Count = g.Count(),  //Всего точек в этой группе
                    ToAddCount = 0,     //Количество которое нужно добавить в отчет из этой группы
                    WorkType = 0
                });
            return JsonConvert.SerializeObject(pointsInReport);
        }

        /// <summary>
        /// Получить список точек доступных для добавления в отчет на оплату
        /// </summary>
        /// <param name="id">Id работника</param>
        /// <returns></returns>
        [Authorize]
        [HttpGet]
        public string GetWorkerAvailablePoints_Json(int id)
        {
            var aviablePoints = new PaymentReportsManager(db).GetAvailableWorkerRegPointsInfo(id)
                .GroupBy(p => p.DeviceDescription)
                .Select(g => new {
                    TypeDevice = g.Key.Replace(" ", string.Empty),  //Название группы (тип ПУ)
                    Points = g.ToList(),//Массив точек учета со всей инфой
                    Count = g.Count(),  //Всего точек в этой группе
                    ToAddCount = 0,     //Количество которое нужно добавить в отчет из этой группы
                    WorkType = 0
                });
            return JsonConvert.SerializeObject(aviablePoints);
        }

        [Authorize]
        [HttpPost]
        public void AttachPoints([FromBody] JObject data)
        {
            var myType = new { Id = 0, Points = new List<RegPointInfo>(), CostRUB = 0, WorkType = 0 };
            string jsonData = data.ToString();
            var _data = JsonConvert.DeserializeAnonymousType(data.ToString(), myType);

            var report = db.PaymentReports.Find(_data.Id);
            var points = _data.Points;
            if(report != null)
            {
                points.ForEach(p =>
                report.PaymentReportRegPoints
                .Add(new PaymentReportRegPoint()
                {
                    CostRUB = _data.CostRUB,
                    WorkType = (PaymentReportWorkType)_data.WorkType,
                    RegPointId = p.RegPointId
                }));
            }
            db.SaveChanges();
        }
        

    }
}