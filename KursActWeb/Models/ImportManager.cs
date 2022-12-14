using GIPManager;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace ActGIPelectroWeb.Models
{
    public static class ImportManager
    {
        //public static string USPD ()
        //{
        //    int count = 0;
        //    using (StoreContext db = new StoreContext())
        //    {
        //        var reports = from report in db.USPDReports
        //                      where report.ReportState.Description == ReportStateTypeName.Imported.ToString()
        //                      select report;
        //        foreach (var report in reports)
        //        {
        //            var uspd = db.Devices.FirstOrDefault(d => d.SerialNumber == report.UspdSerial);
        //            var plc = db.Devices.FirstOrDefault(d => d.SerialNumber == report.PlcSerial);
        //            var substation = db.Substations.FirstOrDefault(s => s.Name == report.Substation);
        //            var regPoint = db.RegPoints.FirstOrDefault(r => r.SubstationId == substation.Id && r.DeviceId == uspd.Id);
        //            if(uspd != null && regPoint == null)
        //            {
                        
        //                    substation.AddRegPoint(uspd.Id, report.CuratorId ?? 1);
        //                    count++;
        //            }
                    
        //            if (uspd != null)
        //            {
        //                var sbDevice = db.SubstationDevices.FirstOrDefault(d => d.SubstationId == substation.Id && d.DeviceId == uspd.Id);
        //                if (sbDevice == null)
        //                    db.SubstationDevices.Add(new SubstationDevice() { DeviceId = uspd.Id, SubstationId = substation.Id });
        //            }

        //            if (plc != null)
        //            {
        //                var sbDevice = db.SubstationDevices.FirstOrDefault(d => d.DeviceId == plc.Id && d.SubstationId == substation.Id);
        //                if (sbDevice == null)
        //                    db.SubstationDevices.Add(new SubstationDevice() { DeviceId = plc.Id, SubstationId = substation.Id });
        //            }
        //        }
        //        db.SaveChanges();
        //    }
        //    return "Обработано " + count + " записей";
        //}

        public static void ImportReports()
        {
            while (true)
            { 
                int importedCount = 0;
                using (StoreContext db = new StoreContext())
                {
                    foreach (var report in db.MounterReportUgesALs.Where(r => r.ReportState.Description == ReportStateTypeName.ForImport.ToString()))
                    {
                        ALReportImport(report.Id, db);
                        importedCount++;
                    }
                    foreach (var report in db.SBReports.Where(r => r.ReportState.Description == ReportStateTypeName.ForImport.ToString()))
                    {
                        SbReportImport(report.Id, db);
                        importedCount++;
                    }
                    foreach (var report in db.USPDReports.Where(r => r.ReportState.Description == ReportStateTypeName.ForImport.ToString()))
                    {
                        UspdReportImport(report.Id, db);
                        importedCount++;
                    }
                    foreach (var report in db.UnmountReports.Where(r => r.ReportState.Description == ReportStateTypeName.ForImport.ToString()))
                    {
                        UnmountReportImport(report.Id, db);
                        importedCount++;
                    }
                }
                Thread.Sleep(3600000);
            }
            
        }

        private static string ALReportImport(int reportId, StoreContext db) //Импорт отчета ВЛ 
        {
            int addedCount = 0;
            bool error = false;
           
                //User user = db.Users.FirstOrDefault(u => u.Login == User.Identity.Name);
                MounterReportUgesAL report = db.MounterReportUgesALs.Find(reportId);

            if (report != null) //Проврека существования отчета
            {
                NetRegion netRegion = db.NetRegions.FirstOrDefault(r => r.ContractId == report.ContractId && r.Id == report.NetRegionId);
                if (netRegion != null) //Проврека существования РЭСа
                {
                    netRegion.AddSubstation(report.Substation, 1);
                    db.SaveChanges();
                    Substation substation = db.Substations.FirstOrDefault(s => s.Name == report.Substation && s.NetRegionId == netRegion.Id);
                    if (substation != null)//проверка существования подстанции
                    {
                        foreach (var support in report.PowerLineSupports) //Опоры в отчете
                        {
                            foreach (var kde in support.KDEs) //КДЕ на опоре
                            {
                                foreach(var pu in kde.MounterReportUgesDeviceItems) //цикл проверки
                                {
                                    Device device = db.Devices.First(d => d.SerialNumber == pu.Serial); //Поиск ПУ в БД
                                    if (device == null)
                                    {
                                        pu.ErrorCode = MounterReportImportError.DeviceNotFound;
                                        error = true;
                                    }
                                        
                                    if (device.CurrentState != DeviceStateTypeName.AcceptedByCurator.Name)
                                    {
                                        pu.ErrorCode = MounterReportImportError.WrongState;
                                        error = true;
                                    }
                                        
                                    if (!device.IsEnableToImportTU())
                                    {
                                        pu.ErrorCode = MounterReportImportError.ExistingRegPoint;
                                        error = true;
                                    }
                                    
                                    db.SaveChanges();
                                }
                                if (error) return "Невозможно импортировать отчет";
                                

                                foreach (var pu in kde.MounterReportUgesDeviceItems) //ПУ в КДЕ
                                {
                                    Device device = db.Devices.First(d => d.SerialNumber == pu.Serial); //Поиск ПУ в БД
                                    if (device != null && device.CurrentState == DeviceStateTypeName.AcceptedByCurator.Name && device.IsEnableToImportTU()) //Если пу найден и его возможно прикрепить к ТУ
                                    {

                                        RegPointFlags flags = new RegPointFlags();
                                        InstallAct installAct = new InstallAct();
                                        Consumer consumer = new Consumer();

                                        //заполнение таблицы RegPointFlags
                                        flags.ReportedByMounter = true;

                                        //заполнение таблицы InstallAct
                                        installAct.InstallActType = InstallActType.VL;
                                        if (pu.InstallPlace == "Опора")
                                            installAct.InstallPlaceTypeId = EnumsHelper.GetInstallPlaceTypeId(InstallPlaceTypeName.Opora);
                                        if (pu.InstallPlace == "Фасад")
                                            installAct.InstallPlaceTypeId = EnumsHelper.GetInstallPlaceTypeId(InstallPlaceTypeName.Fasad);
                                        installAct.InstallPlaceNumber = support.SupportNumber;
                                        installAct.IsCommReg = true;
                                        installAct.IsOutFlow = false;
                                        installAct.Line = report.Fider;
                                        installAct.Seal_AV1 = pu.SwitchSeal;
                                        installAct.Seal_KDE = pu.KDESeal;
                                        installAct.Seal_RegDevice = pu.DeviceSeal;
                                        installAct.Tsum = pu.Sum.ToString();
                                        installAct.T1 = pu.T1.ToString();
                                        installAct.T2 = pu.T2.ToString();

                                        //Заполнение таблицы Consumer
                                        consumer.U_Local = consumer.O_Local = report.Local;
                                        consumer.U_Street = consumer.O_Street = pu.Street;
                                        consumer.U_House = consumer.O_House = pu.House;
                                        consumer.U_Build = consumer.O_Build = pu.Building;
                                        consumer.U_Flat = consumer.O_Flat = pu.Flat;

                                        //Создание ТУ 
                                        substation.AddRegPoint(device.Id, report.CuratorId ?? 1, flags, installAct, consumer);
                                        addedCount++;

                                        //Link
                                        if (pu.PhoneNumber != null && pu.PhoneNumber != "") //если в отчете у ТУ указан номер СИМ
                                        {
                                            bool phoneNumberFound = false;
                                            List<Link> links = db.Links.Where(l => l.DeviceId == device.Id).ToList();
                                            foreach (var link in links)
                                            {
                                                if (link.PhoneNumber == pu.PhoneNumber)
                                                {
                                                    phoneNumberFound = true;
                                                    break;
                                                }
                                            }
                                            if (links.Count == 0 || !phoneNumberFound) //если нет записей в Link с этим deviceId или в Link с этим deviceId нет номера сим из отчета
                                                db.Links.Add(new Link { DeviceId = device.Id, PhoneNumber = pu.PhoneNumber, LinkType = LinkTypeName.BuiltIn.ToString() });
                                        }
                                        db.SaveChanges();
                                    }
                                    else
                                    {
                                        return "Ошибка импорта отчета, ПУ №" + pu.Serial + " не найден в БД!";
                                    }
                                }
                            }
                        }
                        //Смена типа состояния отчета
                        report.ChangeState(ReportStateTypeName.Imported);
                        db.SaveChanges();
                    }
                    else return "Ошибка создания подстанции " + report.Substation + " в " + netRegion.Name + "!";
                }
                else return "РЭС не найден в БД!";
            }
            else return "Отчет не найден в БД!";
            
            return "Импортировано " + addedCount + " точек учета.";
        }

        private static string SbReportImport(int reportId, StoreContext db) //Импорт отчета по ТП/РП 
        {
            int addedCount = 0;
            
                //User user = db.Users.FirstOrDefault(u => u.Login == User.Identity.Name);
           SBReport report = db.SBReports.Find(reportId);
            bool error = false;
                if (report != null) //Если отчет найден
                {
                    NetRegion netRegion = db.NetRegions.Find(report.NetRegionId); //Поиск РЭСа
                    if (netRegion != null) //Если РЭС найден
                    {
                        netRegion.AddSubstation(report.Substation, 1); //Создание подстанции
                        db.SaveChanges();
                        Substation substation = db.Substations.FirstOrDefault(s => s.Name == report.Substation);
                        if (substation != null)
                        {

                            foreach (var pu in report.Switches) //цикл проверки
                            {
                                Device device = db.Devices.First(d => d.SerialNumber == pu.DeviceSerial); //Поиск ПУ в БД
                                if (device == null)
                                {
                                    pu.ErrorCode = MounterReportImportError.DeviceNotFound;
                                    error = true;
                                }

                                if (device.CurrentState != DeviceStateTypeName.AcceptedByCurator.Name)
                                {
                                    pu.ErrorCode = MounterReportImportError.WrongState;
                                    error = true;
                                }

                                if (!device.IsEnableToImportTU())
                                {
                                    pu.ErrorCode = MounterReportImportError.ExistingRegPoint;
                                    error = true;
                                }

                                db.SaveChanges();
                            }
                        if (error) return "Невозможно импортировать отчет";

                        foreach (var pu in report.Switches)
                            {
                                Device device = db.Devices.FirstOrDefault(d => d.SerialNumber == pu.DeviceSerial);
                                if (device != null && device.CurrentState == DeviceStateTypeName.AcceptedByCurator.Name) //Если пу найден и его возможно прикрепит к ТУ
                                {
                                    RegPointFlags flags = new RegPointFlags();
                                    InstallAct installAct = new InstallAct();

                                    //заполнение таблицы RegPointFlags
                                    flags.ReportedByMounter = true;

                                    //Заполнение таблицы InstallAct
                                    installAct.InstallActType = InstallActType.None;
                                    if (pu.SwitchType == "Ввод") //Если ПУ установлен на вводе
                                    {
                                        if (int.TryParse(pu.SwitchNumber, out int switchNumber))
                                        {
                                            installAct.InstallPlaceTypeId = EnumsHelper.GetInstallPlaceTypeId(InstallPlaceTypeName.Vvod);
                                            installAct.InstallPlaceNumber = switchNumber;
                                            installAct.IsCommReg = false;
                                            installAct.IsOutFlow = true;
                                        }
                                        else return "№ рубильника (" + pu.SwitchNumber.ToString() + ") не разобран!";
                                    }
                                    else //Если ПУ на рубильнике
                                    {
                                        if (int.TryParse(pu.SwitchNumber, out int switchNumber))
                                        {
                                            installAct.InstallPlaceTypeId = EnumsHelper.GetInstallPlaceTypeId(InstallPlaceTypeName.Rub);
                                            installAct.InstallPlaceNumber = switchNumber;
                                            installAct.IsCommReg = true;
                                            installAct.IsOutFlow = false;
                                        }
                                        else return "№ рубильника (" + pu.SwitchNumber.ToString() + ") не разобран!";
                                    }
                                    installAct.Seal_KI = pu.TestBoxSeal;
                                    installAct.Seal_RegDevice = pu.DeviceSeal;
                                    installAct.Seal_TT_A = pu.TTASeal;
                                    installAct.Seal_TT_B = pu.TTBSeal;
                                    installAct.Seal_TT_C = pu.TTCSeal;
                                    installAct.T1 = pu.T1.ToString();
                                    installAct.T2 = pu.T2.ToString();
                                    installAct.Tsum = pu.Sum.ToString();
                                    installAct.TT_A_Serial = pu.TTANumber;
                                    installAct.TT_B_Serial = pu.TTBNumber;
                                    installAct.TT_C_Serial = pu.TTCNumber;
                                    installAct.TT_Koefficient = pu.TTAk.ToString() + "/5";

                                    //Создание ТУ 
                                    substation.AddRegPoint(device.Id, report.CuratorId ?? 1, flags, installAct, null);
                                    addedCount++;

                                    //Link
                                    if (report.PhoneNumber != null && report.PhoneNumber != "") //если в отчете у ТУ указан номер СИМ
                                    {
                                        bool phoneNumberFound = false;
                                        List<Link> links = db.Links.Where(l => l.DeviceId == device.Id).ToList();
                                        foreach (var link in links)
                                        {
                                            if (link.PhoneNumber == report.PhoneNumber)
                                            {
                                                phoneNumberFound = true;
                                                break;
                                            }
                                        }
                                        if (links.Count == 0 || !phoneNumberFound) //если нет записей в Link с этим deviceId или в Link с этим deviceId нет номера сим из отчета
                                            db.Links.Add(new Link { DeviceId = device.Id, PhoneNumber = report.PhoneNumber, LinkType = LinkTypeName.External.ToString() });
                                    }
                                    db.SaveChanges();
                                }
                                else return "ПУ №" + pu.DeviceSerial + " не найден в БД!";
                            }
                            //Смена типа состояния отчета
                            report.ChangeState(ReportStateTypeName.Imported);
                            db.SaveChanges();
                        }
                        else return "Ошибка создания подстанции";
                    }
                    else return "РЭС не найден в БД!";
                }
                else return "Отчет не найден в БД!";
            
            return "Импортировано " + addedCount + " точек учета.";
        }

        private static string UspdReportImport(int reportId, StoreContext db) //Импорт отчета по УСПД 
        {
            int addedCount = 0;
           
            //User user = db.Users.FirstOrDefault(u => u.Login == User.Identity.Name);
            USPDReport report = db.USPDReports.Find(reportId);
            bool phoneNumberFound = false;
            if (report != null) //Если отчет найден
            {
                NetRegion netRegion = db.NetRegions.Find(report.NetRegionId); //Поиск РЭСа
                if (netRegion != null) //Если РЭС найден
                {
                    netRegion.AddSubstation(report.Substation, 1); //Создание подстанции
                    db.SaveChanges();
                    Substation substation = db.Substations.FirstOrDefault(s => s.Name == report.Substation);
                    if (substation != null)
                    {
                        foreach (var pu in report.Switches)
                        {
                            Device device = db.Devices.FirstOrDefault(d => d.SerialNumber == pu.DeviceSerial);
                            if (device != null && device.CurrentState == DeviceStateTypeName.AcceptedByCurator.Name && device.IsEnableToImportTU()) //Если пу найден и его возможно прикрепить к ТУ
                            {
                                RegPointFlags flags = new RegPointFlags();
                                InstallAct installAct = new InstallAct();

                                //заполнение таблицы RegPointFlags
                                flags.ReportedByMounter = true;

                                //Заполнение таблицы InstallAct
                                installAct.InstallActType = InstallActType.None;
                                if (pu.SwitchType == "Ввод") //Если ПУ установлен на вводе
                                {
                                    if (int.TryParse(pu.SwitchNumber, out int switchNumber))
                                    {
                                        installAct.InstallPlaceTypeId = EnumsHelper.GetInstallPlaceTypeId(InstallPlaceTypeName.Vvod);
                                        installAct.InstallPlaceNumber = switchNumber;
                                        installAct.IsCommReg = false;
                                        installAct.IsOutFlow = true;
                                    }
                                    else return "№ рубильника (" + pu.SwitchNumber.ToString() + ") не разобран!";
                                }
                                else //Если ПУ на рубильнике
                                {
                                    if (int.TryParse(pu.SwitchNumber, out int switchNumber))
                                    {
                                        installAct.InstallPlaceTypeId = EnumsHelper.GetInstallPlaceTypeId(InstallPlaceTypeName.Rub);
                                        installAct.InstallPlaceNumber = switchNumber;
                                        installAct.IsCommReg = true;
                                        installAct.IsOutFlow = false;
                                    }
                                    else return "№ рубильника (" + pu.SwitchNumber.ToString() + ") не разобран!";
                                }
                                installAct.Seal_KI = pu.TestBoxSeal;
                                installAct.Seal_RegDevice = pu.DeviceSeal;
                                installAct.Seal_TT_A = pu.TTASeal;
                                installAct.Seal_TT_B = pu.TTBSeal;
                                installAct.Seal_TT_C = pu.TTCSeal;
                                installAct.T1 = pu.T1.ToString();
                                installAct.T2 = pu.T2.ToString();
                                installAct.Tsum = pu.Sum.ToString();
                                installAct.TT_A_Serial = pu.TTANumber;
                                installAct.TT_B_Serial = pu.TTBNumber;
                                installAct.TT_C_Serial = pu.TTCNumber;
                                installAct.TT_Koefficient = pu.TTAk.ToString() + "/5";

                                //Создание ТУ 
                                substation.AddRegPoint(device.Id, report.CuratorId??1, flags, installAct, null);
                                addedCount++;

                                //Link SubstationLink
                                if (report.PhoneNumber != null && report.PhoneNumber != "") //если в отчете указан номер СИМ
                                {
                                    //Link
                                    phoneNumberFound = false;
                                    List<Link> links = db.Links.Where(l => l.DeviceId == device.Id).ToList();
                                    foreach (var link in links)
                                    {
                                        if (link.PhoneNumber == report.PhoneNumber)
                                        {
                                            phoneNumberFound = true;
                                            break;
                                        }
                                    }
                                    if (!phoneNumberFound) //если нет записей в Link с этим deviceId или в Link с этим deviceId нет номера сим из отчета
                                    {
                                        db.Links.Add(new Link { DeviceId = device.Id, PhoneNumber = report.PhoneNumber, LinkType = LinkTypeName.External.ToString() });
                                        db.SaveChanges();
                                    }
                                }
                                db.SaveChanges();

                            }
                            else return "ПУ №" + pu.DeviceSerial + " не найден в БД!";
                        }

                        //SubstationLink
                        phoneNumberFound = false;
                        List<SubstationLink> substationLinks = db.SubstationLinks.Where(l => l.SubstationId == substation.Id).ToList();
                        foreach (var substationLink in substationLinks)
                        {
                            if (substationLink.PhoneNumber == report.PhoneNumber)
                            {
                                phoneNumberFound = true;
                                break;
                            }
                        }
                        if (!phoneNumberFound) //если нет записей в SubstationLink с этим SubststationId или в SubstationLinks нет номера сим из отчета
                        {
                            db.SubstationLinks.Add(new SubstationLink { SubstationId = substation.Id, PhoneNumber = report.PhoneNumber });
                            db.SaveChanges();
                        }

                        //Привязка УСПД и PLC модема к ТП
                        var uspd = db.Devices.FirstOrDefault(d => d.SerialNumber == report.UspdSerial);
                        if (uspd.IsEnableToImportTU())
                        {
                            substation.AddRegPoint(uspd.Id, report.CuratorId ?? 1);
                        }        
                        var plc = db.Devices.FirstOrDefault(d => d.SerialNumber == report.PlcSerial);

                        if(uspd != null)
                            db.SubstationDevices.Add(new SubstationDevice { DeviceId = uspd.Id, SubstationId = substation.Id }); //Привязка УСПД к ТП
                        if (plc != null)
                            db.SubstationDevices.Add(new SubstationDevice { DeviceId = plc.Id, SubstationId = substation.Id }); //Привязка PLC-модема к ТП

                        //Смена типа состояния отчета
                        report.ChangeState(ReportStateTypeName.Imported);
                        db.SaveChanges();
                    }
                    else return "Ошибка создания подстанции";
                }
                else return "РЭС не найден в БД!";
            }
            else return "Отчет не найден в БД!";
            
            return "Импортировано " + addedCount + " точек учета.";
        }

        private static void UnmountReportImport (int reportId, StoreContext db)
        {
            UnmountReport report = db.UnmountReports.Find(reportId);
            if(report != null)
            {
                foreach(var item in report.UnmountedDevices)
                {
                    Device device = db.Devices.Find(item.DeviceId);
                    if(device != null)
                    {
                        if(device.CurrentState == DeviceStateTypeName.AddToTU.Name) 
                        //Если текущий статус ПУ привязан к ТУ (т.е. ПУ демонтирован но не возвращен на склад) то ставим статус выдачи со склада
                        {
                            device.CurrentState = DeviceStateTypeName.Outcome.Name;
                            db.SaveChanges();
                        }
                        RegPoint regPoint = db.RegPoints.FirstOrDefault(r => r.DeviceId == device.Id && r.Status != RegPointStatus.Demounted);
                        if(regPoint != null)
                        {
                            regPoint.Status = RegPointStatus.Demounted;
                            db.SaveChanges();
                        }
                    }
                    report.ChangeState(ReportStateTypeName.Imported);
                    db.SaveChanges();
                }
            }
        }
    }
}
