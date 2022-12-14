using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GIPManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ActGIPelectroWeb.Pages
{
    public class PaymentReportPageModel : PageModel
    {
        StoreContext db;
        public PaymentReportPageModel(StoreContext context)
        {
            db = context;
        }
        
        public string WorkerName { get; set; }
        public string Period { get; set; }
        public PaymentReport PaymentReport { get; set; }

        public void OnGet(int id)
        {
            PaymentReport = db.PaymentReports.Find(id);
            WorkerName = 
                PaymentReport.Worker.Name + " " +
                PaymentReport.Worker.Surname + " " +
                PaymentReport.Worker.MIddlename;
            var datePeriodEnd = PaymentReport.DatePeriodStart.Day == 1? 
                new DateTime(PaymentReport.DatePeriodStart.Year, PaymentReport.DatePeriodStart.Month, 15) : 
                new DateTime(PaymentReport.DatePeriodStart.Year, PaymentReport.DatePeriodStart.Month, 1).AddDays(-1); // стало
                /*new DateTime(PaymentReport.DatePeriodStart.Year, PaymentReport.DatePeriodStart.Month + 1, 1).AddDays(-1);*/ //было
            Period = PaymentReport.DatePeriodStart.ToString("dd.MM.yyyy") + " - " + datePeriodEnd.ToString("dd.MM.yyyy");
        }
    }
}