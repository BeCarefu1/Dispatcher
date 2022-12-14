using ActGIPelectroWeb.ViewModels;
using GIPManager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using static ActGIPelectroWeb.ViewModels.SwitchListViewModel;

namespace ActGIPelectroWeb.Controllers
{
    public class ApplicationController : Controller
    {
        public StoreContext db = new StoreContext();
        public List<OrderListViewModel> NetRegionList { get; set; }
        public List<OrderListViewModel> CategoryList { get; set; }
        public List<OrderListViewModel> RepairList { get; set; }
        public List<OrderListViewModel> AGList { get; set; }
        public List<OrderListViewModel> StatusList { get; set; }
        public List<OrderListViewModel> ObjectList { get; set; }
        public List<OrderListViewModel> EquipmentList { get; set; }
        public List<OrderListViewModel> SecurityList { get; set; }
        public List<ApplicationListViewModel> OrderList { get; set; }

        public void GetInfo()
        {
            NetRegionList = (from a in db.NetRegions
                             select new OrderListViewModel
                             {
                                 Id = a.Id,
                                 Name = a.Name
                             }).ToList();

            CategoryList = (from a in db.ApplicationCategory
                            select new OrderListViewModel
                            {
                                Id = a.Id,
                                Name = a.Name
                            }).ToList();

            RepairList = (from a in db.ApplicationRepair
                          select new OrderListViewModel
                          {
                              Id = a.Id,
                              Name = a.Name
                          }).ToList();

            AGList = (from a in db.ApplicationAG
                      select new OrderListViewModel
                      {
                          Id = a.Id,
                          Name = a.Time
                      }).ToList();

            StatusList = (from a in db.AppStatus
                         select new OrderListViewModel
                         {
                             Id = a.Id,
                             Name = a.Name
                         }).ToList();

            ObjectList = (from a in db.ObjectTable
                          select new OrderListViewModel
                          {
                              Id = a.Id,
                              Name = a.Name
                          }).ToList();

            EquipmentList = (from a in db.EquipmentTable
                             select new OrderListViewModel
                             {
                                 Id = a.Id,
                                 Name = a.Name
                             }).ToList();

            SecurityList = (from a in db.SecurityTable
                            select new OrderListViewModel
                            {
                                Id = a.Id,
                                Name = a.Description
                            }).ToList();
        }

        
        public string GetRole(int? roleId)
        {
            switch (roleId)
            {
                case 1:
                    return "администратор";
                    break;
                case 14:
                    return "диспетчер";
                    break;
                case 15:
                    return "мастер";
                    break;
                case 18:
                    return "главный диспетчер";
                    break;
                case 19:
                    return "главный инженер";
                    break;
                case 20:
                    return "главный ПТО";
                    break;
                default:
                    return "Error";
                    break;
            }
        }

        // *** Добавить запись в Оперативный журнал ***
        public async Task AddComment(string comment)
        {
            OperationalLog operational = new OperationalLog();
            operational.UserId = db.Users.FirstOrDefault(s => s.Login == User.Identity.Name).Id;
            operational.RecoilTime = DateTime.Now;
            operational.Description = comment;
            operational.Numb = db.OperationalLog.LastOrDefault().Numb + 1;
            await operational.AddCommentAsync();
        }

        [Authorize]

        public string OpenSwitchFormModal(int id)
        {
            var html = @"<div class='modal-header'>
                <h4 class='modal-title'><i class='fa fa-file-o' aria-hidden='true'></i> Бланк Переключений ___ ___</h4>
                <button type='button' class='close' data-dismiss='modal' aria-label='Close'><span aria-hidden='true'>&times;</span></button>
            </div><div class='modal-body'><table style='width:100%; color: #000000;'>
                    <tr><td style='width: 50%; color: #ffffff;'>space</td><td align='right'><p>Начало выполнения переключений<br /> ___ ч. ___ мин <br /> Дата ________</p></td></tr>
                    <tr><td colspan='2'><p>ОП __________ ООО «ГИП -Электро» <br /> Объект _______________________<br /> Исходная схема _______________<br /> Задание ______________________<br />Последовательность производства операций при переключении</p></td></tr></table>
                <table class='create-table'><tr><th width='10%'>П</th><th width='10%'>О</th><th>Содержание операции</th></tr>
                    <tr><td></td><td></td><td>1. ВЛ-10 кВ Ф-16: Проверить ОП РК-1483 ф. ABC.</td></tr>
                    <tr><td></td><td></td><td>2. Закрыть привод РК-1483 на замок.</td>
                    </tr>
                </table><table style='width:100%; color: #000000;'>
                    <tr><td width='50%'><p>Переключение производит: <br /> ________________________</p></td>
                        <td align='right'><p>Бланк проверил и переключения контролирует: <br /> ________________________</p></td></tr>
                    <tr><td><p>Переключения разрешаю:<br /> _____________________ </p></td>
                        <td align='right'><p>Конец ___ч. ___ мин.<br />Дата ______________</p></td>
                    </tr>
                </table>
            </div>";


            return JsonConvert.SerializeObject(html.ToString());
        }
        
        // *** Начало работ ***
        public async Task<string> StartApplication(int id)
        {
            ApplicationTable table = db.ApplicationTable.Find(id);
            table.Id_Status = 10;
            table.StartActualTime = DateTime.Now;
            try
            {
                DelNotif(id);
                table.EditApplication();
                await CreateNotification(table.Id, table.Id_User, 10);
                //AddComment("<u>" + ShortName(db.Users.Find(db.Users.FirstOrDefault(s => s.Login == User.Identity.Name)).Name) + "</u> — начал работу по заявке №" + table.NumbApp);
                return JsonConvert.SerializeObject("ok");
            }
            catch(Exception ex)
            {
                return JsonConvert.SerializeObject(ex.Message);
            }
        }
        
        // *** Конец работ ***
        public async Task<string> FinishApplication(int id)
        {
            ApplicationTable table = db.ApplicationTable.Find(id);
            table.Id_Status = 9;
            table.EndActualTime = DateTime.Now;
            string result = table.EditApplication();
            if (result == "ok")
            {
                DelNotif(id);
                await CreateNotification(table.Id, table.Id_User, 9);
                //AddComment("<u>" + ShortName(db.Users.Find(db.Users.FirstOrDefault(s => s.Login == User.Identity.Name)).Name) + "</u> — завершил работы по заявке №" + table.NumbApp);
                return JsonConvert.SerializeObject("ok");
            }
            else
            {
                return JsonConvert.SerializeObject(result);
            }
        }


        public string SaveApplication(int id, int elId, string element)
        {
            DateTime date;
            ApplicationTable table = db.ApplicationTable.Find(id);
            switch (elId)
            {
                case 3:
                    table.Id_NetRegion = Convert.ToInt32(element);
                    element = db.NetRegions.Find(Convert.ToInt32(element)).Name;
                    break;
                case 4:
                    table.Id_Category = Convert.ToInt32(element);
                    element = db.ApplicationCategory.Find(Convert.ToInt32(element)).Name;
                    break;
                case 5:
                    table.Id_Repair = Convert.ToInt32(element);
                    element = db.ApplicationRepair.Find(Convert.ToInt32(element)).Name;
                    break;
                case 6:
                    table.Id_AG = Convert.ToInt32(element);
                    element = db.ApplicationAG.Find(Convert.ToInt32(element)).Time;
                    break;
                case 7:
                    //table.Id_State = Convert.ToInt32(element);
                    //element = db..Find(Convert.ToInt32(element)).Name;
                    break;
                case 8:
                    table.ObjectName = element;
                    element = db.ObjectTable.Find(Convert.ToInt32(element)).Name;
                    break;
                case 9:
                    table.Equipment = element;
                    element = db.EquipmentTable.Find(Convert.ToInt32(element)).Name;
                    break;
                case 10:
                    date = Convert.ToDateTime(element);
                    table.StartReqTime = new DateTime(date.Year, date.Month, date.Day);
                    break;
                case 11:
                    table.Description = element;
                    element = element.Replace("\n", "<br>");
                    break;
                case 13:
                    date = Convert.ToDateTime(element);
                    table.StartActualTime = new DateTime(date.Year, date.Month, date.Day);
                    break;
                case 14:
                    table.SecurityDescription = element;
                    element = element.Replace("\n", "<br>");
                    break;
                case 16:
                    date = Convert.ToDateTime(element);
                    table.OpenTime = new DateTime(date.Year, date.Month, date.Day);
                    break;
                case 17:
                    date = Convert.ToDateTime(element);
                    table.CloseTime = new DateTime(date.Year, date.Month, date.Day);
                    break;
                case 18:
                    table.Notes = element;
                    element = element.Replace("\n", "<br>");
                    break;
                default:
                    break;
            }
            table.EditApplication();
            
            return JsonConvert.SerializeObject( element + " <i class='fa fa-pencil' aria-hidden='true'></i>");
        }

        public string SaveApplicationTwo(int id, int elId, string element, string element2)
        {
            DateTime date;
            ApplicationTable table = db.ApplicationTable.Find(id);
            switch (elId)
            {
                case 12:
                    date = Convert.ToDateTime(element);
                    table.StartReqTime = new DateTime(table.StartReqTime.Year, table.StartReqTime.Month, table.StartReqTime.Day, date.Hour, date.Minute, date.Second);
                    date = Convert.ToDateTime(element2);
                    table.EndReqTime = new DateTime(table.EndReqTime.Year, table.EndReqTime.Month, table.EndReqTime.Day, date.Hour, date.Minute, date.Second);
                    break;
                case 15:
                    date = Convert.ToDateTime(element);
                    table.StartActualTime = new DateTime(table.StartActualTime.Year, table.StartActualTime.Month, table.StartActualTime.Day, date.Hour, date.Minute, date.Second);
                    date = Convert.ToDateTime(element2);
                    table.EndReqTime = new DateTime(table.StartActualTime.Year, table.StartActualTime.Month, table.StartActualTime.Day, date.Hour, date.Minute, date.Second);
                    break;
                default:
                    break;
            }
            return JsonConvert.SerializeObject(element + " — " + element2 +  "<i class='fa fa-pencil' aria-hidden='true'></i>");
        }

        // *** Отмена заявки ***
        public async Task<string> CancelhApplicationAsync(int id)
        {
            ApplicationTable table = db.ApplicationTable.Find(id);
            table.Id_Status = 5;
            string result = table.EditApplication();
            if (result == "ok")
            {
                await DelNotif(id);
                await CreateNotification(table.Id, table.Id_User, 8);
                await AddComment("Отменил заявку №" + table.NumbApp);
                await Models.EventHistoryModel.AddEvent("Отменил заявку №", User.Identity.Name);
                return JsonConvert.SerializeObject("ok");
            }
            else
            {
                return JsonConvert.SerializeObject(result);
            }
        }

        public string ShowEditModal(int id, int elId, string text)
        {
            GetInfo();
            if (text != null)
            {
                text = text.Replace("<br>", "\n");
            }
            string onclick = "";
            string html = "<div class='modal-header'><h4 class='modal-title'><i class='fa fa-pencil-square-o' aria-hidden='true'></i> Редактирование заявки</h4><button type='button' class='close' data-dismiss='modal' aria-label='Close'><span aria-hidden='true'>&times;</span></button></div><div class='modal-body' style='align-items:center;'>";
            switch (elId)
            {
                case 3:
                    html += "<select class='custom-select' id='editId' name='mySelect' style='color:#000000;' onchange='Voledit()'>";
                    foreach (var item in NetRegionList) 
                    { html += "<option value='" + item.Id + "'>" + item.Name + "</option>"; }
                    html += "</select>";
                    onclick = "onclick='ApplyEditApp(" + id + "," + elId + "," + 1 + ")'";
                    break;
                case 4:
                    html += "<select class='custom-select' id='editId' name='mySelect' style='color:#000000;' onchange='Voledit()'>";
                    foreach (var item in CategoryList)
                    { html += "<option value='" + item.Id + "'>" + item.Name + "</option>"; }
                    html += "</select>";
                    onclick = "onclick='ApplyEditApp(" + id + "," + elId + "," + 1 + ")'";
                    break;
                case 5:
                    html += "<select class='custom-select' id='editId' name='mySelect' style='color:#000000;' onchange='Voledit()'>";
                    foreach (var item in RepairList)
                    { html += "<option value='" + item.Id + "'>" + item.Name + "</option>"; }
                    html += "</select>";
                    onclick = "onclick='ApplyEditApp(" + id + "," + elId + "," + 1 + ")'";
                    break;
                case 6:
                    html += "<select class='custom-select' id='editId' name='mySelect' style='color:#000000;' onchange='Voledit()'>";
                    foreach (var item in AGList)
                    { html += "<option value='" + item.Id + "'>" + item.Name + "</option>"; }
                    html += "</select>";
                    onclick = "onclick='ApplyEditApp(" + id + "," + elId + "," + 1 + ")'";
                    break;
                case 7:
                    html += "<select class='custom-select' id='editId' name='mySelect' style='color:#000000;' onchange='Voledit()'>";
                    foreach (var item in StatusList)
                    { html += "<option value='" + item.Id + "'>" + item.Name + "</option>"; }
                    html += "</select>";
                    onclick = "onclick='ApplyEditApp(" + id + "," + elId + "," + 1 + ")'";
                    break;
                case 8:
                    html += "<select class='custom-select' id='editId' name='mySelect' style='color:#000000;' onchange='Voledit()'>";
                    foreach (var item in ObjectList)
                    { html += "<option value='" + item.Id + "'>" + item.Name + "</option>"; }
                    html += "</select>";
                    onclick = "onclick='ApplyEditApp(" + id + "," + elId + "," + 1 + ")'";
                    break;
                case 9:
                    html += "<select class='custom-select' id='editId' name='mySelect' style='color:#000000;' onchange='Voledit()'>";
                    foreach (var item in EquipmentList)
                    { html += "<option value='" + item.Id + "'>" + item.Name + "</option>"; }
                    html += "</select>";
                    onclick = "onclick='ApplyEditApp(" + id + "," + elId + "," + 1 + ")'";
                    break;
                case 10:
                    html += "<input id='editId' type='date' name='cron' style='width: 100 %; height: 100 %; ' onchange='Voledit()'>";
                    onclick = "onclick='ApplyEditApp(" + id + "," + elId + "," + 1 + ")'";
                    break;
                case 11:
                    html += "<textarea name='comment' id='editId' class='form-control' onchange='Voledit()'>" + text + "</textarea>";
                    onclick = "onclick='ApplyEditApp(" + id + "," + elId + "," + 1 + ")'";
                    break;
                case 12:
                    html += "<input id='editId' type='time' name='cron' onchange='Voledit()'> — <input id='editId2' type='time' name='cron' onchange='Voledit()'>";
                    onclick = "onclick='ApplyEditApp(" + id + "," + elId + "," + 2 + ")'";
                    break;
                case 13:
                    html += "<input id='editId' type='date' name='cron' style='width: 100 %; height: 100 %; ' onchange='Voledit()'>";
                    onclick = "onclick='ApplyEditApp(" + id + "," + elId + "," + 1 + ")'";
                    break;
                case 14:
                    html += "<textarea name='comment' id='editId' class='form-control' onchange='Voledit()'>" + text + "</textarea>";
                    onclick = "onclick='ApplyEditApp(" + id + "," + elId + "," + 1 + ")'";
                    break;
                case 15:
                    html += "<input id='editId' type='time' name='cron' onchange='Voledit()'> — <input id='editId2' type='time' name='cron' onchange='Voledit()'>";
                    onclick = "onclick='ApplyEditApp(" + id + "," + elId + "," + 2 + ")'";
                    break;
                case 16:
                    html += "<input id='editId' type='time' name='cron' onchange='Voledit()'>";
                    onclick = "onclick='ApplyEditApp(" + id + "," + elId + "," + 1 + ")'";
                    break;
                case 17:
                    html += "<input id='editId' type='time' name='cron' onchange='Voledit()'>";
                    onclick = "onclick='ApplyEditApp(" + id + "," + elId + "," + 1 + ")'";
                    break;
                case 18:
                    html += "<textarea name ='comment' id='editId' class='form-control' onchange='Voledit()'>" + text + "</textarea>";
                    onclick = "onclick='ApplyEditApp(" + id + "," + elId + "," + 1 + ")'";
                    break;

                default:
                    break;
            }
            html += "</div><div class='modal-footer'><button class='btn btn-primary' " + onclick + " > OK</button></div>";
            return JsonConvert.SerializeObject(html);
        }


        public string GetI(int id, string text, string element, int elId, int statisId)
        {
            if(element != null)
            {
                element = element.Replace("\n", "<br>");
            }
            if (text != null)
            {
                text = text.Replace("\n", "<br>");
            }
            string html = "";
            if ((User.IsInRole("administrator") || User.IsInRole("dispatcher")) && (statisId == 1))
            {
                if (element != null)
                {
                    html = " id='t" + elId + "' onclick='EditInfo(" + id + "," + elId + ",\"" + text + "\")'>" + element + " <i class='fa fa-pencil' aria-hidden='true'></i>";
                }
                else
                {
                    html = " id='t" + elId + "' onclick='EditInfo(" + id + "," + elId + ",\"" + text + "\")'>" + " " + " <i class='fa fa-pencil' aria-hidden='true'></i>";
                }
            }
            else
            {
                html = ">" + element;
            }

            return html;
        }


        public async Task DelNotif(int id)
        {
            if (db.NotificationTable.Count(s => s.Id_App == id && s.Id_Type != 7) != 0)
            {
                List<IdContentList> idNotif = (from a in db.NotificationTable
                                               where a.Id_App == id && a.Id_Type != 7
                                               select new IdContentList
                                               {
                                                   Id = a.Id
                                               }).ToList();
                foreach (var item in idNotif)
                {
                    await db .NotificationTable.Find(item.Id).DeleteNotification();
                }
            }
        }

        // *** Отказ заявки ***
        public async Task<string> RefuseApp(int id)
        {
            ApplicationTable table = db.ApplicationTable.Find(id);
            if (User.IsInRole("dispatcher"))
            {
                AppCheckDisp signature = new AppCheckDisp();
                table.Id_Status = 3;
                signature.Id_App = table.Id;
                signature.Id_User = db.Users.FirstOrDefault(s => s.Login == User.Identity.Name).Id;
                signature.AppCheckDisp();
                try
                {
                    await DelNotif(id);
                    table.EditApplication();
                    await CreateNotification(table.Id, table.Id_User, 3);
                    await AddComment("Отклонил заявку №" + table.NumbApp);
                    await Models.EventHistoryModel.AddEvent("Отклонил заявку №" + table.NumbApp, User.Identity.Name);
                    return JsonConvert.SerializeObject("ok");
                }
                catch(Exception ex)
                {
                    return JsonConvert.SerializeObject(ex.Message);
                }
            }
            if (User.IsInRole("сhiefEngineer") || User.IsInRole("bossPTO"))
            {
                AppSignature signature = new AppSignature();
                //table.Id_Status = 3;
                signature.Id_App = table.Id;
                signature.Id_User = db.Users.FirstOrDefault(s => s.Login == User.Identity.Name).Id;
                signature.AddSignature();
                table.Id_Status = 5;
                try
                {
                    await DelNotif(id);
                    table.EditApplication();
                    await CreateNotification(table.Id, table.Id_User, 5);
                    await AddComment("Отклонил заявку №" + table.NumbApp);
                    await Models.EventHistoryModel.AddEvent("Отклонил заявку №" + table.NumbApp, User.Identity.Name);
                    return JsonConvert.SerializeObject("ok");
                }
                catch(Exception ex)
                {
                    return JsonConvert.SerializeObject(ex.Message);
                }
            }
            //CreateNotification(table.Id, table.Id_User, 2);
            return JsonConvert.SerializeObject("Error");
        }


        // *** Подписание заявки ***
        public async Task<string> SubscribeApp(int id)
        {
            ApplicationTable table = db.ApplicationTable.Find(id);
            string result;
            if (User.IsInRole("dispatcher") && table.Id_Status == 1)
            {
                AppCheckDisp signature = new AppCheckDisp();
                signature.Id_App = table.Id;
                signature.Id_User = db.Users.FirstOrDefault(s => s.Login == User.Identity.Name).Id;
                
                table.Id_Status = 2;
                try
                {
                    await DelNotif(id);
                    signature.AppCheckDisp();
                    table.EditApplication();
                    await CreateNotification(table.Id, table.Id_User, 2);
                    await AddComment("Проверил заявку №" + table.NumbApp);
                    await Models.EventHistoryModel.AddEvent("Проверил заявку №" + table.NumbApp, User.Identity.Name);
                    return JsonConvert.SerializeObject("ok");
                }
                catch(Exception ex) { return JsonConvert.SerializeObject(ex.Message); }
            }
            else if (User.IsInRole("dispatcher") && table.Id_Status == 4)
            {
                AppOpenDisp signature = new AppOpenDisp();
                signature.Id_App = table.Id;
                signature.Id_User = db.Users.FirstOrDefault(s => s.Login == User.Identity.Name).Id;
                signature.AppOpenDisp();
                table.OpenTime = DateTime.Now;
                table.Id_Status = 6;
                //result = table.EditApplication();
                try
                {
                    await DelNotif(id);
                    table.EditApplication();
                    await CreateNotification(table.Id, table.Id_User, 6);
                    await AddComment("Открыл заявку №" + table.NumbApp);
                    await Models.EventHistoryModel.AddEvent("Открыл заявку №" + table.NumbApp, User.Identity.Name);
                    return JsonConvert.SerializeObject("ok");
                }
                catch (Exception ex) { return JsonConvert.SerializeObject(ex.Message); }
            }
            else if (User.IsInRole("dispatcher") && table.Id_Status == 6)
            {
                AppCloseDisp signature = new AppCloseDisp();
                signature.Id_App = table.Id;
                signature.Id_User = db.Users.FirstOrDefault(s => s.Login == User.Identity.Name).Id;
                signature.AppCloseDisp();
                table.CloseTime = DateTime.Now;
                table.Id_Status = 7;
                try
                {
                    await DelNotif(id);
                    table.EditApplication();
                    await CreateNotification(table.Id, table.Id_User, 7);
                    await AddComment("Закрыл заявку №" + table.NumbApp);
                    await Models.EventHistoryModel.AddEvent("Закрыл заявку №" + table.NumbApp, User.Identity.Name);
                    return JsonConvert.SerializeObject("ok");
                }
                catch (Exception ex) { return JsonConvert.SerializeObject(ex.Message); }
            }
            else if (User.IsInRole("сhiefEngineer") || User.IsInRole("bossPTO"))
            {
                AppSignature signature = new AppSignature();
                signature.Id_App = table.Id;
                signature.Id_User = db.Users.FirstOrDefault(s => s.Login == User.Identity.Name).Id;
                signature.AddSignature();
                table.Id_Status = 4;
                try
                {
                    await DelNotif(id);
                    table.EditApplication();
                    await CreateNotification(table.Id, table.Id_User, 4);
                    await AddComment("Разрешил заявку №" + table.NumbApp);
                    await Models.EventHistoryModel.AddEvent("Разрешил заявку №" + table.NumbApp, User.Identity.Name);
                    return JsonConvert.SerializeObject("ok");
                }
                catch(Exception ex) { return JsonConvert.SerializeObject(ex.Message); }
                
            }
            else
            {
                return JsonConvert.SerializeObject("Error");
            }

            //CreateNotification(table.Id, table.Id_User, 2);
            //table.Id_SigUser = db.Users.FirstOrDefault(s => s.Login == User.Identity.Name).Id;
            
            
        }

        // *** Создание заявки ***
        public async Task<string> CreateApplication(int Id_AG, int Id_NetRegion, int Id_Category, int Id_Repair, string Object, string Equipment, string Description, DateTime StartReqDate, DateTime StartReqTime, DateTime EndReqTime, string Notes, string Security)
        {
            ApplicationTable table = new ApplicationTable();
            int userId = db.Users.FirstOrDefault(s => s.Login == User.Identity.Name).Id;
            table.Id_AG = Id_AG;
            table.Id_NetRegion = Id_NetRegion;
            table.Id_Category = Id_Category;
            //table.Id_State = Id_State;
            table.Id_Repair = Id_Repair;
            table.ObjectName = Object;
            table.Equipment = Equipment;
            table.StartReqTime = new DateTime(StartReqDate.Year, StartReqDate.Month, StartReqDate.Day, StartReqTime.Hour, StartReqTime.Minute, StartReqTime.Second);
            table.EndReqTime = new DateTime(StartReqDate.Year, StartReqDate.Month, StartReqDate.Day, EndReqTime.Hour, EndReqTime.Minute, EndReqTime.Second);
            //table.StartActualTime = new DateTime(StartActualDate.Year, StartActualDate.Month, StartActualDate.Day, StartActualTime.Hour, StartActualTime.Minute, StartActualTime.Second);
            //table.EndActualTime = new DateTime(StartActualDate.Year, StartActualDate.Month, StartActualDate.Day, EndActualTime.Hour, EndActualTime.Minute, EndActualTime.Second);
            //table.OpenTime = OpenTime;
            //table.CloseTime = CloseTime;
            table.Date = DateTime.Now;
            if (User.IsInRole("master"))
            {
                table.Id_Status = 1;
            }
            else
            {
                table.Id_Status = 2;
            }
            table.Id_User = userId;
            if (Description != null)
            {
                table.Description = Description;
            }
            if (Security != null)
            {
                table.SecurityDescription = Security;
            }
            if (Notes != null)
            {
                table.Notes = Notes;
            }
            if (db.ApplicationTable.Count() == 0)
            {
                table.NumbApp = 1;
            }
            else  if (DateTime.Now.Month == 1 && DateTime.Now.Day == 1)
            {
                table.NumbApp = 1;
            }
            else
            {
                table.NumbApp = db.ApplicationTable.LastOrDefault().NumbApp + 1;
                
            }
            try
            {
                table.CreateApplication();
                int id_App = table.Id;
                await CreateNotification(id_App, userId, 1);
                await AddComment("Создал новую заявку №" + table.NumbApp);
                await Models.EventHistoryModel.AddEvent("Заявка №" + table.NumbApp + " создана", User.Identity.Name);
            }
            catch(Exception ex) 
            { 
                return JsonConvert.SerializeObject(ex.Message);
            }
            
            return JsonConvert.SerializeObject("ok");
        }

        // *** Создание БП ***
        public IActionResult CreateSwitchForm(int id)
        {
            try
            {
                SwitchFormTable switchForm = new SwitchFormTable();
                switchForm.idApp = id;
                switchForm.CreateSwitchForm();
                return View("_AppAndSwitches", new AppAndSwitchListViewModels(id));
            }
            catch { return View("_AppAndSwitches", new AppAndSwitchListViewModels(id)); }
            
        }

        // *** Удаление БП ***
        public IActionResult DelSwitchForm(int id)
        {
            int idApp = 0;
            try
            {
                SwitchFormTable switchForm = db.SwitchFormTable.Find(id);
                idApp = switchForm.idApp;
                switchForm.DelSwitchForm();
                return View("_AppAndSwitches", new AppAndSwitchListViewModels(idApp));
            }
            catch 
            {
                SwitchFormTable switchForm = db.SwitchFormTable.Find(id);
                idApp = switchForm.idApp;
                return View("_AppAndSwitches", new AppAndSwitchListViewModels(idApp)); 
            }

        }

        public class IdContentList
        {
            public int Id { get; set; }
        }

        // *** Удалении ***
        public async Task<string> DeleteApplicationAsync(int id)
        {
            try
            {
                // Доделать
                if (db.SwitchFormTable.Count(s => s.idApp == id) != 0)
                {
                    SwitchFormTable switchForm = db.SwitchFormTable.FirstOrDefault(s => s.idApp == id);
                    List<IdContentList> idContents = (from a in db.ContentOfOperations
                                                      join b in db.SwitchFormTable on a.IdSwitchForm equals b.Id
                                                      where b.idApp == id
                                                      select new IdContentList
                                                      {
                                                          Id = a.Id
                                                      }).ToList();
                    foreach(var item in idContents)
                    {
                        db.ContentOfOperations.Find(item.Id).DelContent();
                    }
                    switchForm.DelSwitchForm();
                }
                if (db.NotificationTable.Count(s => s.Id_App == id) != 0)
                {
                    List<IdContentList> idNotif = (from a in db.NotificationTable
                                                   where a.Id_App == id
                                                   select new IdContentList
                                                   {
                                                       Id = a.Id
                                                   }).ToList();
                    foreach(var item in idNotif)
                    {
                        await db.NotificationTable.Find(item.Id).DeleteNotification();
                    }
                }
                ApplicationTable table = db.ApplicationTable.Find(id);

                
                table.DeleteApplication();
                int id_App = table.Id;
                await CreateNotification(table.Id, table.Id_User, 11);
                await AddComment("Заявка №" + table.NumbApp + " удалена");
                await Models.EventHistoryModel.AddEvent("Заявка №" + table.NumbApp +" удалена", User.Identity.Name);
                return JsonConvert.SerializeObject("ok");
            }
            catch (Exception ex) { return JsonConvert.SerializeObject(ex.Message); }
        }

        // Изменение примечаний 
        public async Task<string> EditApplication(int id, string objects, string equipment, string description, string security, DateTime time1, DateTime time2, string note)
        {
            ApplicationTable application = db.ApplicationTable.Find(id);
            application.ObjectName = objects;
            application.Equipment = equipment;
            application.Description = description;
            application.SecurityDescription = security;
            application.StartReqTime = new DateTime(application.StartReqTime.Year, application.StartReqTime.Month, application.StartReqTime.Day, time1.Hour, time1.Minute, time1.Second); 
            application.EndReqTime = new DateTime(application.EndReqTime.Year, application.EndReqTime.Month, application.EndReqTime.Day, time2.Hour, time2.Minute, time2.Second);
            application.Notes = note;
            string result = application.EditApplication();
            if(result == "ok")
            {
                await CreateNotification(application.NumbApp, application.Id_User, 2);
            }
            return JsonConvert.SerializeObject(result);

        }

        // добавить содержание операции
        public string AddContent(int id, string content)
        {
            ContentOfOperations contents = new ContentOfOperations();
            contents.Content = content;
            contents.IdSwitchForm = id;
            try
            {
                contents.AddContent();
                return JsonConvert.SerializeObject(contents.Id);
            }
            catch(Exception ex)
            {
                return JsonConvert.SerializeObject(ex.Message);
            }

        }

        // Удаление операций в БП
        public string DelContent(int id)
        {
            try
            {
                ContentOfOperations content = db.ContentOfOperations.Find(id);
                content.DelContent();
                return JsonConvert.SerializeObject("ok");
            }
            catch(Exception ex)
            {
                return JsonConvert.SerializeObject(ex.Message);
            }

        }

        /// <summary>
        /// Начало работ БП
        /// </summary>
        /// <param name="id"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        public void SaveStartDateOnChange(int id, DateTime date)
        {
            try
            {
                SwitchFormTable switchForm = db.SwitchFormTable.Find(id);
                switchForm.StartDate = date;
                switchForm.EditSwitchForm();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Конец работ БП
        /// </summary>
        /// <param name="id"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        public void SaveEndDateOnChange(int id, DateTime date)
        {
            try
            {
                SwitchFormTable switchForm = db.SwitchFormTable.Find(id);
                switchForm.EndDate = date;
                switchForm.EditSwitchForm();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        
        /// <summary>
        /// Изменение исходной схемы
        /// </summary>
        /// <param name="id">Id БП</param>
        /// <param name="scheme"></param>
        /// <returns></returns>
        public void SaveSourceSchemeDateOnChange(int id, string scheme)
        {
            SwitchFormTable switchForm = db.SwitchFormTable.Find(id);
            switchForm.SourceSchema = scheme;
            switchForm.EditSwitchForm();
            
        }

        // *** Просмотр заявки и БП ***
        public IActionResult GetAppTable (int id)
        {
            
            return View("_AppAndSwitches", new AppAndSwitchListViewModels(id));
            //return View("_AppShow", orderList);
        }

        public IActionResult GetSwitchTable(int id)
        {
            if(db.SwitchFormTable.FirstOrDefault(s => s.idApp == id) == null)
            {
                SwitchFormTable switchForm = new SwitchFormTable();
                switchForm.idApp = id;
                switchForm.CreateSwitchForm();
            }
            string nameDisp = "";
            string roleDisp = "";

            if (db.SwitchFormSignature.SingleOrDefault(s => s.idSwitch == db.SwitchFormTable.SingleOrDefault(f => f.idApp == id).Id) != null)
            {
                var signatureDisp = db.SwitchFormSignature.SingleOrDefault(s => s.idSwitch == db.SwitchFormTable.SingleOrDefault(f => f.idApp == id).Id);
                nameDisp = Models.ShortNameModel.ShortName(db.Users.Find(signatureDisp.idUser).Name);
                roleDisp = GetRole(db.Users.Find(signatureDisp.idUser).RoleId);
            }

            var switchs = (from a in db.SwitchFormTable
                       join b in db.ApplicationTable on a.idApp equals b.Id
                       //join c in db.ContentOfOperations on a.Id equals c.IdSwitchForm
                       join d in db.NetRegions on b.Id_NetRegion equals d.Id
                       where a.idApp == id
                       select new SwitchListViewModel
                       {
                           Id = a.Id,
                           IdNetRegion = b.Id_NetRegion,
                           NetRegion = d.Name,
                           ObjectName = b.ObjectName,
                           Job = b.Description,
                           NumbApp = b.NumbApp,
                           IdApp = b.Id,
                           NameDisp = nameDisp,
                           RoleDisp = roleDisp,
                           IdStatusApp = b.Id_Status,
                           Contents = (from f in db.ContentOfOperations
                                       where f.IdSwitchForm == a.Id
                                       select new ContentOperationList
                                       {
                                           Id = f.Id,
                                           Content = f.Content,
                                           IdSwitchForm = f.IdSwitchForm
                                       }).ToList()
                       }).FirstOrDefault();
            return View("_SwitchForm", switchs);
        }
        
        // *** Добавление уведомление ***
        public async Task CreateNotification(int idApp, int userId, int typeId)
        {
            List<ApplicationListViewModel> usersList = new List<ApplicationListViewModel>();
            
            switch (typeId)
            {
                case 1:
                    usersList = (from a in db.Users
                                 where a.RoleId == 14 // || a.RoleId == 18 || a.RoleId == 19 || a.RoleId == 20
                                 select new ApplicationListViewModel
                                 {
                                     Id = a.Id
                                 }).ToList();
                    break;
                case 2:
                    List<ApplicationListViewModel> usersListTwo = new List<ApplicationListViewModel>();
                    usersList = (from a in db.Users
                                 where userId == a.Id
                                 select new ApplicationListViewModel
                                 {
                                     Id = a.Id
                                 }).ToList();
                    usersListTwo = (from a in db.Users
                                    where a.RoleId == 19 || a.RoleId == 20
                                    select new ApplicationListViewModel
                                    {
                                        Id = a.Id
                                    }).ToList();
                    foreach (var item in usersListTwo)
                    {
                        await new NotificationTable().CreateNotification(userId, 1, idApp, item.Id);
                    }
                    break;
                case 3:
                    usersList = (from a in db.Users
                                 where userId == a.Id
                                 select new ApplicationListViewModel
                                 {
                                     Id = a.Id
                                 }).ToList();
                    break;
                case 4:
                    usersList = (from a in db.Users
                                 where userId == a.Id || a.RoleId == 14
                                 select new ApplicationListViewModel
                                 {
                                     Id = a.Id
                                 }).ToList();
                    break;
                case 5:
                    usersList = (from a in db.Users
                                 where userId == a.Id || a.RoleId == 14
                                 select new ApplicationListViewModel
                                 {
                                     Id = a.Id
                                 }).ToList();
                    break;
                case 6:
                    usersList = (from a in db.Users
                                 where userId == a.Id
                                 select new ApplicationListViewModel
                                 {
                                     Id = a.Id
                                 }).ToList();
                    break;
                case 7:
                    usersList = (from a in db.Users
                                 where userId == a.Id || a.RoleId == 14
                                 select new ApplicationListViewModel
                                 {
                                     Id = a.Id
                                 }).ToList();
                    break;

                case 8:
                    usersList = (from a in db.Users
                                 where a.RoleId == 14 // || a.RoleId == 18 || a.RoleId == 19 || a.RoleId == 20
                                 select new ApplicationListViewModel
                                 {
                                     Id = a.Id
                                 }).ToList();
                    break;
                case 9:
                    usersList = (from a in db.Users
                                 where a.RoleId == 14 // || a.RoleId == 18 || a.RoleId == 19 || a.RoleId == 20
                                 select new ApplicationListViewModel
                                 {
                                     Id = a.Id
                                 }).ToList();
                    break;
                case 10:
                    usersList = (from a in db.Users
                                 where a.RoleId == 14 // || a.RoleId == 18 || a.RoleId == 19 || a.RoleId == 20
                                 select new ApplicationListViewModel
                                 {
                                     Id = a.Id
                                 }).ToList();
                    break;
                case 11:
                    usersList = (from a in db.Users
                                 where userId == a.Id || a.RoleId == 14
                                 select new ApplicationListViewModel
                                 {
                                     Id = a.Id
                                 }).ToList();
                    break;
                case 12:
                    usersList = (from a in db.Users
                                 where userId == a.Id
                                 select new ApplicationListViewModel
                                 {
                                     Id = a.Id
                                 }).ToList();
                    break;

                default:
                    break;
            }

            foreach (var item in usersList)
            {
                await new NotificationTable().CreateNotification(userId, typeId, idApp, item.Id);
            }
        }

        public string SubscribeSwitchForm(int id)
        {
            try
            {
                if (db.SwitchFormSignature.FirstOrDefault(s => s.idSwitch == id) == null)
                {
                    SwitchFormSignature signature = new SwitchFormSignature();
                    signature.idSwitch = id;
                    signature.idUser = db.Users.FirstOrDefault(s => s.Login == User.Identity.Name).Id;
                    signature.AddSwitchSig();
                }
                return JsonConvert.SerializeObject("ok");
            }
            catch (Exception ex) { return JsonConvert.SerializeObject(ex.Message); }
        }

        [HttpPost]
        public async Task DeleteNotification(int id)
        {
            NotificationTable notification;
            if (User.IsInRole("dispatcher"))
            {
                List<ApplicationListViewModel> notifList = new List<ApplicationListViewModel>();
                int appId = db.NotificationTable.Find(id).Id_App;

                notifList = (from a in db.Users
                             join b in db.NotificationTable on a.Id equals b.WhitchId
                                where a.RoleId == 14 && b.Id_App == appId
                                select new ApplicationListViewModel
                                {
                                    Id = b.Id
                                }).ToList();
                
                foreach (var item in notifList)
                {
                    notification = db.NotificationTable.Find(item.Id);
                    await notification.DeleteNotification();
                }
                return;
            }
            if (User.IsInRole("chiefEngineer") || User.IsInRole("bossPTO"))
            {
                List<ApplicationListViewModel> notifList = new List<ApplicationListViewModel>();
                int appId = db.NotificationTable.Find(id).Id_App;

                notifList = (from a in db.Users
                             join b in db.NotificationTable on a.Id equals b.WhitchId
                             where (a.RoleId == 19 || a.RoleId == 20) && b.Id_App == appId
                             select new ApplicationListViewModel
                             {
                                 Id = b.Id
                             }).ToList();

                foreach (var item in notifList)
                {
                    notification = db.NotificationTable.Find(item.Id);
                    await notification .DeleteNotification();
                }
                return;
            }
            notification = db.NotificationTable.Find(id);
            await notification.DeleteNotification();

        }

        public async Task<int> CheckUpDateApp(int count)
        {
            int countDB = db.ApplicationTable.Count(a => a.Id_Status == 1 || a.Id_Status == 2 || a.Id_Status == 4 || a.Id_Status == 6 || a.Id_Status == 9 || a.Id_Status == 10);
            if (countDB != count)
            {
                return countDB;
            }
            else
            {
                return count;
            }
        }

        public async Task<int> CheckUpDateHome(int count)
        {
            int countDB = 0;
            //int allDB = db.ApplicationTable.Count(a => a.Id_Status == 1 || a.Id_Status == 2 || a.Id_Status == 4 || a.Id_Status == 6 || a.Id_Status == 9 || a.Id_Status == 10);
            if (User.IsInRole("master"))
            {
                countDB = db.ApplicationTable.Count(s => (s.Id_Status == 6 || s.Id_Status == 10) && s.Id_User == db.Users.SingleOrDefault(a => a.Login == User.Identity.Name).Id);
            }
            if (User.IsInRole("dispatcher") || User.IsInRole("chief_dispatcher"))
            {
                countDB = db.ApplicationTable.Count(s => s.Id_Status == 1 || s.Id_Status == 4 || s.Id_Status == 9 || s.Id_Status == 10);
            }
            if (User.IsInRole("сhiefEngineer") || User.IsInRole("bossPTO"))
            {
                countDB = db.ApplicationTable.Count(s => s.Id_Status == 2);
            }
            if (count == countDB)
            {
                return count;
            }
            else
            {
                return countDB;
            }

        }

        // *** Сохранение изменений ***

        public async Task SaveContentOnChange(int id, string text)
        {
            ContentOfOperations content = db.ContentOfOperations.Find(id);
            content.Content = text;
            await Task.Run(async delegate { await content.EditContentAsync(); });
        }

        public IActionResult GetAppPrintTable(int id)
        {
            return View("_AppShow", new AppAndSwitchPrintViewModels(id));
        }

        public IActionResult GetSwitchPrintTable(int id)
        {
            if (db.SwitchFormTable.FirstOrDefault(s => s.idApp == id) == null)
            {
                SwitchFormTable switchForm = new SwitchFormTable();
                switchForm.idApp = id;
                switchForm.CreateSwitchForm();
            }
            string nameDisp = "";
            string roleDisp = "";

            if (db.SwitchFormSignature.SingleOrDefault(s => s.idSwitch == db.SwitchFormTable.SingleOrDefault(f => f.idApp == id).Id) != null)
            {
                var signatureDisp = db.SwitchFormSignature.SingleOrDefault(s => s.idSwitch == db.SwitchFormTable.SingleOrDefault(f => f.idApp == id).Id);
                nameDisp = Models.ShortNameModel.ShortName(db.Users.Find(signatureDisp.idUser).Name);
                roleDisp = GetRole(db.Users.Find(signatureDisp.idUser).RoleId);
            }

            var switchs = (from a in db.SwitchFormTable
                           join b in db.ApplicationTable on a.idApp equals b.Id
                           //join c in db.ContentOfOperations on a.Id equals c.IdSwitchForm
                           join d in db.NetRegions on b.Id_NetRegion equals d.Id
                           where a.Id == id
                           select new SwitchListViewModel
                           {
                               Id = a.Id,
                               IdNetRegion = b.Id_NetRegion,
                               NetRegion = d.Name,
                               ObjectName = b.ObjectName,
                               Job = b.Description,
                               NumbApp = b.NumbApp,
                               IdApp = b.Id,
                               NameDisp = nameDisp,
                               RoleDisp = roleDisp,
                               IdStatusApp = b.Id_Status,
                               Contents = (from f in db.ContentOfOperations
                                           where f.IdSwitchForm == a.Id
                                           select new ContentOperationList
                                           {
                                               Id = f.Id,
                                               Content = f.Content,
                                               IdSwitchForm = f.IdSwitchForm
                                           }).ToList()
                           }).FirstOrDefault();
            return View("_SwitchForm", switchs);
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
