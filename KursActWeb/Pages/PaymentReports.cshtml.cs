using System;
using System.Collections.Generic;
using System.Linq;
using GIPManager;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StoreGIPManager.RegPoint;
using StoreGIPManager.Worker;

namespace ActGIPelectroWeb.Pages
{
    public class PaymentReportsModel : PageModel
    {
        readonly StoreContext db;
        public PaymentReportsModel(StoreContext context)
        {
            db = context;
        }

        public List<Worker> Workers { get; set; }
        public List<WorkerRegPoints> WorkerDevices { get; set; }

        public void OnGet()
        {
            int workerTypeId = db.WorkerTypes.FirstOrDefault(wt => wt.Description == WorkerTypeName.Mounter.ToString()).Id;
            Workers = (from w in db.Workers
                       where w.WorkerTypeId == workerTypeId
                       select w).ToList();

            WorkerDevices = new List<WorkerRegPoints>();
            PaymentReportsManager prManager = new PaymentReportsManager(db);
            Workers.ForEach(w =>
            {
                WorkerDevices.Add(
                    new WorkerRegPoints(
                        w.Id,
                        w.Name + " " + w.MIddlename + " " + w.Surname,
                        prManager.GetWorkerRegPointsInfo(w.Id),
                        prManager.GetWorkerPaymentReports(w.Id)
                        ));
            });
        }

        public class WorkerRegPoints
        {
            public int WorkerId { get; set; }
            public string Name { get; set; }
            public List<RegPointInfo> Points { get; set; }
            public List<PaymentReportMonthCard> Months { get; set; }

            /// <summary>
            /// </summary>
            /// <param name="WorkerId">Id работника</param>
            /// <param name="Name">Полное имя аботника</param>
            /// <param name="Points">Точки учета связанные с работником</param>
            /// <param name="exsistWorkerReports">Отчеты уже созданные ранее (существующие из БД)</param>
            public WorkerRegPoints(int WorkerId, string Name, List<RegPointInfo> Points, List<PaymentReport> exsistWorkerReports)
            {
                this.WorkerId = WorkerId;
                this.Name = Name;
                this.Points = Points;
                Months = new List<PaymentReportMonthCard>();
                //Сгруппируем по отчетным периодам
                var gr = Points.GroupBy(d => d.DatePeriodStart).ToList();
                gr.ForEach(period =>
                {
                    //Ищем месяц в отчетах
                    var month = Months.FirstOrDefault(r => r.Date.Month == period.Key.Month && r.Date.Year == period.Key.Year);
                    //Существующие отчеты
                    int? firstPeriodActId = exsistWorkerReports.FirstOrDefault(fr => fr.DatePeriodStart.Day == 1 && fr.DatePeriodStart.Month == period.Key.Month && fr.DatePeriodStart.Year == period.Key.Year)?.Id;
                    int? secondPeriodActId = exsistWorkerReports.FirstOrDefault(fr => fr.DatePeriodStart.Day == 16 && fr.DatePeriodStart.Month == period.Key.Month && fr.DatePeriodStart.Year == period.Key.Year)?.Id;
                    //Если нет - создаем
                    if (month == null)
                    {
                        month = new PaymentReportMonthCard(firstPeriodActId, secondPeriodActId, WorkerId, period.Key);
                        Months.Add(month);
                    }
                    if (period.Key.Day == 1)
                    {
                        month.First.CountAviablePU = period.Count(d => d.RegPointStatus == RegPointStatus.Default && !d.IsInPayReportAlready);
                        month.First.CountPULinkIsOk = period.Count(d => d.RegPointStatus == RegPointStatus.Default && d.IsLinkOk && !d.IsInPayReportAlready);
                        month.First.CountAviableUSPD = period.Count(d => d.RegPointStatus == RegPointStatus.USPD && !d.IsInPayReportAlready);
                    }
                    else
                    {
                        month.Second.CountAviablePU = period.Count(d => d.RegPointStatus == RegPointStatus.Default && !d.IsInPayReportAlready);
                        month.Second.CountPULinkIsOk = period.Count(d => d.RegPointStatus == RegPointStatus.Default && d.IsLinkOk && !d.IsInPayReportAlready);
                        month.Second.CountAviableUSPD = period.Count(d => d.RegPointStatus == RegPointStatus.USPD && !d.IsInPayReportAlready);
                    }
                });
            }
        }
    }
}