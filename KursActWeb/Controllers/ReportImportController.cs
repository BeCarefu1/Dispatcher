using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ActGIPelectroWeb.ViewModels;
using ActGIPelectroWeb.Models;
using GIPManager;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ActGIPelectroWeb.Controllers
{
    public class ReportImportController : Controller
    {
        private readonly StoreContext _db;
        public ReportImportController(StoreContext db)
        {
            _db = db;
        }
        private async Task<List<RegPointRowViewModel>> ALreportToRegPoints(int reportId) //Отчет по ВЛ в RegPointsViewModel 
        {
            List<RegPointRowViewModel> regPoints = new List<RegPointRowViewModel>();
            using (StoreContext db = new StoreContext())
            {
                MounterReportUgesAL report = await db.MounterReportUgesALs.FindAsync(reportId);
                if (report != null)
                {
                    foreach (var support in report.PowerLineSupports)
                    {
                        foreach (var kde in support.KDEs)
                        {
                            foreach (var pu in kde.MounterReportUgesDeviceItems)
                            {
                                Device device = await db.Devices.FirstOrDefaultAsync(d => d.SerialNumber == pu.Serial);
                                if (device != null)
                                {
                                    string adrress = report.Local + ", ул." + pu.Street + ", д." + pu.House + ", корп." + pu.Building + ", кв." + pu.Flat;
                                    RegPointRowViewModel regPoint = new RegPointRowViewModel(adrress, "ВЛ-0,4 кВ, " + pu.InstallPlace + " №" + support.SupportNumber, device.DeviceType.Name, device.SerialNumber, pu.PhoneNumber);
                                    regPoints.Add(regPoint);
                                }
                            }
                        }
                    }
                }
                return regPoints;
            }
        }

        private async Task<List<RegPointRowViewModel>> SwitchReportToRegPoints(int reportId, string reportType) //Отчеты по ТП и УСПД в RegPointsViewModel 
        {
            List<RegPointRowViewModel> regPoints = new List<RegPointRowViewModel>();
            using (StoreContext db = new StoreContext())
            {
                dynamic report = null;
                switch (reportType)
                {
                    case "ТП/РП":
                        report = await db.SBReports.FindAsync(reportId);
                        break;
                    case "УСПД":
                        report = await db.USPDReports.FindAsync(reportId);
                        break;
                }


                if (report != null)
                {
                    string switchNumber = "РУ-0,4, ";
                    foreach (Switch _switch in report.Switches)
                    {
                        switchNumber += _switch.SwitchType + " №";
                        string serial = _switch.DeviceSerial;
                        Device device = await db.Devices.FirstOrDefaultAsync(d => d.SerialNumber == serial);
                        if (device != null)
                        {
                            string phoneNumber = "";
                            string adrress = report.Local;
                            phoneNumber = report.PhoneNumber;

                            RegPointRowViewModel regPoint = new RegPointRowViewModel(adrress, switchNumber + _switch.SwitchNumber, device.DeviceType.Name, device.SerialNumber, phoneNumber);
                            regPoints.Add(regPoint);

                        }
                    }
                }
                return regPoints;
            }
        }

        private async Task<string> ALReportImport(int reportId) //Импорт отчета ВЛ 
        {
            int addedCount = 0;
            using (StoreContext db = new StoreContext())
            {
                User user = db.Users.FirstOrDefault(u => u.Login == User.Identity.Name);
                MounterReportUgesAL report = await db.MounterReportUgesALs.FindAsync(reportId);

                if (report != null) //Проврека существования отчета
                {
                    //Смена типа состояния отчета
                    report.ChangeState(ReportStateTypeName.ForImport);
                    db.SaveChanges();
                }

                else return "Отчет не найден в БД!";
            }
            return "Отчет поставлен в очередь на импорт";
        }

        private async Task<string> SbReportImport(int reportId) //Импорт отчета по ТП/РП 
        {

            using (StoreContext db = new StoreContext())
            {
                SBReport report = await db.SBReports.FindAsync(reportId);

                if (report != null) //Если отчет найден
                {

                    //Смена типа состояния отчета
                    report.ChangeState(ReportStateTypeName.ForImport);
                    db.SaveChanges();

                }
                else return "Отчет не найден в БД!";
            }
            return "Отчет поставлен в очередь на импорт";
        }

        private async Task<string> UspdReportImport(int reportId) //Импорт отчета по УСПД 
        {
            using (StoreContext db = new StoreContext())
            {

                USPDReport report = await db.USPDReports.FindAsync(reportId);

                if (report != null) //Если отчет найден
                {

                    //Смена типа состояния отчета
                    report.ChangeState(ReportStateTypeName.ForImport);
                    db.SaveChanges();

                }
                else return "Отчет не найден в БД!";
            }
            return "Отчет поставлен в очередь на импорт";
        }

        private int GetReportCount(Worker worker) //Подсчет кол-ва отчетов по монтажнику 
        {
            var count = 0;

            using (StoreContext db = new StoreContext())
            {
                foreach (var alReport in db.MounterReportUgesALs.Where(r => r.ReportState.Description == ReportStateTypeName.AcceptedByCurator.ToString() && r.WorkerId == worker.Id))
                {
                    count++;
                }
                foreach (var sbReport in db.SBReports.Where(r => r.ReportState.Description == ReportStateTypeName.AcceptedByCurator.ToString() && r.WorkerId == worker.Id))
                {
                    count++;
                }
                foreach (var uspdReport in db.USPDReports.Where(r => r.ReportState.Description == ReportStateTypeName.AcceptedByCurator.ToString() && r.WorkerId == worker.Id))
                {
                    count++;
                }
            }
            return count;
        }

        public async Task<IActionResult> GetReportRegPoints(int reportId, string reportType) //Просмотр точек отчета в виде regpoints 
        {
            switch (reportType)
            {
                case ("ВЛ"):
                    return PartialView("_PointsTable", await ALreportToRegPoints(reportId));
                default:
                    return PartialView("_PointsTable", await SwitchReportToRegPoints(reportId, reportType));
            }
        }

        public IActionResult GetWorkers() //Список работников с отчетами
        {
            List<DropDownItem> dropDownMounters = new List<DropDownItem>();
            dropDownMounters.Add(new DropDownItem { Id = 0, Name = "---" });
            using (StoreContext db = new StoreContext())
            {
                foreach (var item in db.Workers.Where(p => p.WorkerType.Description == "монтажник"))
                {
                    dropDownMounters.Add(new DropDownItem { Id = item.Id, Name = item.Surname + " " + item.Name + " " + item.MIddlename + " [" + GetReportCount(item) + "]" });
                }
            }
            SelectList mounters = new SelectList(dropDownMounters, "Id", "Name");
            return PartialView("_workers", mounters);
        }

        public IActionResult Reports() //Страница выбора подрядчика и просмотрп отчетов 
        {
            //bool phoneNumberFound = false;
            List<DropDownItem> dropDownMounters = new List<DropDownItem>();
            dropDownMounters.Add(new DropDownItem { Id = 0, Name = "---" });
            using (StoreContext db = new StoreContext())
            {
                foreach (var item in db.Workers.Where(p => p.WorkerType.Description == "монтажник"))
                {
                    dropDownMounters.Add(new DropDownItem { Id = item.Id, Name = item.Surname + " " + item.Name + " " + item.MIddlename + " [" + GetReportCount(item) + "]" });

                }


            }
            SelectList mounters = new SelectList(dropDownMounters, "Id", "Name");
            ViewBag.Mounters = mounters;
            return View();
        }

        public async Task<IActionResult> GetMounterReports(int workerId) //Отчеты работника 
        {
            using (StoreContext db = new StoreContext())
            {
                var worker = await db.Workers.FindAsync(workerId);
                if (worker != null)
                {
                    ReportState reportState = await db.ReportStates.FirstOrDefaultAsync(r => r.Description == "принят куратором");
                    List<Report> reports = new List<Report>();

                    //Отчеты по ВЛ
                    foreach (var alReport in db.MounterReportUgesALs.Where(r => r.WorkerId == workerId && r.ReportStateId == reportState.Id))
                    {
                        var smrClosed = false;
                        var sb = db.Substations.FirstOrDefault(s => s.Name == alReport.Substation && s.NetRegion.ContractId == alReport.ContractId);
                        string sbName = alReport.Substation;
                        if (sb != null)
                            smrClosed = sb.IsInstallationDone;

                        if (smrClosed) sbName += " СМР закрыт";
                        Report tmp = new Report { Id = alReport.Id, Date = alReport.Date, Mounted = 0, NetRegion = alReport.NetRegion.Name, Substation = sbName, Type = "ВЛ", ClosedSMR = smrClosed };
                        int count = 0;
                        foreach (var support in alReport.PowerLineSupports) //Подсчет ПУ вотчете
                        {
                            foreach (var kde in support.KDEs)
                            {
                                foreach (var pu in kde.MounterReportUgesDeviceItems)
                                {
                                    count++;
                                }
                            }
                        }
                        tmp.Mounted = count;
                        reports.Add(tmp);
                    }
                    //отчеты по ТП/РП
                    foreach (var sbReport in db.SBReports.Where(r => r.WorkerId == workerId && r.ReportStateId == reportState.Id))
                    {
                        var smrClosed = false;
                        var sb = db.Substations.FirstOrDefault(s => s.Name == sbReport.Substation && s.NetRegion.ContractId == sbReport.ContractId);
                        string sbName = sbReport.Substation;
                        if (sb != null)
                            smrClosed = sb.IsInstallationDone;

                        if (smrClosed) sbName += " СМР закрыт";
                        reports.Add(new Report { Id = sbReport.Id, Date = sbReport.Date, Mounted = sbReport.Switches.Count, NetRegion = sbReport.NetRegion.Name, Substation = sbName, Type = "ТП/РП", ClosedSMR = smrClosed });
                    }
                    //Отчеты по УСПД
                    foreach (var uspdReport in db.USPDReports.Where(r => r.WorkerId == workerId && r.ReportStateId == reportState.Id))
                    {
                        var smrClosed = false;
                        var sb = db.Substations.FirstOrDefault(s => s.Name == uspdReport.Substation && s.NetRegion.ContractId == uspdReport.ContractId);
                        string sbName = uspdReport.Substation;
                        if (sb != null)
                            smrClosed = sb.IsInstallationDone;

                        if (smrClosed) sbName += " СМР закрыт";
                        reports.Add(new Report { Id = uspdReport.Id, Date = uspdReport.Date, Mounted = uspdReport.Switches.Count, NetRegion = uspdReport.NetRegion.Name, Substation = sbName, Type = "УСПД" });
                    }

                    return PartialView("_reports", reports);
                }
                else return PartialView("_reports", new List<Report>());
            }
        }

        public async Task<string> ReportImport(int reportId, string reportType) //Импорт отчета 
        {
            switch (reportType)
            {
                case "ВЛ":
                    return await ALReportImport(reportId);
                case "ТП/РП":
                    return await SbReportImport(reportId);
                case "УСПД":
                    return await UspdReportImport(reportId);
                default:
                    return "Неопределенный тип отчета";
            }
        }

        public string GetReportCount() //Подсчет кол-во отчетов для импорта для _Layout 
        {
            var count = 0;

            using (StoreContext db = new StoreContext())
            {
                foreach (var alReport in db.MounterReportUgesALs.Where(r => r.ReportState.Description == ReportStateTypeName.AcceptedByCurator.ToString()))
                {
                    count++;
                }
                foreach (var sbReport in db.SBReports.Where(r => r.ReportState.Description == ReportStateTypeName.AcceptedByCurator.ToString()))
                {
                    count++;
                }
                foreach (var uspdReport in db.USPDReports.Where(r => r.ReportState.Description == ReportStateTypeName.AcceptedByCurator.ToString()))
                {
                    count++;
                }
            }
            return count.ToString();

        }

        public async Task<string> AddRemarkToReport(int reportId, string reportType, string text) //Добавляет замечание к отчету и отправляет монтажнику 
        {
            using (StoreContext db = new StoreContext())
            {
                User user = await db.Users.FirstOrDefaultAsync(u => u.Login == User.Identity.Name);
                switch (reportType)
                {
                    case "ВЛ":
                        MounterReportUgesAL report = await db.MounterReportUgesALs.FindAsync(reportId);
                        ReportType reportTp = await db.ReportTypes.Where(t => t.Description == reportType).FirstAsync();
                        if (report != null && reportTp != null)
                        {
                            await db.ReportRemarks.AddAsync(new ReportRemark { Text = text, ReportId = reportId, ReportTypeId = reportTp.Id, UserId = user.Id });
                            ReportState state = await db.ReportStates.Where(s => s.Description == "с замечаниями куратора").FirstAsync();
                            report.ReportStateId = state.Id;
                            await db.SaveChangesAsync();
                            return "Замечания отправлены!";
                        }
                        else return "Ошибка сохранения отчета!";

                    case "ТП/РП":
                        SBReport sbReport = await db.SBReports.FindAsync(reportId);
                        ReportType sbReportTp = await db.ReportTypes.Where(t => t.Description == reportType).FirstAsync();
                        if (sbReport != null && sbReportTp != null)
                        {
                            await db.ReportRemarks.AddAsync(new ReportRemark { Text = text, ReportId = reportId, ReportTypeId = sbReportTp.Id, UserId = user.Id });
                            ReportState state = await db.ReportStates.Where(s => s.Description == "с замечаниями куратора").FirstAsync();
                            sbReport.ReportStateId = state.Id;
                            await db.SaveChangesAsync();
                            return "Замечания отправлены!";
                        }
                        else return "Ошибка сохранения отчета!";

                    case "УСПД":
                        USPDReport uspdReport = await db.USPDReports.FindAsync(reportId);
                        ReportType uspdReportTp = await db.ReportTypes.Where(t => t.Description == reportType).FirstAsync();
                        if (uspdReport != null && uspdReportTp != null)
                        {
                            await db.ReportRemarks.AddAsync(new ReportRemark { Text = text, ReportId = reportId, ReportTypeId = uspdReportTp.Id, UserId = user.Id });
                            ReportState state = await db.ReportStates.Where(s => s.Description == "с замечаниями куратора").FirstAsync();
                            uspdReport.ReportStateId = state.Id;
                            await db.SaveChangesAsync();
                            return "Замечания отправлены!";
                        }
                        else return "Ошибка сохранения отчета!";

                    case "Демонтаж":
                        UnmountReport unmountReport = await db.UnmountReports.FindAsync(reportId);
                        ReportType unmountReportType = await db.ReportTypes.Where(t => t.Description == reportType).FirstAsync();
                        if (unmountReport != null && unmountReportType != null)
                        {
                            await db.ReportRemarks.AddAsync(new ReportRemark { Text = text, ReportId = reportId, ReportTypeId = unmountReportType.Id, UserId = user.Id });
                            ReportState state = await db.ReportStates.Where(s => s.Description == "с замечаниями куратора").FirstAsync();
                            unmountReport.ReportStateId = state.Id;
                            await db.SaveChangesAsync();
                            return "Замечания отправлены!";
                        }
                        else return "Ошибка сохранения отчета!";

                    default:
                        return "Операция не определена!";


                }
            }
        }

        public async Task<IActionResult> GetReportRemarks(int reportId, string reportType) //Получить замечания по отчету 
        {

            List<ReportRemark> remarks = new List<ReportRemark>();
            ReportType type = await _db.ReportTypes.FirstOrDefaultAsync(t => t.Description == reportType);
            remarks = _db.ReportRemarks.Where(r => r.ReportTypeId == type.Id && r.ReportId == reportId).ToList();
            return PartialView("/Views/ReportImport/_remarks.cshtml", remarks);

        }

        public async Task<IActionResult> AddCommentToReport(int reportId, string reportType, string text) //Добавить комментарий к отчету
        {
            User user = await _db.Users.FirstOrDefaultAsync(u => u.Login == User.Identity.Name);
            var _reportType = await _db.ReportTypes.FirstOrDefaultAsync(t => t.Description == reportType);

            await _db.ReportComments.AddAsync(new ReportComment { Date = DateTime.Now, ReportId = reportId, ReportTypeId = _reportType.Id, UserId = user.Id, Text = text });
            await _db.SaveChangesAsync();
            return GetReportComments(reportId, reportType);
        }

        public IActionResult GetReportComments(int reportId, string reportType) //Полуить все комментарии к отчету
        {
            var _reportType = _db.ReportTypes.FirstOrDefault(t => t.Description == reportType);
            var comments = from comment in _db.ReportComments
                           where comment.ReportTypeId == _reportType.Id && comment.ReportId == reportId
                           select comment;
            var _comments = comments.ToList();
            return PartialView("/Views/ReportImport/_comments.cshtml", _comments);
        }
    }
}