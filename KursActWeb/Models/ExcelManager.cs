using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using GIPManager;
using ActGIPelectroWeb.ViewModels;
using Microsoft.AspNetCore.Hosting;
using StoreGIPManager.Worker;

namespace ActGIPelectroWeb.Models
{
    public class ExcelManagerFile
    {
        public string Name { get; set; }
        public byte[] Data { get; set; }
    }

    public class ExcelManager
    {
        IHostingEnvironment _env;
        public ExcelManager(IHostingEnvironment env = null)
        {
            _env = env;
        }

        /// <summary>
        /// Получить файл Excel список точек подстанции с информацией для ПНР (Серийник, флаги, номер симки, адрес)
        /// </summary>
        /// <param name="substation">Подстанция по которой нужна инфа</param>
        /// <returns></returns>
        public byte[] PointsInSubstationPNR(Substation substation)
        {
            var workbook = new XLWorkbook();
            string fileName = "notFound";
            if (substation == null) return null;

            fileName = substation.Name + "_" + DateTime.Now.ToShortDateString();
            var ws = workbook.Worksheets.Add(fileName);
            //создадим заголовки у столбцов
            ws.Cell(1, 1).Value = "№";
            ws.Cell(1, 2).Value = "Адрес";
            ws.Cell(1, 3).Value = "ФИО";
            ws.Cell(1, 4).Value = "Место установки";
            ws.Cell(1, 5).Value = "Тип ПУ";
            ws.Cell(1, 6).Value = "Тип связи";
            ws.Cell(1, 7).Value = "Зав.№";
            ws.Cell(1, 8).Value = "Сетевой №";
            ws.Cell(1, 9).Value = "Номер тел.";
            ws.Cell(1, 10).Value = "Связь";
            ws.Cell(1, 11).Value = "Отправлен в ЭС";
            ws.Cell(1, 12).Value = "В Энергосфере";
            ws.Range(ws.Cell(1, 1), ws.Cell(1, 12)).Style.Font.Bold = true;
            ws.Range(ws.Cell(1, 1), ws.Cell(1, 12)).Style.Fill.BackgroundColor = XLColor.LightGray;
            //Заполним список точек
            using (StoreContext db = new StoreContext())
            {
                var points = from p in db.RegPoints
                             where p.SubstationId == substation.Id
                             select new
                             {
                                 p.Id,
                                 p.Status,
                                 Address = RegPointRowViewModel.FormatAddress(
                                            p.Consumer.O_Local,
                                            p.Consumer.O_Local_Secondary,
                                            p.Consumer.O_Street,
                                            p.Consumer.O_House,
                                            p.Consumer.O_Build,
                                            p.Consumer.O_Flat
                                            ),
                                 ConsummerName = p.Consumer.Name,
                                 TT_Coefficient = p.InstallAct.TT_Koefficient,
                                 ModelPU = p.Device.DeviceType.Name,
                                 InstallPlace = p.InstallAct.InstallPlaceType.Name + p.InstallAct.InstallPlaceNumber,
                                 TypePU = p.Device.DeviceType.Description,
                                 SerialNum = p.Device.SerialNumber,
                                 PhoneNum = "-",
                                 //Flags
                                 p.RegPointFlags.IsLinkOk,
                                 p.RegPointFlags.IsAscueChecked,
                                 p.RegPointFlags.IsAscueOk
                             };
                //substation.RegPoints.ForEach(p => pointsList.Add(new RegPointViewModel(p)));

                int n = 1;
                foreach (var p in points)
                {
                    ws.Cell(n + 1, 1).Value = n;
                    ws.Cell(n + 1, 2).Value = p.Address;
                    ws.Cell(n + 1, 3).Value = p.ConsummerName;
                    ws.Cell(n + 1, 4).Value = p.InstallPlace + " " + p.TT_Coefficient;
                    ws.Cell(n + 1, 5).Value = (p.TypePU.Length > 5) ? p.TypePU.Substring(0, 5) : p.TypePU;
                    ws.Cell(n + 1, 6).Value = p.TypePU;
                    ws.Cell(n + 1, 7).Value = p.SerialNum;
                    ws.Cell(n + 1, 7).Style.NumberFormat.Format = "000000000000000";
                    //Сетевой номер в зависимости от типа ПУ
                    string netAdrs = RegPointRowViewModel.GetNetAddress(p.SerialNum, p.TypePU);
                    string formatNum = "";
                    for (int i = 0; i < netAdrs.Length; i++) formatNum += "0";
                    ws.Cell(n + 1, 8).Value = netAdrs;
                    ws.Cell(n + 1, 8).Style.NumberFormat.Format = formatNum;
                    //
                    ws.Cell(n + 1, 9).Value = p.PhoneNum;
                    ws.Cell(n + 1, 10).Value = (p.IsLinkOk) ? "Проверен" : "";
                    ws.Cell(n + 1, 11).Value = (p.IsAscueChecked) ? "Отправлен" : "";
                    ws.Cell(n + 1, 12).Value = (p.IsAscueOk) ? "Да" : "";
                    ws.Cell(n + 1, 13).Value = (p.Status == RegPointStatus.Demounted) ? "Демонтирован" : "";
                    //Красим
                    if (p.IsLinkOk) { ws.Range(ws.Cell(n + 1, 1), ws.Cell(n + 1, 12)).Style.Fill.BackgroundColor = XLColor.LightYellow; }
                    if (p.IsAscueChecked) { ws.Range(ws.Cell(n + 1, 1), ws.Cell(n + 1, 12)).Style.Fill.BackgroundColor = XLColor.LightBlue; }
                    if (p.IsAscueOk) { ws.Range(ws.Cell(n + 1, 1), ws.Cell(n + 1, 12)).Style.Fill.BackgroundColor = XLColor.LightGreen; }
                    n++;
                }
            
                // пример создания сетки в диапазоне
                var rngTable = ws.Range("A1:L" + n);
                rngTable.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                rngTable.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Columns().AdjustToContents(); //ширина столбца по содержимому
            }
            //Вернем файл в виде массива байт
            using (MemoryStream stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                return stream.ToArray();
            }
        }

        /// <summary>
        /// Реестр потребителей с договорами для ЭСКБ
        /// </summary>
        /// <param name="substation">Подстанция по которой нужна инфа</param>
        /// <returns></returns>
        public byte[] ConsumersInESKBFile(Substation substation)
        {
            var workbook = new XLWorkbook();
            string fileName = "notFound";
            if (substation == null) return null;

            fileName = substation.Name + "_" + DateTime.Now.ToShortDateString();
            var ws = workbook.Worksheets.Add(fileName);

            //создадим заголовки у столбцов
            ws.Cell(7, 1).Value = "№ п/п";
            ws.Cell(7, 2).Value = "РЭС";
            ws.Cell(7, 3).Value = "ТП/РП";
            ws.Cell(7, 4).Value = "Адрес";
            ws.Cell(7, 5).Value = "Тип ПУ";
            ws.Cell(7, 6).Value = "Серийный номер";
            ws.Cell(7, 7).Value = "ФИО потребителя";
            ws.Cell(7, 8).Value = "№ договора на электроснабжение";
            ws.Cell(7, 9).Value = "№ лицевого счета";
            ws.Cell(7, 10).Value = "№ прибора учета (старый)";
            ws.Range(ws.Cell(7, 1), ws.Cell(7, 10)).Style.Font.Bold = true;
            ws.Range(ws.Cell(7, 7), ws.Cell(7, 10)).Style.Alignment.WrapText = true;
            //Заполним список точек
            using (StoreContext db = new StoreContext())
            {
                var points = from p in db.RegPoints
                             join l in db.Letters on p.Id equals l.RegPointId
                             join d in db.Devices on p.DeviceId equals d.Id
                             where p.SubstationId == substation.Id && l.Printed == true && p.Consumer.Name != "нет договора" 
                             && p.Device.DeviceTypeId != 5 && p.Device.DeviceTypeId != 6 && p.Status == 0
                             group d by new
                             {
                                 Name = p.Substation.NetRegion.Name,
                                 Substation = p.Substation.Name,
                                 p.Consumer.O_Local,
                                 p.Consumer.O_Local_Secondary,
                                 p.Consumer.O_Street,
                                 p.Consumer.O_House,
                                 p.Consumer.O_Build,
                                 p.Consumer.O_Flat,
                                 TypePU = d.DeviceType.Name,
                                 p.Device.SerialNumber,
                                 ConsumerName = p.Consumer.Name,
                                 p.Consumer.ContractNumber,
                                 p.InstallAct.Uninstalled_Serial,
                                 ModelPU = p.Device.DeviceType.Name,
                                 d.DeviceType.Description
                             }
                             into g
                             select new
                             {
                                 Region = g.Key.Name,
                                 Substation = g.Key.Substation,
                                 Address = RegPointRowViewModel.FormatAddress(
                                            g.Key.O_Local,
                                            g.Key.O_Local_Secondary,
                                            g.Key.O_Street,
                                            g.Key.O_House,
                                            g.Key.O_Build,
                                            g.Key.O_Flat
                                            ),
                                 TypePU = g.Key.TypePU,
                                 SerialNum = g.Key.SerialNumber,
                                 ConsumerName = g.Key.ConsumerName,
                                 ContractNumber = g.Key.ContractNumber,
                                 PersonalAccount = g.Key.ContractNumber,
                                 Uninstalled_Serial = g.Key.Uninstalled_Serial,
                                 ModelPU = g.Key.ModelPU,
                                 Count1 = g.Count(x => x.DeviceType.Description == "1ф PLC") + g.Count(x => x.DeviceType.Description == "1ф GSM"),
                                 Count3 = g.Count(x => x.DeviceType.Description == "3ф PLC") + g.Count(x => x.DeviceType.Description == "3ф GSM"),
                                 Count = g.Count(x => x.DeviceType.Description == "1ф PLC") + g.Count(x => x.DeviceType.Description == "1ф GSM") + g.Count(x => x.DeviceType.Description == "3ф PLC") + g.Count(x => x.DeviceType.Description == "3ф GSM")
                             };

                int n = 7;
                int cnt = 0;
                int PU = 0;
                int PU_1 = 0;
                int PU_3 = 0;
                foreach (var p in points)
                {
                    PU = PU + p.Count;
                    PU_1 = PU_1 + p.Count1;
                    PU_3 = PU_3 + p.Count3;
                    ws.Cell(1, 2).Value = PU_1;
                    ws.Cell(2, 2).Value = PU_3;
                    ws.Cell(3, 2).Value = PU;
                    ws.Cell(n + 1, 1).Value = n - 6;
                    ws.Cell(n + 1, 2).Value = p.Region;
                    ws.Cell(n + 1, 3).Value = p.Substation;
                    ws.Cell(n + 1, 4).Value = p.Address;
                    ws.Cell(n + 1, 5).Value = p.TypePU;
                    ws.Cell(n + 1, 6).Value = p.SerialNum;
                    ws.Cell(n + 1, 6).Style.NumberFormat.Format = "000000000000000";
                    ws.Cell(n + 1, 7).Value = p.ConsumerName;
                    ws.Cell(n + 1, 8).Value = p.ContractNumber;
                    ws.Cell(n + 1, 8).Style.NumberFormat.Format = "000000000000";
                    ws.Cell(n + 1, 9).Value = p.PersonalAccount;
                    ws.Cell(n + 1, 9).Style.NumberFormat.Format = "000000000000";
                    ws.Cell(n + 1, 10).Value = p.Uninstalled_Serial;
                    n++;
                    cnt++;
                }

                // пример создания сетки в диапазоне
                var rngTable = ws.Range("A7:J" + n);
                rngTable.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                rngTable.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                rngTable.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                rngTable.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                rngTable.Style.Font.FontFamilyNumbering = XLFontFamilyNumberingValues.Roman;
                rngTable.Style.Font.FontSize = 11;
                ws.Columns().AdjustToContents(); //ширина столбца по содержимому
                ws.Worksheet.Name = "Итого " + cnt + " шт";
            }
            //создадим шапку
            ws.Range(ws.Cell(1, 10), ws.Cell(3, 10)).Style.Font.FontSize = 11;
            ws.Cell(1, 1).Value = "Итого 1ф:";
            ws.Cell(2, 1).Value = "Итого 3ф:";
            ws.Cell(3, 1).Value = "Итого:";
            var rng = ws.Range("A1:B3");
            rng.Style.Border.RightBorder = XLBorderStyleValues.Thin;
            rng.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            rng.Style.Border.TopBorder = XLBorderStyleValues.Thin;
            rng.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            rng.Style.Font.FontFamilyNumbering = XLFontFamilyNumberingValues.Roman;
            rng.Style.Font.FontSize = 11;
            ws.Cell(1, 2).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Cell(2, 2).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Cell(3, 2).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Columns(1, 1).AdjustToContents();
            ws.Cell(1, 5).Value = "Приложение";
            ws.Cell(1, 5).Style.Font.Bold = true;
            ws.Range("E1:J1").Merge();
            ws.Cell(1, 5).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
            ws.Cell(2, 5).Value = "к Письму №___________ от ____________________г.";
            ws.Cell(2, 5).Style.Font.Bold = true;
            ws.Range("E2:J2").Merge();
            ws.Cell(2, 5).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
            ws.Cell(3, 5).Value = "Протокол совещания №1 от 02.07.2019г. по допуску приборов учёта в эксплуатацию";
            ws.Range("E3:J3").Merge();
            ws.Cell(3, 5).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
            ws.Cell(5, 1).Value = "Реестр потребителей с договорами на электроснабжение с ООО «ЭСКБ»";
            ws.Cell(5, 1).Style.Font.FontSize = 14;
            ws.Cell(5, 1).Style.Font.Bold = true;
            ws.Range("A5:J5").Merge();
            ws.Cell(5, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Columns(1, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Range("A7:J7").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Range("A7:J7").Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            //Вернем файл в виде массива байт
            using (MemoryStream stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                return stream.ToArray();
            }
        }

        /// <summary>
        /// Реестр потребителей с договорами для ЭСКБ
        /// </summary>
        /// <param name="substation">Подстанция по которой нужна инфа</param>
        /// <returns></returns>
        public byte[] NoConsumersInESKBFile(Substation substation)
        {
            var workbook = new XLWorkbook();
            string fileName = "notFound";
            if (substation == null) return null;

            fileName = substation.Name + "_" + DateTime.Now.ToShortDateString();
            var ws = workbook.Worksheets.Add(fileName);

            //создадим заголовки у столбцов
            ws.Cell(7, 1).Value = "№ п/п";
            ws.Cell(7, 2).Value = "РЭС";
            ws.Cell(7, 3).Value = "ТП/РП";
            ws.Cell(7, 4).Value = "Адрес";
            ws.Cell(7, 5).Value = "Тип ПУ";
            ws.Cell(7, 6).Value = "Серийный номер";
            ws.Cell(7, 7).Value = "ФИО потребителя";
            ws.Cell(7, 8).Value = "№ договора на электроснабжение";
            ws.Cell(7, 9).Value = "№ лицевого счета";
            ws.Cell(7, 10).Value = "№ прибора учета (старый)";
            ws.Range(ws.Cell(7, 1), ws.Cell(7, 10)).Style.Font.Bold = true;
            ws.Range(ws.Cell(7, 7), ws.Cell(7, 10)).Style.Alignment.WrapText = true;
            //Заполним список точек
            using (StoreContext db = new StoreContext())
            {
                var points = from p in db.RegPoints
                             join d in db.Devices on p.DeviceId equals d.Id
                             where p.SubstationId == substation.Id && p.Device.DeviceTypeId != 6 && p.Status == 0
                             && (p.Consumer.Name == "нет договора" || p.Consumer.Name == "Ввод" || p.Consumer.Name == "Руб-к")
                             group d by new
                             {
                                 Name = p.Substation.NetRegion.Name,
                                 Substation = p.Substation.Name,
                                 p.Consumer.O_Local,
                                 p.Consumer.O_Local_Secondary,
                                 p.Consumer.O_Street,
                                 p.Consumer.O_House,
                                 p.Consumer.O_Build,
                                 p.Consumer.O_Flat,
                                 TypePU = d.DeviceType.Name,
                                 p.Device.SerialNumber,
                                 ConsumerName = p.Consumer.Name,
                                 p.Consumer.ContractNumber,
                                 p.InstallAct.Uninstalled_Serial,
                                 ModelPU = p.Device.DeviceType.Name,
                                 d.DeviceType.Description
                             }
                             into g
                             select new
                             {
                                 Region = g.Key.Name,
                                 Substation = g.Key.Substation,
                                 Address = RegPointRowViewModel.FormatAddress(
                                            g.Key.O_Local,
                                            g.Key.O_Local_Secondary,
                                            g.Key.O_Street,
                                            g.Key.O_House,
                                            g.Key.O_Build,
                                            g.Key.O_Flat
                                            ),
                                 TypePU = g.Key.TypePU,
                                 SerialNum = g.Key.SerialNumber,
                                 ConsumerName = g.Key.ConsumerName,
                                 ContractNumber = g.Key.ContractNumber,
                                 PersonalAccount = g.Key.ContractNumber,
                                 Uninstalled_Serial = g.Key.Uninstalled_Serial,
                                 ModelPU = g.Key.ModelPU,
                                 Count1 = g.Count(x => x.DeviceType.Description == "1ф PLC") + g.Count(x => x.DeviceType.Description == "1ф GSM"),
                                 Count3 = g.Count(x => x.DeviceType.Description == "3ф PLC") + g.Count(x => x.DeviceType.Description == "3ф GSM"),
                                 Count = g.Count(x => x.DeviceType.Description == "1ф PLC") + g.Count(x => x.DeviceType.Description == "1ф GSM") + g.Count(x => x.DeviceType.Description == "3ф PLC") + g.Count(x => x.DeviceType.Description == "3ф GSM") + g.Count(x => x.DeviceType.Description == "3ф ТТ")
                             };

                int n = 7;
                int cnt = 0;
                int PU = 0;
                int PU_1 = 0;
                int PU_3 = 0;
                foreach (var p in points)
                {
                    PU = PU + p.Count;
                    PU_1 = PU_1 + p.Count1;
                    PU_3 = PU_3 + p.Count3;
                    ws.Cell(1, 2).Value = PU_1;
                    ws.Cell(2, 2).Value = PU_3;
                    ws.Cell(3, 2).Value = PU;
                    ws.Cell(n + 1, 1).Value = n - 6;
                    ws.Cell(n + 1, 2).Value = p.Region;
                    ws.Cell(n + 1, 3).Value = p.Substation;
                    ws.Cell(n + 1, 4).Value = p.Address;
                    ws.Cell(n + 1, 5).Value = p.TypePU;
                    ws.Cell(n + 1, 6).Value = p.SerialNum;
                    ws.Cell(n + 1, 6).Style.NumberFormat.Format = "000000000000000";
                    ws.Cell(n + 1, 7).Value = p.ConsumerName;
                    ws.Cell(n + 1, 8).Value = "не заключен/отсутствует";
                    ws.Cell(n + 1, 9).Value = "отсутствует";
                    ws.Cell(n + 1, 10).Value = "отсутствует";
                    n++;
                    cnt++;
                }

                // пример создания сетки в диапазоне
                var rngTable = ws.Range("A7:J" + n);
                rngTable.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                rngTable.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                rngTable.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                rngTable.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                rngTable.Style.Font.FontFamilyNumbering = XLFontFamilyNumberingValues.Roman;
                rngTable.Style.Font.FontSize = 11;
                ws.Columns().AdjustToContents(); //ширина столбца по содержимому
                ws.Worksheet.Name = "Итого " + cnt + " шт";
            }
            //создадим шапку
            ws.Range(ws.Cell(1, 10), ws.Cell(3, 10)).Style.Font.FontSize = 11;
            ws.Cell(1, 1).Value = "Итого 1ф:";
            ws.Cell(2, 1).Value = "Итого 3ф:";
            ws.Cell(3, 1).Value = "Итого:";
            var rng = ws.Range("A1:B3");
            rng.Style.Border.RightBorder = XLBorderStyleValues.Thin;
            rng.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            rng.Style.Border.TopBorder = XLBorderStyleValues.Thin;
            rng.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            rng.Style.Font.FontFamilyNumbering = XLFontFamilyNumberingValues.Roman;
            rng.Style.Font.FontSize = 11;
            ws.Cell(1, 2).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Cell(2, 2).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Cell(3, 2).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Columns(1, 1).AdjustToContents();
            ws.Cell(1, 5).Value = "Приложение";
            ws.Cell(1, 5).Style.Font.Bold = true;
            ws.Range("E1:J1").Merge();
            ws.Cell(1, 5).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
            ws.Cell(2, 5).Value = "к Письму №___________ от ____________________г.";
            ws.Cell(2, 5).Style.Font.Bold = true;
            ws.Range("E2:J2").Merge();
            ws.Cell(2, 5).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
            ws.Cell(3, 5).Value = "Протокол совещания №1 от 02.07.2019г. по допуску приборов учёта в эксплуатацию";
            ws.Range("E3:J3").Merge();
            ws.Cell(3, 5).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
            ws.Cell(5, 1).Value = "Реестр потребителей без договоров на электроснабжение с ООО «ЭСКБ»";
            ws.Cell(5, 1).Style.Font.FontSize = 14;
            ws.Cell(5, 1).Style.Font.Bold = true;
            ws.Range("A5:J5").Merge();
            ws.Cell(5, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Columns(1, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Range("A7:J7").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Range("A7:J7").Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            //Вернем файл в виде массива байт
            using (MemoryStream stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                return stream.ToArray();
            }
        }

        /// <summary>
        /// Excel список точек по всему контракту (максимально полная и общая информация)
        /// </summary>
        /// <param name="contractId">Id контракта</param>
        /// <returns></returns>
        public ExcelManagerFile PointsInContract(int contractId)
        {
            ExcelManagerFile file = new ExcelManagerFile();
            using (StoreContext db = new StoreContext())
            {
                var workbook = new XLWorkbook();
                //Данные
                var contract = db.Contracts.Find(contractId);
                if(contract != null)
                {
                    //Получаем данные
                    var points = (from p in db.RegPoints
                                  join pAction in db.RegPointActions on p.Id equals pAction.RegPointId
                                 join d in db.Devices on p.DeviceId equals d.Id into ds //ПУ
                                 from d in ds.DefaultIfEmpty()
                                 join dStates in db.DeviceStates on d.Id equals dStates.DeviceId into dss //Статусы ПУ
                                 from dStates in dss.DefaultIfEmpty()
                                 join linkD in db.Links on d.Id equals linkD.DeviceId into lDs  //Link привязанные к устройствам
                                 from linkD in lDs.DefaultIfEmpty()   
                                 join linkS in db.SubstationLinks on p.SubstationId equals linkS.SubstationId into lSs  //Link привязанные к подстанции
                                 from linkS in lSs.DefaultIfEmpty()   
                                 join letter in db.Letters on p.Id equals letter.RegPointId into ls
                                 from letter in ls.DefaultIfEmpty()
                                 where p.Status == RegPointStatus.Default || p.Status == RegPointStatus.Demounted
                                 where p.Substation.NetRegion.ContractId == contractId
                                    //let lastAct = db.RegPointActions.Where(a => a.RegPointId == p.Id).OrderBy(a => a.Date).LastOrDefault()
                                    //let firstAct = db.RegPointActions.Where(a => a.RegPointId == p.Id).OrderBy(a => a.Date).FirstOrDefault()
                                 select new
                                    {
                                        p.Id,
                                        p.Status,
                                        ActionDate = pAction.Date,
                                        RegionName = p.Substation.NetRegion.Name,
                                        p.InstallAct.Feeder,
                                        SubstationName = p.Substation.Name,
                                        p.InstallAct.Section,
                                        InstallPlace = p.InstallAct.InstallPlaceType.Name + p.InstallAct.InstallPlaceNumber,
                                        p.InstallAct.Line,
                                        ConsummerName = p.Consumer.Name,
                                        Address_U = RegPointRowViewModel.FormatAddress(
                                                        p.Consumer.U_Local,
                                                        p.Consumer.U_Local_Secondary,
                                                        p.Consumer.U_Street,
                                                        p.Consumer.U_House,
                                                        p.Consumer.U_Build,
                                                        p.Consumer.U_Flat
                                                        ),
                                        ModelPU = d.DeviceType.Name,
                                        DeviceStateDate = dStates.Date,
                                        //YearProducePU = (firtsDState == null) ? "-" : firtsDState.Date.Year.ToString(),
                                        d.SerialNumber,
                                        NetAdrs = RegPointRowViewModel.GetNetAddress(d.SerialNumber, d.DeviceType.Description),
                                        PhoneNumber = (d.DeviceType.Description.ToLower().Contains("plc")) ? linkS.PhoneNumber : linkD.PhoneNumber,
                                        p.InstallAct.Seal_RegDevice,
                                        p.InstallAct.Seal_KDE,
                                        p.InstallAct.Seal_AV1,
                                        p.InstallAct.Seal_AV2,
                                        p.InstallAct.Seal_KI,
                                        p.InstallAct.TT_Koefficient,
                                        p.InstallAct.TT_A_Serial,
                                        p.InstallAct.TT_B_Serial,
                                        p.InstallAct.TT_C_Serial,
                                        p.InstallAct.Seal_TT_A,
                                        p.InstallAct.Seal_TT_B,
                                        p.InstallAct.Seal_TT_C,
                                        DateLetter = letter.DateLetter.ToShortDateString(),
                                        //Flags
                                        p.RegPointFlags.IsLinkOk,
                                        //p.RegPointFlags.IsAscueChecked,
                                        p.RegPointFlags.IsAscueOk
                                    }).OrderBy(p => p.DeviceStateDate)
                                    .OrderBy(p => p.ActionDate)
                                    .GroupBy(p => p.Id)
                                    .ToList()
                                    .Select(p => new {
                                        Point = p.First(),
                                        DateCreate = p.First().ActionDate,
                                        DateChange = p.Last().ActionDate,
                                        YearProducePU = p.First().DeviceStateDate.Year,
                                        LastDateLetter = p.Last().DateLetter
                                    });

                    var gPoints = points.GroupBy(p => p.Point.RegionName);
                    foreach (var region in gPoints)
                    {
                        //Новый лист и шапка
                        var ws = workbook.Worksheets.Add(region.Key);
                        //Создадим заголовки у столбцов
                        int col = 1;
                        ws.Cell(1, col++).Value = "№";
                        ws.Cell(1, col++).Value = "РЭС";
                        ws.Cell(1, col++).Value = "ПС";
                        ws.Cell(1, col++).Value = "Фидер";
                        ws.Cell(1, col++).Value = "Диспетчерское наименование ТП/РП";
                        ws.Cell(1, col++).Value = "Секция (1/2)";
                        ws.Cell(1, col++).Value = "Место установки";
                        ws.Cell(1, col++).Value = "№ линии";
                        ws.Cell(1, col++).Value = "Потребитель (ФИО или наименование)";
                        ws.Cell(1, col++).Value = "Юр. адрес";
                        ws.Cell(1, col++).Value = "Модель ПУ";
                        ws.Cell(1, col++).Value = "Год выпуска";
                        ws.Cell(1, col++).Value = "Заводской номер";
                        ws.Cell(1, col++).Value = "Сетевой номер";
                        ws.Cell(1, col++).Value = "№ sim-карты";
                        ws.Cell(1, col++).Value = "№ пломбы";
                        ws.Cell(1, col++).Value = "№ пломбы КДЕ";
                        ws.Cell(1, col++).Value = "№ пломбы АВ1";
                        ws.Cell(1, col++).Value = "№ пломбы АВ2";
                        ws.Cell(1, col++).Value = "№ пломбы КИ-10";
                        ws.Cell(1, col++).Value = "Ктт";
                        ws.Cell(1, col++).Value = "Зав. № ТТ фазы A";
                        ws.Cell(1, col++).Value = "Зав. № ТТ фазы B";
                        ws.Cell(1, col++).Value = "Зав. № ТТ фазы C";
                        ws.Cell(1, col++).Value = "№ пломбы ф.A";
                        ws.Cell(1, col++).Value = "№ пломбы ф.B";
                        ws.Cell(1, col++).Value = "№ пломбы ф.C";
                        ws.Cell(1, col++).Value = "Номер акта допуска";
                        ws.Cell(1, col++).Value = "Уведомление на допуск направлено Потребителю(да/нет)";
                        ws.Cell(1, col++).Value = "Акт допуска оформлен (дата)";
                        ws.Cell(1, col++).Value = "Дата изменения";
                        ws.Cell(1, col++).Value = "Связь проверена";
                        ws.Cell(1, col++).Value = "Принято в ПО АИИСКУЭ";
                        ws.Range(ws.Cell(1, 1), ws.Cell(1, col-1)).Style.Font.Bold = true;
                        ws.Range(ws.Cell(1, 1), ws.Cell(1, col-1)).Style.Fill.BackgroundColor = XLColor.LightGray;

                        //Заполняем строки
                        int n = 1;
                        foreach (var p in region)
                        {
                            col = 1;
                            ws.Cell(n + 1, col++).Value = n;
                            ws.Cell(n + 1, col++).Value = p.Point.RegionName; //РЭС
                            ws.Cell(n + 1, col++).Value = ""; //ПС
                            ws.Cell(n + 1, col++).Value = p.Point.Feeder; //"Фидер";
                            ws.Cell(n + 1, col++).Value = p.Point.SubstationName; //"Диспетчерское наименование ТП/РП";
                            ws.Cell(n + 1, col++).Value = p.Point.Section; //"Секция (1/2)";
                            ws.Cell(n + 1, col++).Value = p.Point.InstallPlace;//Место установки";
                            ws.Cell(n + 1, col++).Value = p.Point.Line;//№ линии";
                            ws.Cell(n + 1, col++).Value = p.Point.ConsummerName;//Потребитель (ФИО или наименование)";
                            ws.Cell(n + 1, col++).Value = p.Point.Address_U;//Юр. адрес";
                            ws.Cell(n + 1, col++).Value = p.Point.ModelPU;//Модель ПУ";
                            ws.Cell(n + 1, col++).Value = p.YearProducePU;
                            ws.Cell(n + 1, col).Value = p.Point.SerialNumber;//Заводской номер";
                            ws.Cell(n + 1, col++).Style.NumberFormat.Format = "000000000000000";
                            //Сетевой номер в зависимости от типа ПУ
                            ws.Cell(n + 1, col).Value = p.Point.NetAdrs;
                            ws.Cell(n + 1, col++).Style.NumberFormat.Format = new string('0', p.Point.NetAdrs.Length);
                            //
                            ws.Cell(n + 1, col++).Value = p.Point.PhoneNumber;//№ sim-карты";
                            ws.Cell(n + 1, col++).Value = p.Point.Seal_RegDevice;//№ пломбы";
                            ws.Cell(n + 1, col++).Value = p.Point.Seal_KDE;//№ пломбы КДЕ";
                            ws.Cell(n + 1, col++).Value = p.Point.Seal_AV1;//№ пломбы АВ1";
                            ws.Cell(n + 1, col++).Value = p.Point.Seal_AV2;//№ пломбы АВ2";
                            ws.Cell(n + 1, col++).Value = p.Point.Seal_KI;//№ пломбы КИ-10";
                            ws.Cell(n + 1, col++).Value = p.Point.TT_Koefficient;//Ктт";
                            ws.Cell(n + 1, col++).Value = p.Point.TT_A_Serial;//Зав. № ТТ фазы A";
                            ws.Cell(n + 1, col++).Value = p.Point.TT_B_Serial;//Зав. № ТТ фазы B";
                            ws.Cell(n + 1, col++).Value = p.Point.TT_C_Serial;//Зав. № ТТ фазы C";
                            ws.Cell(n + 1, col++).Value = p.Point.Seal_TT_A;//№ пломбы ф.A";
                            ws.Cell(n + 1, col++).Value = p.Point.Seal_TT_B;//№ пломбы ф.B";
                            ws.Cell(n + 1, col++).Value = p.Point.Seal_TT_C;//№ пломбы ф.C";
                            ws.Cell(n + 1, col++).Value = p.Point.Id;//Номер акта допуска";
                            ws.Cell(n + 1, col++).Value = p.LastDateLetter;//Уведомление на допуск направлено Потребителю(да/нет)";
                            ws.Cell(n + 1, col++).Value = p.DateCreate;//Акт допуска оформлен (дата)";
                            ws.Cell(n + 1, col++).Value = p.DateChange;//"Дата изменения";
                            ws.Cell(n + 1, col++).Value = (p.Point.IsLinkOk) ? "Да" : "";//"Связь проверена";
                            ws.Cell(n + 1, col++).Value = (p.Point.IsAscueOk) ? "Да" : "";//"Принято в ПО АИИСКУЭ";
                            if(p.Point.Status == RegPointStatus.Demounted)
                            {
                                ws.Cell(n + 1, col).Value = "Демонтирован";
                                var row_to_color = ws.Range(n + 1, 1, n + 1, col);
                                row_to_color.Style.Fill.BackgroundColor = XLColor.Red;
                            }
                            //Красим
                            //if (p.IsLinkOk) { ws.Range(ws.Cell(n + 1, 1), ws.Cell(n + 1, 12)).Style.Fill.BackgroundColor = XLColor.LightYellow; }
                            //if (p.IsAscueChecked) { ws.Range(ws.Cell(n + 1, 1), ws.Cell(n + 1, 12)).Style.Fill.BackgroundColor = XLColor.LightBlue; }
                            //if (p.IsAscueOk) { ws.Range(ws.Cell(n + 1, 1), ws.Cell(n + 1, 12)).Style.Fill.BackgroundColor = XLColor.LightGreen; }
                            n++;

                        }
                        // пример создания сетки в диапазоне
                        var rngTable = ws.Range(1, 1, n, col-1);
                        //var rngTable = ws.Range("A1:L" + n);
                        rngTable.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                        rngTable.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        ws.Columns().AdjustToContents(); //ширина столбца по содержимому
                    }
                    
                }
                
                //Вернем файл в виде массива байт
                MemoryStream stream = new MemoryStream();
                workbook.SaveAs(stream);
                file.Name = contract.Name + "_" + DateTime.Now.ToShortDateString() + ".xlsx";
                file.Data = stream.ToArray();
                return file;
            }
            
        }

        /// <summary>
        /// RegPoint в акт допуска
        /// </summary>
        /// <param name="regpointId">id точки учета</param>
        /// <returns></returns>
        public ExcelManagerFile Act(int regpointId)
        {
            //TODO проверить возможность одновременного исаользования шаблона несколькими пользоватлями
            var wb = new XLWorkbook(_env.WebRootPath + "/Files/ExcelTemplates/ActTemplate.xlsx"); //Открытие шаблона акта допуска
            var ws = wb.Worksheet(1); //Выбор листа
            ExcelManagerFile file = new ExcelManagerFile();
            using (StoreContext db = new StoreContext())
            {
                var rp = from p in db.RegPoints
                               where p.Id == regpointId
                         orderby p.Consumer.U_Local, p.Consumer.U_Street, p.Consumer.U_House, p.Consumer.U_Build, p.Consumer.U_Flat
                         select new
                               {
                                   point = p,
                                   p.InstallAct,
                                   p.Consumer,
                                   p.Device,
                                   p.Device.DeviceStates,
                                   p.Device.Links,
                                   p.Substation.SubstationLinks,
                                   p.Substation.NetRegion,
                                   p.Substation,
                                   letter = p.Letters.LastOrDefault()
                               };

                var regPoint = rp.First();

                if (regPoint != null) //Если ТУ есть
                {
                    //Потребитель
                    string fullAddress = RegPointRowViewModel.FormatOAddress(regPoint.Consumer); // полный адрес потребителя
                    ws.Cell("G3").Value = "'" + regPoint.Consumer.ContractNumber; //Номер контракта
                    if (regPoint.letter != null)
                        ws.Cell("B5").Value = regPoint.point.Id + "/" + regPoint.letter.DateLetter.Year.ToString().Substring(2, 2);
                    ws.Cell("J5").Value = regPoint.letter?.InviteDate.ToShortDateString();
                    ws.Cell("F6").Value = regPoint.Consumer.Name + ", договор " + regPoint.Consumer.ContractNumber; //ФИО потребителя
                    ws.Cell("E8").Value = fullAddress;
                    ws.Cell("F18").Value += " " + regPoint.NetRegion.Name;

                    //Прибор учета
                    ws.Cell("G25").Value = regPoint.InstallAct.InstallPlaceType.Name + " " + regPoint.InstallAct.InstallPlaceNumber + Environment.NewLine + regPoint.Substation.Name; //Место установки ПУ

                    //перетоки IsOutFlow - отдача
                    if (regPoint.InstallAct.IsOutFlow)
                        ws.Cell("G26").Value = "отдача";
                    else
                        ws.Cell("G26").Value = "прием";

                    ws.Cell("G28").Value = "'" + regPoint.Device.SerialNumber;   //Заводской номер ПУ
                    ws.Cell("G29").Value = regPoint.Device.DeviceType.Name; //Тип ПУ
                    ws.Cell("G29").Style.Font.FontSize = 6; //Размер шрифта
                    ws.Cell("G30").Value = regPoint.Device.DeviceType.EnergyType; //Тип измеряемой энергии
                    ws.Cell("G31").Value = regPoint.Device.DeviceType.INom; //Номинальгый ток
                    ws.Cell("G32").Value = regPoint.Device.DeviceType.UNom; //Номинальное напряжение
                    ws.Cell("G33").Value = "'" + regPoint.Device.DeviceType.AccuracyClass; //Класс точности

                    //TODO определится сдатой предыдущей госповерки
                    ws.Cell("G34").Value = regPoint.Device.DeviceStates.FirstOrDefault().Date.Year.ToString(); //Дата первого статуса device
                    ws.Cell("G35").Value = regPoint.Device.DeviceType.TestInterval; //Межповерочный интервал

                    //Показания ПУ 
                    //TODO определится с источником показаний ПУ, пока взял из Install act
                    //ws.Cell("G37").Value = "'" + regPoint.InstallAct.Tsum; //Сумма
                    //ws.Cell("G38").Value = "'" + regPoint.InstallAct.T1; //Т1
                    //ws.Cell("G39").Value = "'" + regPoint.InstallAct.T2; //Т2

                    // w//s.Cell("G37").
                    ws.Range("G28", "G39").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                    //Пломба ПУ
                    ws.Cell("G41").Value = "'" + regPoint.InstallAct.Seal_RegDevice;

                    //Link
                    if (regPoint.Device.DeviceType.Description.ToLower().Contains("plc")) //Если пу с plc модулем
                    {
                        ws.Cell("G46").Value = "Внешний GSM-модем";
                        var link = regPoint.SubstationLinks.LastOrDefault();
                        if (link != null)
                            ws.Cell("G47").Value = link.PhoneNumber;
                    }
                    else //Остальные ПУ
                    {
                        var link = regPoint.Device.Links.LastOrDefault();
                        if (link != null)
                        {
                            ws.Cell("G46").Value = link.LinkType;
                            ws.Cell("G47").Value = link.PhoneNumber;
                        }
                    }

                    //Снятый ПУ
                    if (regPoint.InstallAct.Uninstalled_Serial != "" && regPoint.InstallAct.Uninstalled_Serial != null && regPoint.InstallAct.Uninstalled_Serial != "норматив") //Если ПУ снят 
                    {
                        ws.Cell("I23").Value = "☐ установлен ☑ снят";
                        ws.Cell("I28").Value = "'" + regPoint.InstallAct.Uninstalled_Serial;
                    }

                    ws.Cell("D52").Value = "'" + regPoint.Device.SerialNumber;
                    if (regPoint.InstallAct.InstallPlaceType.Name.ToLower().Contains("ру")) //Если ПУ установлен 
                    {
                        ws.Cell("D53").Value = "☑ установлен ☐ снят";
                        // ТТ А
                        ws.Cell("D58").Value = "'" + regPoint.InstallAct.TT_A_Serial;
                        ws.Cell("D59").Value = "Т-0,66";
                        ws.Cell("D60").Value = "5";
                        ws.Cell("D61").Value = "0,5s";
                        ws.Cell("D62").Value = regPoint.InstallAct.TT_Koefficient;
                        ws.Cell("D63").Value = "1кв. 2019";
                        ws.Cell("D65").Value = "5";
                        ws.Cell("D66").Value = "'" + regPoint.InstallAct.Seal_TT_A;

                        // ТТ B
                        ws.Cell("E58").Value = "'" + regPoint.InstallAct.TT_B_Serial;
                        ws.Cell("E59").Value = "Т-0,66";
                        ws.Cell("E60").Value = "5";
                        ws.Cell("E61").Value = "0,5s";
                        ws.Cell("E62").Value = regPoint.InstallAct.TT_Koefficient;
                        ws.Cell("E63").Value = "1кв. 2019";
                        ws.Cell("E65").Value = "5";
                        ws.Cell("E66").Value = "'" + regPoint.InstallAct.Seal_TT_B;

                        // ТТ C
                        ws.Cell("F58").Value = "'" + regPoint.InstallAct.TT_C_Serial;
                        ws.Cell("F59").Value = "Т-0,66";
                        ws.Cell("F60").Value = "5";
                        ws.Cell("F61").Value = "0,5s";
                        ws.Cell("F62").Value = regPoint.InstallAct.TT_Koefficient;
                        ws.Cell("F63").Value = "1кв. 2019";
                        ws.Cell("F65").Value = "5";
                        ws.Cell("F66").Value = "'" + regPoint.InstallAct.Seal_TT_C;

                        //Пломба КИ-10
                        ws.Cell("D90").Value = "№ " + regPoint.InstallAct.Seal_KI;
                    }
                    else //Если ПУ не в ТП
                    {
                        ws.Cell("D86").Value = "№ " + regPoint.InstallAct.Seal_AV1; //Пломба автомата
                    }
                    ws.Cell("L105").Value = regPoint.letter.InviteDate.ToShortDateString();
                }
            }
            //Вернем файл в виде массива байт
            MemoryStream stream = new MemoryStream();
            wb.SaveAs(stream);
            file.Name = "Акт допуска " + regpointId + ".xlsx";
            file.Data = stream.ToArray();
            return file;
        }

        /// <summary>
        /// Генерация актов допуска в один файл по дате приглашения
        /// </summary>
        /// <param name="inviteDate"></param>
        /// <returns></returns>
        public ExcelManagerFile Acts(DateTime inviteDate)
        {
            var wb = new XLWorkbook(_env.WebRootPath + "/Files/ExcelTemplates/ActTemplate.xlsx"); //Открытие шаблона акта допуска
           // var ws = wb.Worksheet(1); //Выбор листа
            var actsBook = new XLWorkbook();
            ExcelManagerFile file = new ExcelManagerFile();
            using (StoreContext db = new StoreContext())
            {
                var rp = from p in db.RegPoints
                         join l in db.Letters on p.Id equals l.RegPointId
                         where l.InviteDate == inviteDate
                         orderby p.Consumer.U_Local, p.Consumer.U_Street, p.Consumer.U_House, p.Consumer.U_Build, p.Consumer.U_Flat
                         select new
                         {
                             point = p,
                             p.InstallAct,
                             p.Consumer,
                             p.Device,
                             p.Device.DeviceStates,
                             p.Device.Links,
                             p.Substation.SubstationLinks,
                             p.Substation.NetRegion,
                             p.Substation,
                             letter = p.Letters.LastOrDefault()
                         };
                var regPoints = rp.ToList();
                foreach(var regPoint in regPoints)
                {

                    var ws = wb.Worksheet(1).CopyTo(actsBook, regPoint.point.Id.ToString());

                    //Потребитель
                    string fullAddress = RegPointRowViewModel.FormatOAddress(regPoint.Consumer); // полный адрес потребителя
                    ws.Cell("G3").Value = "'" + regPoint.Consumer.ContractNumber; //Номер контракта
                    if (regPoint.letter != null)
                        ws.Cell("B5").Value = regPoint.point.Id + "/" + regPoint.letter.DateLetter.Year.ToString().Substring(2, 2);
                    ws.Cell("J5").Value = regPoint.letter.InviteDate.ToShortDateString();
                    ws.Cell("F6").Value = regPoint.Consumer.Name + ", договор " + regPoint.Consumer.ContractNumber; //ФИО потребителя
                    ws.Cell("E8").Value = fullAddress;
                    ws.Cell("F18").Value += " " + regPoint.NetRegion.Name;

                    //Прибор учета
                    ws.Cell("G25").Value = regPoint.InstallAct.InstallPlaceType.Name + " " + regPoint.InstallAct.InstallPlaceNumber + Environment.NewLine + regPoint.Substation.Name; //Место установки ПУ

                    //перетоки IsOutFlow - отдача
                    if (regPoint.InstallAct.IsOutFlow)
                        ws.Cell("G26").Value = "отдача";
                    else
                        ws.Cell("G26").Value = "прием";

                    ws.Cell("G28").Value = "'" + regPoint.Device.SerialNumber;   //Заводской номер ПУ
                    ws.Cell("G29").Value = regPoint.Device.DeviceType.Name; //Тип ПУ
                    ws.Cell("G29").Style.Font.FontSize = 6; //Размер шрифта
                    ws.Cell("G30").Value = regPoint.Device.DeviceType.EnergyType; //Тип измеряемой энергии
                    ws.Cell("G31").Value = regPoint.Device.DeviceType.INom; //Номинальгый ток
                    ws.Cell("G32").Value = regPoint.Device.DeviceType.UNom; //Номинальное напряжение
                    ws.Cell("G33").Value = "'" + regPoint.Device.DeviceType.AccuracyClass; //Класс точности

                    //TODO определится сдатой предыдущей госповерки
                    ws.Cell("G34").Value = regPoint.Device.DeviceStates.FirstOrDefault().Date.Year.ToString(); //Дата первого статуса device
                    ws.Cell("G35").Value = regPoint.Device.DeviceType.TestInterval; //Межповерочный интервал

                    //Показания ПУ 
                    //TODO определится с источником показаний ПУ, пока взял из Install act
                    //ws.Cell("G37").Value = "'" + regPoint.InstallAct.Tsum; //Сумма
                    //ws.Cell("G38").Value = "'" + regPoint.InstallAct.T1; //Т1
                    //ws.Cell("G39").Value = "'" + regPoint.InstallAct.T2; //Т2

                    ws.Range("G28", "G39").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                    //Пломба ПУ
                    ws.Cell("G41").Value = "'" + regPoint.InstallAct.Seal_RegDevice;

                    //Link
                    if (regPoint.Device.DeviceType.Description.ToLower().Contains("plc")) //Если пу с plc модулем
                    {
                        ws.Cell("G46").Value = "Внешний GSM-модем";
                        var link = regPoint.SubstationLinks.LastOrDefault();
                        if (link != null)
                            ws.Cell("G47").Value = link.PhoneNumber;
                    }
                    else //Остальные ПУ
                    {
                        var link = regPoint.Device.Links.LastOrDefault();
                        if (link != null)
                        {
                            ws.Cell("G46").Value = link.LinkType;
                            ws.Cell("G47").Value = link.PhoneNumber;
                        }
                    }

                    //Снятый ПУ
                    if (regPoint.InstallAct.Uninstalled_Serial != "" && regPoint.InstallAct.Uninstalled_Serial != null && regPoint.InstallAct.Uninstalled_Serial != "норматив") //Если ПУ снят 
                    {
                        ws.Cell("I23").Value = "☐ установлен ☑ снят";
                        ws.Cell("I28").Value = "'" + regPoint.InstallAct.Uninstalled_Serial;
                    }

                    ws.Cell("D52").Value = "'" + regPoint.Device.SerialNumber;
                    if (regPoint.InstallAct.InstallPlaceType.Name.ToLower().Contains("ру")) //Если ПУ установлен 
                    {
                        ws.Cell("D53").Value = "☑ установлен ☐ снят";
                        // ТТ А
                        ws.Cell("D58").Value = "'" + regPoint.InstallAct.TT_A_Serial;
                        ws.Cell("D59").Value = "Т-0,66";
                        ws.Cell("D60").Value = "5";
                        ws.Cell("D61").Value = "0,5s";
                        ws.Cell("D62").Value = regPoint.InstallAct.TT_Koefficient;
                        ws.Cell("D63").Value = "1кв. 2019";
                        ws.Cell("D65").Value = "5";
                        ws.Cell("D66").Value = "'" + regPoint.InstallAct.Seal_TT_A;

                        // ТТ B
                        ws.Cell("E58").Value = "'" + regPoint.InstallAct.TT_B_Serial;
                        ws.Cell("E59").Value = "Т-0,66";
                        ws.Cell("E60").Value = "5";
                        ws.Cell("E61").Value = "0,5s";
                        ws.Cell("E62").Value = regPoint.InstallAct.TT_Koefficient;
                        ws.Cell("E63").Value = "1кв. 2019";
                        ws.Cell("E65").Value = "5";
                        ws.Cell("E66").Value = "'" + regPoint.InstallAct.Seal_TT_B;

                        // ТТ C
                        ws.Cell("F58").Value = "'" + regPoint.InstallAct.TT_C_Serial;
                        ws.Cell("F59").Value = "Т-0,66";
                        ws.Cell("F60").Value = "5";
                        ws.Cell("F61").Value = "0,5s";
                        ws.Cell("F62").Value = regPoint.InstallAct.TT_Koefficient;
                        ws.Cell("F63").Value = "1кв. 2019";
                        ws.Cell("F65").Value = "5";
                        ws.Cell("F66").Value = "'" + regPoint.InstallAct.Seal_TT_C;

                        //Пломба КИ-10
                        ws.Cell("D90").Value = "№ " + regPoint.InstallAct.Seal_KI;
                    }
                    else //Если ПУ не в ТП
                    {
                        ws.Cell("D86").Value = "№ " + regPoint.InstallAct.Seal_AV1; //Пломба автомата
                    }
                    ws.Cell("L105").Value = regPoint.letter.InviteDate.ToShortDateString();

                }
            }
            //Вернем файл в виде массива байт
            MemoryStream stream = new MemoryStream();
            actsBook.SaveAs(stream);
            file.Name = "Акты допуска " + inviteDate.ToShortDateString() + ".xlsx";
            file.Data = stream.ToArray();
            return file;
        }

        /// <summary>
        /// Генерация писем по дате приглашения
        /// </summary>
        /// <param name="inviteDate">Дата приглашения на допуск в формате '03.05.2019'</param>
        /// <returns></returns>
        public ExcelManagerFile Letters(DateTime inviteDate)
        {
            ExcelManagerFile file = new ExcelManagerFile();
            var templateBook = new XLWorkbook(_env.WebRootPath + "/Files/ExcelTemplates/LetterTemplate.xlsx"); //Открытие шаблона акта допуска
            var template = templateBook.Worksheet(1); //Выбор листа
            var lettersBook = new XLWorkbook();
            using (StoreContext db = new StoreContext())
            {
                var data = from d in db.Letters
                           where d.InviteDate.Day == inviteDate.Day && d.InviteDate.Month == inviteDate.Month && d.InviteDate.Year == inviteDate.Year
                           orderby d.RegPoint.Consumer.Name
                           select new
                           {
                               d,
                               d.RegPoint,
                               d.RegPoint.Consumer,
                               d.RegPoint.Device,
                               d.RegPoint.Device.DeviceType,
                               d.RegPoint.Device.DeviceStates,
                               d.RegPoint.Substation.NetRegion
                           };

                foreach (var letter in data)
                {
                    if ( letter.d != null)
                    {
                        var ls = templateBook.Worksheet(1).CopyTo(lettersBook, letter.d.OutNumber.Replace('/', '.')); //Копирование шаблона письма
                        var nextTestDate = letter.DeviceStates.First().Date.AddYears(letter.DeviceType.TestInterval);
                        string fullAddress = RegPointRowViewModel.FormatOAddress(letter.Consumer); // полный адрес потребителя
                        ls.Cell("A11").Value = letter.d.DateLetter.ToShortDateString() + " №" + letter.d.OutNumber; //номер пиьсма
                        ls.Cell("F11").Value = letter.Consumer.Name; //ФИО Потребителя
                        ls.Cell("F12").Value = fullAddress;
                        ls.Cell("A22").Value += fullAddress + " на допуске прибора учета в эксплуатацию установленного на опор ВЛ-0,4 кВ в отношении Потребителя: " + letter.Consumer.Name;
                        ls.Cell("C28").Value = letter.d.InviteDate; //Дата приглашения
                        ls.Cell("C29").Value = "Тип прибора учета:" + letter.DeviceType.Name + ", класс точности: " + letter.DeviceType.AccuracyClass + ", срок очередной поверки: " + nextTestDate.Year.ToString() + " г.";
                        ls.Cell("C29").Style.Alignment.WrapText = true; //Перенос текста в ячейке
                        ls.Cell("A49").Value = "Исп. " + "Артамонова Альбина Венеровна";//letter.NetRegion.ContactName;
                        ls.Cell("A50").Value = "Тел. " + "8-987-480-32-67";//letter.NetRegion.PhoneNumber;
                    }
                }
                MemoryStream stream = new MemoryStream();
                lettersBook.SaveAs(stream);
                file.Name = "Письма " + inviteDate.ToShortDateString() + ".xlsx";
                file.Data = stream.ToArray();
                return file;
            }
        }
        
        /// <summary>
        /// Генерация одного письма по Id
        /// </summary>
        /// <param name="letterId">Id письма</param>
        /// <returns></returns>
        public ExcelManagerFile Letter(int letterId)
        {
            ExcelManagerFile file = new ExcelManagerFile();
            var templateBook = new XLWorkbook(_env.WebRootPath + "/Files/ExcelTemplates/LetterTemplate.xlsx"); //Открытие шаблона акта допуска
            var template = templateBook.Worksheet(1); //Выбор листа
            var lettersBook = new XLWorkbook();
            using (StoreContext db = new StoreContext())
            {
                var data = from d in db.Letters
                           where d.Id == letterId
                           select new
                           {
                               d,
                               d.RegPoint,
                               d.RegPoint.Consumer,
                               d.RegPoint.Device,
                               d.RegPoint.Device.DeviceType,
                               d.RegPoint.Device.DeviceStates,
                               d.RegPoint.Substation.NetRegion
                           };

                var letter = data.First();
                if (letter.d != null)
                {
                    var ls = templateBook.Worksheet(1).CopyTo(lettersBook, letter.d.OutNumber.Replace('/', '.')); //Копирование шаблона письма
                    var nextTestDate = letter.DeviceStates.First().Date.AddYears(letter.DeviceType.TestInterval);
                    string fullAddress = RegPointRowViewModel.FormatOAddress(letter.Consumer); // полный адрес потребителя
                    ls.Cell("A11").Value = letter.d.DateLetter.ToShortDateString() + " №" + letter.d.OutNumber; //номер пиьсма
                    ls.Cell("F11").Value = letter.Consumer.Name; //ФИО Потребителя
                    ls.Cell("F12").Value = fullAddress;
                    ls.Cell("A22").Value += fullAddress + " на допуске прибора учета в эксплуатацию установленного на опор ВЛ-0,4 кВ в отношении Потребителя: " + letter.Consumer.Name;
                    ls.Cell("C28").Value = letter.d.InviteDate; //Дата приглашения
                    ls.Cell("C29").Value = "Тип прибора учета:" + letter.DeviceType.Name + ", класс точности: " + letter.DeviceType.AccuracyClass + ", срок очередной поверки: " + nextTestDate.Year.ToString() + " г.";
                    ls.Cell("C29").Style.Alignment.WrapText = true; //Перенос текста в ячейке
                    ls.Cell("A49").Value = "Исп. " + "Артамонова Альбина Венеровна";//letter.NetRegion.ContactName;
                    ls.Cell("A50").Value = "Тел. " + "8-987-480-32-67";//letter.NetRegion.PhoneNumber;
                }
                
                MemoryStream stream = new MemoryStream();
                lettersBook.SaveAs(stream);
                file.Name = "Письмо №" + letter.d.OutNumber + ".xlsx";
                file.Data = stream.ToArray();
                return file;
            }
        }

        /// <summary>
        /// Реестр писем по дате приглашения для загрузки на сайт Почты Росии
        /// </summary>
        /// <param name="inviteDate">Дата приглашения на допуск</param>
        /// <returns></returns>
        public ExcelManagerFile LettersReestrToPostSite(DateTime inviteDate)
        {
            ExcelManagerFile file = new ExcelManagerFile();
            var workBook = new XLWorkbook(); //Открытие шаблона акта допуска
            var ws = workBook.Worksheets.Add("letters"); //Выбор листа
            //создадим заголовки у столбцов
            ws.Cell(1, 1).Value = "ADDRESSLINE";
            ws.Cell(1, 2).Value = "ADRESAT";
            ws.Cell(1, 3).Value = "MASS";
            ws.Cell(1, 23).Value = "NORETURN";

            using (StoreContext db = new StoreContext())
            {
                var data = from l in db.Letters
                           where l.InviteDate.Day == inviteDate.Day && l.InviteDate.Month == inviteDate.Month && l.InviteDate.Year == inviteDate.Year
                           orderby l.RegPoint.Consumer.Name
                           select new
                           {
                               l.RegPoint.Consumer.U_Index,
                               ADDRESSLINE = RegPointRowViewModel.FormatUAddress(l.RegPoint.Consumer),
                               ADRESAT = l.RegPoint.Consumer.Name + " " + l.RegPointId,
                               MASS = 0.02,
                               NORETURN = 0
                           };

                int row = 1;
                foreach (var item in data.ToList())
                {
                    row++;
                    if(string.IsNullOrEmpty(item.U_Index))
                        ws.Cell(row, 1).Value = "Башкортостан Респуб, " + item.ADDRESSLINE;
                    else
                        ws.Cell(row, 1).Value = item.U_Index + ", Башкортостан Респуб, " + item.ADDRESSLINE;
                    ws.Cell(row, 2).Value = item.ADRESAT;
                    ws.Cell(row, 3).Value = item.MASS;
                    ws.Cell(row, 23).Value = item.NORETURN;
                }
                ws.Columns().AdjustToContents(); //ширина столбца по содержимому

                MemoryStream stream = new MemoryStream();
                workBook.SaveAs(stream);
                file.Name = "Реестр для сайта ПОЧТЫ РОССИИ " + inviteDate.ToShortDateString() + ".xlsx";
                file.Data = stream.ToArray();
                return file;
            }
        }

        /// <summary>
        /// Excel список точек учета запланированых на допуск по дате
        /// </summary>
        /// <param name="inviteDate">Дата приглашения на допуск в формате '03.05.2019'</param>
        /// <returns></returns>
        public ExcelManagerFile RequestReestrToInsertPu(DateTime inviteDate)
        {
            ExcelManagerFile file = new ExcelManagerFile();
            var templateBook = new XLWorkbook(_env.WebRootPath + "/Files/ExcelTemplates/RequestToInsertPuOnDateTemplate.xltx"); //Открытие шаблона заявки/реестра на допуск
            var template = templateBook.Worksheet(1);
            //Копирование шаблона письма
            var lettersBook = new XLWorkbook();
            var ls = template.CopyTo(lettersBook, inviteDate.ToShortDateString());
            using (StoreContext db = new StoreContext())
            {
                var data = from l in db.Letters
                           join rp in db.RegPoints on l.RegPointId equals rp.Id
                           join consumer in db.Consumers on rp.Id equals consumer.RegPointId
                           join d in db.Devices on rp.DeviceId equals d.Id into ds
                           from d in ds.DefaultIfEmpty()
                           join dst in db.DeviceStates on d.Id equals dst.DeviceId
                           where l.InviteDate.Day == inviteDate.Day && l.InviteDate.Month == inviteDate.Month && l.InviteDate.Year == inviteDate.Year
                           orderby dst.Date
                           select new
                           {
                               l.Id,
                               ConsumerName = consumer.Name + " " + rp.Id,
                               UAddress = RegPointRowViewModel.FormatUAddress(consumer),
                               OAddress = RegPointRowViewModel.FormatOAddress(consumer),
                               NumEnergoContract = consumer.ContractNumber,
                               DeviceModel = d.DeviceType.Name,
                               DeviceStateDate = dst.Date
                           };
                var list_g = data.GroupBy(l => l.Id).Select(g => g.First()).ToList();
                var list = list_g.OrderBy(consumer => consumer.ConsumerName).ToList();

                //Шапка
                ls.Cell(1, 1).SetValue(DateTime.Now.ToString("от dd MMMM yyyyг."));
                ls.Cell(1, 4).SetValue("№" + inviteDate.ToShortDateString());

                int row = 3;
                foreach (var item in list)
                {
                    row++;
                    ls.Cell(row, 1).Value = row - 3;
                    ls.Cell(row, 2).Value = item.ConsumerName; //ФИО Потребителя
                    ls.Cell(row, 3).Value = item.UAddress;
                    ls.Cell(row, 4).Value = item.OAddress;
                    ls.Cell(row, 5).Value = "'" + item.NumEnergoContract;
                    ls.Cell(row, 6).Value = inviteDate.ToShortDateString();
                    ls.Cell(row, 7).Value =
                        "Тип: " + item.DeviceModel + Environment.NewLine +
                        "Срок очередной поверки: " + (item.DeviceStateDate.Year + 16).ToString();

                    ls.Cell(row, 2).Style.Alignment.WrapText = true;
                    ls.Cell(row, 3).Style.Alignment.WrapText = true;
                    ls.Cell(row, 4).Style.Alignment.WrapText = true;
                    ls.Cell(row, 5).Style.Alignment.WrapText = true;
                    ls.Cell(row, 6).Style.Alignment.WrapText = true;
                    ls.Cell(row, 7).Style.Alignment.WrapText = true;
                }
                // пример создания сетки в диапазоне
                var rngTable = ls.Range(4, 1, row, 7);
                rngTable.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                rngTable.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                rngTable.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                rngTable.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                rngTable.Style.Font.FontFamilyNumbering = XLFontFamilyNumberingValues.Roman;
                rngTable.Style
                  .Alignment.SetVertical(XLAlignmentVerticalValues.Top)
                  .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                MemoryStream stream = new MemoryStream();
                lettersBook.SaveAs(stream);
                file.Name = "Заявка на допуск №" + inviteDate.ToShortDateString() + ".xlsx";
                file.Data = stream.ToArray();
                return file;
            }
        }

        /// <summary>
        /// Excel список точек учета запланированых на допуск по дате
        /// </summary>
        /// <param name="inviteDate">Дата приглашения на допуск в формате '03.05.2019'</param>
        /// <returns></returns>
        public ExcelManagerFile TransferredReestrActsAdmission(DateTime inviteDate)
        {
            ExcelManagerFile file = new ExcelManagerFile();
            var workBook = new XLWorkbook(); //Открытие шаблона акта допуска
            var ws = workBook.Worksheets.Add("reestr"); //Выбор листа
            //создадим заголовки у столбцов
            ws.Cell(4, 1).Value = "№ п/п";
            ws.Cell(4, 2).Value = "РЭС";
            ws.Cell(4, 3).Value = "ТП";
            ws.Cell(4, 4).Value = "1ф";
            ws.Cell(4, 5).Value = "3ф";
            ws.Cell(4, 6).Value = "Итого";
            using (StoreContext db = new StoreContext())
            {
                var data = from points in db.RegPoints
                           join substations in db.Substations on points.SubstationId equals substations.Id
                           join regions in db.NetRegions on substations.NetRegionId equals regions.Id
                           join devices in db.Devices on points.DeviceId equals devices.Id
                           join devicetype in db.DeviceTypes on devices.DeviceTypeId equals devicetype.Id
                           join letters in db.Letters on points.Id equals letters.RegPointId
                           where letters.Printed == true && letters.InviteDate.Day == inviteDate.Day && letters.InviteDate.Month == inviteDate.Month && letters.InviteDate.Year == inviteDate.Year
                           group devices by new
                           {
                               Regions = regions.Name,
                               Substations = substations.Name,
                               devicetype.Description
                           }
                    into g
                    select new
                    {
                        Name = g.Key,
                        Region = g.Key.Regions,
                        Substation = g.Key.Substations,
                        Count1 = g.Count(x => x.DeviceType.Description == "1ф PLC") + g.Count(x => x.DeviceType.Description == "1ф GSM"),
                        Count3 = g.Count(x => x.DeviceType.Description == "3ф PLC") + g.Count(x => x.DeviceType.Description == "3ф GSM"),
                        Count = g.Count(x => x.DeviceType.Description == "1ф PLC") + g.Count(x => x.DeviceType.Description == "1ф GSM") + g.Count(x => x.DeviceType.Description == "3ф PLC") + g.Count(x => x.DeviceType.Description == "3ф GSM")
                    };
                var result = data.ToList();

                //Шапка
                ws.Cell(1, 1).Value = "Реестр переданных актов допуска";
                ws.Cell(1, 5).SetValue(DateTime.Now.ToString("от dd MMMM yyyyг."));
                ws.Cell(2, 1).SetValue("№" + inviteDate.ToShortDateString());

                int row = 4;
                int PU = 0;
                int PU_1 = 0;
                int PU_3 = 0;
                string Name = "";
                foreach (var item in result)
                {
                    //Если наименование ТП совпадает, то суммируем результат
                    if (item.Substation == Name)
                    {
                        PU_1 = PU_1 + item.Count1;
                        PU_3 = PU_3 + item.Count3;
                        PU = PU + item.Count;
                        ws.Cell(row, 1).Value = row - 4;
                        ws.Cell(row, 2).Value = item.Region;
                        ws.Cell(row, 3).Value = item.Substation;
                        ws.Cell(row, 4).Value = PU_1;
                        ws.Cell(row, 5).Value = PU_3;
                        ws.Cell(row, 6).Value = PU;
                    }
                    else
                    {
                        PU_1 = item.Count1;
                        PU_3 = item.Count3;
                        PU = item.Count;
                        row++;
                        ws.Cell(row, 1).Value = row - 4;
                        ws.Cell(row, 2).Value = item.Region;
                        ws.Cell(row, 3).Value = item.Substation;
                        ws.Cell(row, 4).Value = PU_1;
                        ws.Cell(row, 5).Value = PU_3;
                        ws.Cell(row, 6).Value = PU;
                    }
                    ws.Cell(row, 2).Style.Alignment.WrapText = true;
                    ws.Cell(row, 3).Style.Alignment.WrapText = true;
                    ws.Cell(row, 4).Style.Alignment.WrapText = true;
                    ws.Cell(row, 5).Style.Alignment.WrapText = true;
                    ws.Cell(row, 6).Style.Alignment.WrapText = true;
                    Name = item.Substation.ToString();
                }
                row++;
                ws.Cell(row, 3).Value = "Итого:";
                ws.Cell(row, 4).FormulaR1C1 = "=SUM(R[-" + (row - 3) + "]C:R[-1]C)";
                ws.Cell(row, 5).FormulaR1C1 = "=SUM(R[-" + (row - 3) + "]C:R[-1]C)";
                ws.Cell(row, 6).FormulaR1C1 = "=SUM(R[-" + (row - 3) + "]C:R[-1]C)";

                //
                ws.Cell(row + 3, 2).Value = "Сдал";
                ws.Cell(row + 3, 2).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
                ws.Cell(row + 3, 5).Value = "Миронова А.Р.";
                ws.Cell(row + 5, 2).Value = "Принял";
                ws.Cell(row + 5, 2).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
                ws.Cell(row + 7, 2).Value = "Дата";
                ws.Cell(row + 7, 2).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);

                // пример создания сетки в диапазоне
                var rngTable = ws.Range(4, 1, row, 6);
                rngTable.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                rngTable.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                rngTable.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                rngTable.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                rngTable.Style.Font.FontFamilyNumbering = XLFontFamilyNumberingValues.Roman;
                rngTable.Style
                  .Alignment.SetVertical(XLAlignmentVerticalValues.Top)
                  .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                MemoryStream stream = new MemoryStream();
                workBook.SaveAs(stream);
                file.Name = "Реестр по актом допуска на " + inviteDate.ToShortDateString() + ".xlsx";
                file.Data = stream.ToArray();
                return file;
            }
        }

        /// <summary>
        /// Excel отчет на оплату СМР/ПНР
        /// </summary>
        /// <param name="id">Id отчета на оплату СМР/ПНР (PaymentReport)</param>
        /// <returns></returns>
        public ExcelManagerFile PaymentReport(int id)
        {
            ExcelManagerFile file = new ExcelManagerFile();
            var wb = new XLWorkbook(_env.WebRootPath + "/Files/ExcelTemplates/PaymentReport.xlsx"); //Открытие шаблона
            var ws = wb.Worksheet(1);
            using (StoreContext db = new StoreContext())
            {
                var report = db.PaymentReports.Find(id);
                PaymentReportsManager prm = new PaymentReportsManager(db);
                var reportPoints = prm.GetPointsInReport(id).ToList();

                //Шапка
                ws.Cell(1, 5).SetValue("Отчет №" + report.Id);
                //ws.Cell(2, 2).SetValue(report.GetPeriodString());
                ws.Cell(2, 5).SetValue(report.Worker.GetFullName());
                string[] workTypes = { "", "Монтаж", "Демонтаж", "ПНР" };
                double costSumm = 0;
                int row = 3;
                foreach (var item in reportPoints)
                {
                    row++;
                    ws.Cell(row, 1).Value = row - 3;
                    ws.Cell(row, 2).Value = item.RegionName;
                    ws.Cell(row, 3).Value = item.SubstationName;
                    ws.Cell(row, 4).Value = workTypes[(int)item.WorkType];
                    ws.Cell(row, 5).Value = item.OAddress;
                    ws.Cell(row, 6).Value = item.DeviceDescription;
                    ws.Cell(row, 7).Value = item.IsLinkOk ? "отв." : "не отв.";
                    ws.Cell(row, 8).Value = item.CostRUB;
                    costSumm += item.CostRUB;
                    //
                    ws.Cell(row, 2).Style.Alignment.WrapText = true;
                    ws.Cell(row, 3).Style.Alignment.WrapText = true;
                    ws.Cell(row, 4).Style.Alignment.WrapText = true;
                    ws.Cell(row, 5).Style.Alignment.WrapText = true;
                    ws.Cell(row, 6).Style.Alignment.WrapText = true;
                    ws.Cell(row, 7).Style.Alignment.WrapText = true;
                    ws.Cell(row, 8).Style.Alignment.WrapText = true;
                }
                //Итого
                row++;
                string typesCount = "";
                ws.Cell(row, 5).Value = "Всего ТУ " + reportPoints.Count();
                ws.Cell(row, 5).Style.Font.Bold = true;
                foreach (var gr in reportPoints.GroupBy(p => p.DeviceDescription))
                {
                    typesCount += gr.Key + " " + gr.Count() + Environment.NewLine;
                }
                ws.Cell(row, 6).Value = typesCount;
                ws.Cell(row, 6).Style.Font.Bold = true;
                ws.Cell(row, 7).Value = "Отв.: " + reportPoints.Count(p=> p.IsLinkOk) + "/" + reportPoints.Count();
                ws.Cell(row, 7).Style.Font.Bold = true;
                ws.Cell(row, 8).Value = costSumm;
                ws.Cell(row, 8).Style.Font.Bold = true;
                // пwsример создания сетки в диапазоне
                var rngTable = ws.Range(4, 1, row, 8);
                rngTable.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                rngTable.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                rngTable.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                rngTable.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                rngTable.Style.Font.FontFamilyNumbering = XLFontFamilyNumberingValues.Roman;
                rngTable.Style
                  .Alignment.SetVertical(XLAlignmentVerticalValues.Top)
                  .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                MemoryStream stream = new MemoryStream();
                wb.SaveAs(stream);
                file.Name = "Отчет на оплату №" + report.Id + " " + report.Worker.GetFullName() + ".xlsx";
                /*file.Name = "Отчет на оплату №" + report.Id + " " + report.Worker.GetFullName() + report.GetPeriodString() + ".xlsx";*/
                file.Data = stream.ToArray();
                return file;
            }
        }

        /// <summary>
        /// Реестр писем по дате приглашения для загрузки на сайт Почты Росии
        /// </summary>
        /// <param name></param>
        /// <returns></returns>
        /*public ExcelManagerFile AdmissionReestr()
        {
            ExcelManagerFile file = new ExcelManagerFile();
            var workBook = new XLWorkbook(); //Открытие шаблона акта допуска
            var ws = workBook.Worksheets.Add("reesstr"); //Выбор листа
            //создадим заголовки у столбцов
            ws.Cell(1, 1).Value = "ADDRESSLINE";
            ws.Cell(1, 2).Value = "ADRESAT";
            ws.Cell(1, 3).Value = "MASS";
            ws.Cell(1, 23).Value = "NORETURN";

            using (StoreContext db = new StoreContext())
            {
                var data = from points in db.RegPoints
                              
                              join substations in db.Substations on points.SubstationId equals substations.Id
                              join regions in db.NetRegions on substations.NetRegionId equals regions.Id
                              join devices in db.Devices on points.DeviceId equals devices.Id
                              join devicetype in db.DeviceTypes on devices.DeviceTypeId equals devicetype.Id
                              join letters in db.Letters on points.Id equals letters.RegPointId
                              where points.Status == RegPointStatus.Default && letters.Printed == true
                              group devicetype by new
                              {
                                  Substation = substations.Name,
                                  Region = regions.Name,
                                  Type = devicetype.Description
                              } into g
                              select g;
                var result = data.ToList();
                int row = 1;
                int PU_1 = 0;
                int PU_3 = 0;
                foreach (var item in result)
                {
                    if (item.Key.Type == "1ф PLC" || item.Key.Type == "1ф GSM")
                        PU_1++;
                    if (item.Key.Type == "3ф PLC" || item.Key.Type == "3ф GSM")
                        PU_3++;
                }
                foreach (var item in result)
                {
                    row++;
                    ws.Cell(row, 1).Value = row;
                    ws.Cell(row, 2).Value = item.Key.Region;
                    ws.Cell(row, 3).Value = item.Key.Substation;
                    ws.Cell(row, 4).Value = PU_1;
                    ws.Cell(row, 5).Value = PU_3;
                }
                ws.Columns().AdjustToContents(); //ширина столбца по содержимому

                MemoryStream stream = new MemoryStream();
                workBook.SaveAs(stream);
                file.Name = "Реестр по допускам на " + DateTime.Now.ToShortDateString() + ".xlsx";
                file.Data = stream.ToArray();
                return file;
            }
        }*/

        public ExcelManagerFile ExportReportForUSPD (int reportId)
        {
            ExcelManagerFile file = new ExcelManagerFile();
            using (StoreContext db = new StoreContext())
            {
                var report = db.MounterReportUgesALs.Find(reportId);
                var _devices = from support in db.PowerLineSupports
                               where support.MounterReportUgesALId == reportId
                               from kde in support.KDEs
                               from item in kde.MounterReportUgesDeviceItems
                               from device in db.Devices
                               where device.SerialNumber == item.Serial
                               select new { device, device.DeviceType, device.Links};
                                  
                var devices = _devices.ToList().OrderBy(x=> x.device.SerialNumber);

                var wb = new XLWorkbook();
                var ws = wb.Worksheets.Add(report.Substation);
                int row = 1;
                int rowGsm = 1;
                foreach (var item in devices)
                {
                    if(item.DeviceType.Description.ToLower().Contains("plc"))
                    {
                        ws.Cell(row, 1).Value = item.DeviceType.Name;
                        ws.Cell(row, 2).Value ="'" + item.device.SerialNumber;
                        ws.Cell(row, 3).Value = item.device.SerialNumber.Substring(10, 5);
                        row++;
                    }

                    if (item.DeviceType.Description.ToLower().Contains("gsm"))
                    {
                        ws.Cell(rowGsm, 5).Value = item.DeviceType.Name;
                        ws.Cell(rowGsm, 6).Value = "'" + item.device.SerialNumber;
                        ws.Cell(rowGsm, 7).Value = item.device.SerialNumber.Substring(10, 5);
                        if (item.Links != null)
                            ws.Cell(rowGsm, 8).Value = item.Links.LastOrDefault().PhoneNumber;
                        rowGsm++;
                    }

                }
                MemoryStream stream = new MemoryStream();
                wb.SaveAs(stream);
                file.Name = report.Substation + ".xlsx";
                file.Data = stream.ToArray();
            }
            return file;
        }

        public ExcelManagerFile ExportDutySchedule()
        {
            ExcelManagerFile file = new ExcelManagerFile();
            var wb = new XLWorkbook(_env.WebRootPath + "/Files/Duty/Duty.xlsx"); //Открытие шаблона
            MemoryStream stream = new MemoryStream();
            wb.SaveAs(stream);
            file.Name = "График дежурств.xlsx";
            file.Data = stream.ToArray();
            return file;
        }

        public ExcelManagerFile SetDutySchedule(string path)
        {
            ExcelManagerFile file = new ExcelManagerFile();
            var wb = new XLWorkbook(path); //Открытие шаблона
            MemoryStream stream = new MemoryStream();
            wb.SaveAs(stream);
            file.Name = "График дежурств.xlsx";
            file.Data = stream.ToArray();
            return file;
        }
    }
}
