using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using GIPManager;
using ActGIPelectroWeb.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ActGIPelectroWeb.Controllers
{
	public class RegPointController : Controller
	{
		private StoreContext db;

		public RegPointController(StoreContext context)
		{
			db = context;
		}
        private User GetUser()
        {
            return db.Users.FirstOrDefault(u => u.Login == User.Identity.Name);
        }

        [Authorize]
        public IActionResult HTML_PointsLettersTable(int Id)
        {
            List<PointaLettersRowViewModel> data = new List<PointaLettersRowViewModel>();

            //Запрос из базы
            var list = (from rp in db.RegPoints
                        join l in db.Letters on rp.Id equals l.RegPointId into ls
                        from l in ls.DefaultIfEmpty()
                        join device in db.Devices on rp.DeviceId equals device.Id into dsub
                        from device in dsub.DefaultIfEmpty()
                        join linkD in db.Links on device.Id equals linkD.DeviceId into lDs
                        from linkD in lDs.DefaultIfEmpty()   //Link привязанные к устройствам
                        join linkS in db.SubstationLinks on Id equals linkS.SubstationId into lSs
                        from linkS in lSs.DefaultIfEmpty()   //Link привязанные к подстанции
                        where rp.Status == RegPointStatus.Default
                        where rp.SubstationId == Id
                        select new
                        {
                            rp.Id,
                            CustomerName = rp.Consumer.Name,
                            Address_U = RegPointRowViewModel.FormatAddress(rp.Consumer.U_Local, rp.Consumer.U_Local_Secondary, rp.Consumer.U_Street, rp.Consumer.U_House, rp.Consumer.U_Build, rp.Consumer.U_Flat),
                            Address_O = RegPointRowViewModel.FormatAddress(rp.Consumer.O_Local, rp.Consumer.O_Local_Secondary, rp.Consumer.O_Street, rp.Consumer.O_House, rp.Consumer.O_Build, rp.Consumer.O_Flat),
                            PhoneNumber = (device.DeviceType.Description.ToLower().Contains("plc")) ? linkS.PhoneNumber : linkD.PhoneNumber,
                            LinkIsOk = rp.RegPointFlags.IsLinkOk,
                            Letter = l,
                        }).ToList();

            var group = list
                .GroupBy(p => p.Id).ToList(); //Группируем для отбора писем
            //Фасуем данные
            foreach (var item in group)
            {
                var p = item.Last(); //Последний номер телефона
                var letters = item.Where(rp => rp.Letter != null).GroupBy(gr => gr.Letter.Id).Select(gr => gr.First().Letter).ToList();
                data.Add(new PointaLettersRowViewModel()
                {
                    Id = p.Id,
                    CustomerName = p.CustomerName,
                    Address_U = p.Address_U,
                    Address_O = p.Address_O,
                    PhoneNumber = p.PhoneNumber,
                    LinkIsOk = p.LinkIsOk,
                    Letters = letters
                });
            }
            //Возвращаем html
            return View("_PointsLetters", data);
        }

        [Authorize]
        public IActionResult HTML_ConsumerImportDataTable(int? Id) //Список точек учета по Id подстанции
        {
            var points = from p in db.RegPoints
                         where p.SubstationId == Id && !p.RegPointFlags.ImportConsummerData
                         where p.Status == RegPointStatus.Default
                         select new RegPointRowViewModel()
                         {
                             Id = p.Id,
                             Address = RegPointRowViewModel.FormatAddress(
                                p.Consumer.O_Local,
                                p.Consumer.O_Local_Secondary,
                                p.Consumer.O_Street,
                                p.Consumer.O_House,
                                p.Consumer.O_Build,
                                p.Consumer.O_Flat
                                )
                         };

            return View("_PointsConsumerNameImportTable", points);
        }
        
        [Authorize]
        public IActionResult ConsumerNameUAddressForm(int? Id) //Форма для редактирования информации о потребителе
        {
            var consumer = db.Consumers.FirstOrDefault(c => c.RegPointId == Id);
            if (consumer == null) return NotFound();
            return View("_ConsumerNameUAddressForm", consumer);
        }

        //Обновление инфы по потребителю ФИО и Юридический адресс
        [Authorize]
        [HttpPost]
        public void UpdateConsummerNameUAddress(Consumer model)
        {
            var consummerDB = db.Consumers.Find(model.Id);
            if (consummerDB == null) throw new Exception("Не найден Consumer в БД");
            int userId = GetUser().Id;
            //Action изменения данных
            consummerDB.RegPoint.AddAction(ActionTypeName.EditConsummer, userId, null);
            consummerDB.Name = model.Name;
            consummerDB.ContractNumber = model.ContractNumber;
            consummerDB.U_Index = model.U_Index;
            consummerDB.U_Local = model.U_Local;
            consummerDB.U_Street = model.U_Street;
            consummerDB.U_House = model.U_House;
            consummerDB.U_Build = model.U_Build;
            consummerDB.U_Flat = model.U_Flat;
            //Флаг импорта данных
            consummerDB.RegPoint.RegPointFlags.ImportConsummerData = true;
            db.SaveChanges();
        }

        /// <summary>
        /// Перенос точек учета в подстанцию (Id)
        /// </summary>
        /// <param name="data">Содержит в себе Id подстанции в которую нужно перенести точки учета и Points - массив id точек учета</param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        public void ReplaceTo([FromBody] JObject data)
        {
            var user = GetUser();
            if(user.Role.Name == "operator" || user.Role.Name == "administrator" || user.Role.Name == "pnr")
            {
                //Transform data
                var myType = new { Id = 0, Points = new List<int>() };
                var _data = JsonConvert.DeserializeAnonymousType(data.ToString(), myType);
                var subNew = db.Substations.Find(_data.Id);

                //New substation Id for each reg point
                _data.Points.ForEach(pId =>
                {
                    var point = db.RegPoints.Find(pId);
                    var subOld = point.Substation;
                    point.SubstationId = _data.Id;
                    //Actions
                    subOld.AddAction(ActionTypeName.ReplaceRegPoint, user.Id, "[" + point.Id + "] отсюда в " + subNew.Name);
                    subNew.AddAction(ActionTypeName.ReplaceRegPoint, user.Id, "["+ point.Id +"] сюда из " + subOld.Name);
                    point.AddAction(ActionTypeName.ReplaceRegPoint, user.Id, "из " + subOld.Name + " в " + subNew.Name);
                });
                db.SaveChanges();
            }
            else
            {
                throw new Exception("Недостаточно прав для изменения точек учета");
            }
        }

        #region Редактирование Точки учета
        [Authorize]
		public IActionResult Edit(int? id)
		{
			if (id == null) return NotFound();
			var regPoint = db.RegPoints.Find(id);
			if (regPoint == null) return NotFound();

			//Выпадающий список с местами установки
			ViewBag.InstallPlaceTypesList = new SelectList(db.InstallPlaceTypes, "Id", "Name", regPoint.InstallAct.InstallPlaceTypeId);
            ViewBag.IsDataConfirmed = regPoint.RegPointFlags.IsDataConfirmed;
			
			return View(regPoint);
		}
        
        [Authorize]
        [HttpGet]
        public string SetDataConfirmationFlag(int id, bool value)
        {
            var regpointFlag = db.RegPointFlags.FirstOrDefault(f => f.RegPointId == id);
            if (regpointFlag is null) { return "RegPoint not found"; }
            var regpoint = db.RegPoints.Find(id);

            regpointFlag.IsDataConfirmed = value;

            //Action о подтверждении данных
            int userId = GetUser().Id;
            if (value)
                regpoint.AddAction(ActionTypeName.FlagSet, userId, "Данные точки учета корректы");
            else
                regpoint.AddAction(ActionTypeName.FlagReset, userId, "Данные точки учета корректы");

            db.SaveChanges();
            return "Ok";
        }

        [Authorize]
		[HttpPost]
		public string UpdateConsummer(Consumer model)
		{
			var consummerDB = db.Consumers.Find(model.Id);
			if (consummerDB == null) throw new Exception("Не найден Consumer в БД");
            int userId = GetUser().Id;
            //Action об изменении данных
            int actionId = EnumsHelper.GetActionId(ActionTypeName.EditConsummer);
            var lastAction = consummerDB.RegPoint.RegPointActions.LastOrDefault(a => a.UserId == userId && a.ActionTypeId == actionId);
            //Если последнее изменение было больше чем 120 секунд назад то добавляем Action
            if (lastAction == null || (DateTime.Now - lastAction.Date).TotalSeconds > 120)
                consummerDB.RegPoint.AddAction(ActionTypeName.EditConsummer, userId, null);
            consummerDB.CopyDataFrom(model);
            db.SaveChanges();
			return "ОК";
		}

		[Authorize]
		[HttpPost]
		public string UpdateInstallAct(InstallAct model)
		{
			var installAct = db.InstallActs.Find(model.Id);
			if (installAct == null) throw new Exception("Не найден InstallAct в БД");
            int userId = GetUser().Id;
            //Action об изменении данных
            int actionId = EnumsHelper.GetActionId(ActionTypeName.EditInstallAct);
            var lastAction = installAct.RegPoint.RegPointActions.LastOrDefault(a => a.UserId == userId && a.ActionTypeId == actionId);
            //Если последнее изменение было больше чем 120 секунд назад то добавляем Action
            if (lastAction == null || (DateTime.Now - lastAction.Date).TotalSeconds > 120)
                installAct.RegPoint.AddAction(ActionTypeName.EditInstallAct, userId, null);
            //Изменение данных
            installAct.CopyDataFrom(model);
            db.SaveChanges();
			return "ОК";
		}

		[Authorize]
		[HttpPost]
		public string UpdateRegPointFlags(RegPointFlags model)
		{
			var regPointFlags = db.RegPointFlags.Find(model.Id);
			if (regPointFlags == null)
				throw new Exception("Не найден RegPointFlags в БД");
			regPointFlags.CopyDataFrom(model);
			db.SaveChanges();
			return "ОК";
		}
        #endregion
        #region Изменение флагов
        [Authorize]
        [HttpPost]
        public void Update_IsLinkOk(int id, bool newstatus)
        {
            var regPoint = db.RegPoints.Find(id);
            regPoint.RegPointFlags.IsLinkOk = newstatus;
            var user = GetUser();
            if (user.Role.Name != "pnr" && user.Role.Name != "administrator") throw new Exception("Недостаточно прав для изменения флага");
            //Обязательно сохраним это действие
            var action = (newstatus) ? ActionTypeName.FlagSet : ActionTypeName.FlagReset;
            regPoint.AddAction(action, user.Id, "Связь проверена");
            regPoint.Substation.AddAction(action, user.Id, "Связь проверена - ТУ №" + regPoint.Id);
            db.SaveChanges();
        }
        [Authorize]
        [HttpPost]
        public void Update_IsAscueChecked(int id, bool newstatus)
        {
            var regPoint = db.RegPoints.Find(id);
            regPoint.RegPointFlags.IsAscueChecked = newstatus;
            var user = GetUser();
            if (user.Role.Name != "pnr" && user.Role.Name != "administrator") throw new Exception("Недостаточно прав для изменения флага");
            //Обязательно сохраним это действие
            var action = (newstatus) ? ActionTypeName.FlagSet : ActionTypeName.FlagReset;
            regPoint.AddAction(action, user.Id, "Проверено для ПО АСКУЭ");
            regPoint.Substation.AddAction(action, user.Id, "Проверено для ПО АСКУЭ - ТУ №" + regPoint.Id);
            db.SaveChanges();
        }
        [Authorize]
        [HttpPost]
        public void Update_IsAscueOk(int id, bool newstatus)
        {
            var regPoint = db.RegPoints.Find(id);
            regPoint.RegPointFlags.IsAscueOk = newstatus;
            var user = GetUser();
            if (user.Role.Name != "pnr" && user.Role.Name != "administrator") throw new Exception("Недостаточно прав для изменения флага");
            //Обязательно сохраним это действие
            var action = (newstatus) ? ActionTypeName.FlagSet : ActionTypeName.FlagReset;
            regPoint.AddAction(action, user.Id, "Работает в ПО АСКУЭ");
            regPoint.Substation.AddAction(action, user.Id, "Работает в ПО АСКУЭ - ТУ №" + regPoint.Id);
            db.SaveChanges();
        }
        [Authorize]
        [HttpPost]
        public void Update_IsReplace(int id, bool newstatus)
        {
            var regPoint = db.RegPoints.Find(id);
            regPoint.RegPointFlags.IsReplace = newstatus;
            var user = GetUser();
            if (user.Role.Name != "pnr" && user.Role.Name != "administrator") throw new Exception("Недостаточно прав для изменения флага");
            //Обязательно сохраним это действие
            var action = (newstatus) ? ActionTypeName.FlagSet : ActionTypeName.FlagReset;
            regPoint.AddAction(action, user.Id, "Замена ПУ");
            regPoint.Substation.AddAction(action, user.Id, "Замена ПУ - ТУ №" + regPoint.Id);
            db.SaveChanges();
        }

        #endregion

        [HttpPost]
		[Authorize]
		public string Delete(int id)
		{
			var rp = db.RegPoints.Find(id);
			User user = db.Users.FirstOrDefault(u => u.Login == User.Identity.Name);
			if (rp != null && user != null)
			{
				return rp.Remove(user.Id);
			}
			return "Ошибка";
		}

        [HttpPost]
        [Authorize]
        public void ClearEnergyData(string inviteDate)
        {
            var date = DateTime.ParseExact(inviteDate, "dd_MM_yyyy", CultureInfo.InvariantCulture);
            var acts = (from p in db.RegPoints
                        join act in db.InstallActs on p.Id equals act.RegPointId
                        join l in db.Letters on p.Id equals l.RegPointId
                        where l.InviteDate == date
                        select act)
                        .ToList();
            User user = GetUser();
            if (user != null && (user.Role.Name == "operator" || user.Role.Name == "administrator"))
            {
                acts.ForEach(a => { a.T1 = ""; a.T2 = ""; a.Tsum = ""; });
            }
            db.SaveChanges();
        }

        [HttpPost]
        [Authorize]
        public void ClearConsumerData(string inviteDate)
        {
            /*var date = DateTime.ParseExact(inviteDate, "dd_MM_yyyy", CultureInfo.InvariantCulture);
            var acts = (from p in db.RegPoints
                        join act in db.InstallActs on p.Id equals act.RegPointId
                        join l in db.Letters on p.Id equals l.RegPointId
                        where l.InviteDate == date
                        select act)
                        .ToList();
            User user = GetUser();
            if (user != null && (user.Role.Name == "operator" || user.Role.Name == "administrator"))
            {
                acts.ForEach(a => { a.T1 = ""; a.T2 = ""; a.Tsum = ""; });
            }
            db.SaveChanges();*/
        }

    }
}