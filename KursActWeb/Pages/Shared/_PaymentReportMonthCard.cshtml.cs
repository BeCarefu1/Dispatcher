using System;

namespace ActGIPelectroWeb.Pages
{
    public class PaymentReportMonthCard
    {
        public DateTime Date { get; set; }
        public string PeriodName { get => Date.ToString("MMMM yyyy"); }
        public PaymentReportMonthPeriod First { get; set; }
        public PaymentReportMonthPeriod Second { get; set; }

        public PaymentReportMonthCard(int? firstPeriodActId, int? secondPeriodActId, int workerId, DateTime date)
        {
            Date = date;
            First = new PaymentReportMonthPeriod(firstPeriodActId, workerId, Date.Year, Date.Month, 1);
            Second = new PaymentReportMonthPeriod(secondPeriodActId, workerId, Date.Year, Date.Month, 16);
        }
    }

    public class PaymentReportMonthPeriod
    {
        public PaymentReportMonthPeriod(int workerId, int year, int month, int day) {
            this.workerId = workerId;
            this.year = year;
            this.month = month;
            this.day = day;
        }
        public PaymentReportMonthPeriod(int? id, int workerId, int year, int month, int day)
        {
            PaymentReportId = id ?? 0;
            this.workerId = workerId;
            this.year = year;
            this.month = month;
            this.day = day;
        }

        private readonly int workerId;
        private int year;
        private int month;
        private int day;

        public bool IsEnable { get => DateTime.Now >= new DateTime(year, month, day); }
        public DateTime DateStartPeriod { get => new DateTime(year, month, day); }

        public int PaymentReportId { get; set; }
        public string ReportIcon { get => (PaymentReportId == 0) ? "fas fa-file-medical" : "fas fa-file-invoice-dollar text-success"; }
        public string ReportIconOnClick { get => (PaymentReportId == 0) ? "CreatePaymentReport(" + workerId + ", " + year + ", " + month + ", " + day + ")" : "document.location.href='/PaymentReportPage/"+ PaymentReportId + "'"; }

        public int CountAviablePU { get; set; }
        public int CountPULinkIsOk { get; set; }
        public int CountAviableUSPD { get; set; }
    }
}
