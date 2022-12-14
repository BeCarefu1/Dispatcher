using System;
using System.Collections.Generic;
using System.Linq;
using GIPManager;
using ActGIPelectroWeb.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StoreGIPManager.Substation;

namespace ActGIPelectroWeb.Controllers
{
    public class SubstationController : Controller
    {
        private StoreContext db;
        readonly IHostingEnvironment _env;
        public SubstationController(StoreContext context, IHostingEnvironment env)
        {
            db = context;
            _env = env;
        }
        private User GetUser()
        {
            return db.Users.FirstOrDefault(u => u.Login == User.Identity.Name);
        }

        [Authorize]
        public IActionResult GetTable(int? Id) //PartialView таблица объектов по Id района
        {
            List<SubstationRowViewModel> model = new List<SubstationRowViewModel>();
            if (Id != null)
            {
                NetRegion region = db.NetRegions.Find(Id);
                if (region != null)
                {
                    var substations = from substation in db.Substations
                                      where substation.NetRegionId == Id
                                      select new SubstationRowViewModel(substation);
                    //model = substations.OrderBy(substation => substation.Name).ToList();
                    model = substations.ToList();
                    //foreach (var item in db.Substations.Where(s => s.NetRegionId == Id))
                    //{
                    //    model.Add(new SubstationRowViewModel(item));
                    //}
                }
            }
            return View("_SubstationsTable", model);
        }

        [Authorize]
        [HttpGet]
        public string AllPoints_json(int id)
        {
            SubstationRepository sr = new SubstationRepository(db);
            return JsonConvert.SerializeObject(sr.RegPoints(id));
        }

        /// <summary>
        /// Массив подстанций в том же районе что и эта подстанция (id)
        /// </summary>
        /// <param name="id">Id подстанции</param>
        /// <returns></returns>
        [Authorize]
        [HttpGet]
        public string SubstationsNear_json(int id)
        {
            SubstationRepository sr = new SubstationRepository(db);
            Dictionary<int, string> substations = new Dictionary<int, string>();
            sr.NearSubstations(id).ForEach(s =>
            {
                substations.Add(s.Id, s.Name + " [" + s.RegPoints.Count + "] " + s.NetRegion.Name);
            });
            return JsonConvert.SerializeObject(substations);
        }

        [Authorize]
        [HttpPost]
        public string AddSubstation(string name, int idRegion)
        {
            if (name != "" && idRegion != 0)
            {
                User user = db.Users.FirstOrDefault(u => u.Login == User.Identity.Name);
                NetRegion region = db.NetRegions.Find(idRegion);
                if(user != null)
                {
                    string result = region.AddSubstation(name, user.Id);
                    db.SaveChanges();
                    return result;
                }
            }
            return "Ошибка при добавлении. Попробуйте позже снова.";
        }

        [HttpPost]
        [Authorize]
        public string Delete(int id)
        {
            var s = db.Substations.Find(id);
            var user = GetUser();
            if (s != null && user != null && (user.Role.Name == "administrator" || user.Role.Name == "operator" || user.Role.Name == "pnr"))
            {
                if (s.RegPoints.Count() > 0) return "В подстанции есть точки учета. Удаление невозможно.";
                return s.Remove(user.Id);
            }
            return "Ошибка";
        }
        
        [HttpPost]
        [Authorize]
        public string Rename(string newName, int id, int contractId)
        {
            var substation = db.Substations.FirstOrDefault(s => s.Name == newName && s.NetRegion.ContractId == contractId);
            User user = db.Users.FirstOrDefault(u => u.Login == User.Identity.Name);
            if (substation != null) return "Ошибка! Подстанция с таким номером уже существует!";
            else
            {
                if(user != null)
                {
                    substation = db.Substations.Find(id);
                    substation.AddAction(ActionTypeName.RenameSubstation, user.Id, substation.Name + " => " + newName);
                    substation.Name = newName;
                    db.SaveChanges();
                    return "Подстанция переименована";
                }
                else 
                    return "Пользователь не авторизован";
            }
         }

        [HttpPost]
        [Authorize]
        public string SetFlag(int id, bool isInstallationDone, bool isPropSchemeDone, bool isBalanceDone, bool isKS2Done)
        {
	        var substation = db.Substations.Find(id);
	        if (substation is null)
		        return "🤷🤷🤷🤷 Где подстанция? 🤷🤷🤷🤷";
	        else
	        {
                if(substation.IsInstallationDone != isInstallationDone)
                {
                    substation.IsInstallationDone = isInstallationDone;
                    var action = (isInstallationDone) ? ActionTypeName.FlagSet : ActionTypeName.FlagReset;
                    substation.AddAction(action, GetUser().Id, "СМР закончены");
                }
                if (substation.IsPropSchemeDone != isPropSchemeDone)
                {
                    substation.IsPropSchemeDone = isPropSchemeDone;
                    var action = (isPropSchemeDone) ? ActionTypeName.FlagSet : ActionTypeName.FlagReset;
                    substation.AddAction(action, GetUser().Id, "Поопорная схема готова");
                }
                if (substation.IsBalanceDone != isBalanceDone)
                {
                    substation.IsBalanceDone = isBalanceDone;
                    var action = (isBalanceDone) ? ActionTypeName.FlagSet : ActionTypeName.FlagReset;
                    substation.AddAction(action, GetUser().Id, "Баланс сведен");
                }
                if (substation.IsKS2Done != isKS2Done)
                {
                    substation.IsKS2Done = isKS2Done;
                    var action = (isKS2Done) ? ActionTypeName.FlagSet : ActionTypeName.FlagReset;
                    substation.AddAction(action, GetUser().Id, "КС-2 сделана");
                }
                db.SaveChanges();
		        return "Флаги подстанции обновлены";
	        }
        }
        [HttpPost]
        [Authorize]
        public string AddPhoneNumber(string number, int id)
        {
            Substation substation = db.Substations.Find(id);
            if (substation != null)
            {
                SubstationLink substationLink = new SubstationLink() { SubstationId = id, PhoneNumber = number };
                db.SubstationLinks.Add(substationLink);
                db.SaveChanges();
                return "Номер добавлен!";
            }
            else return "Ошибка! Подстанция не найдена!";
        }
    }
}