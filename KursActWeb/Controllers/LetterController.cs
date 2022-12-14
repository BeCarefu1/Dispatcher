using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using GIPManager;
using ActGIPelectroWeb.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ActGIPelectroWeb.Controllers
{
    public class LetterController : Controller
    {
        private readonly StoreContext db;
        public LetterController(StoreContext context)
        {
            db = context;
        }

        [Authorize]
        [HttpPost]
        public void Add(int id, DateTime inviteDate)
        {
            var rp = db.RegPoints.Find(id);
            var user = db.Users.FirstOrDefault(u => u.Login == User.Identity.Name);
            if (user.Role.Name == "operator" || user.Role.Name == "administrator")
            {
                rp.Letters.Add(new Letter()
                {
                    RegPointId = id,
                    InviteDate = inviteDate,
                    OutNumber = "КУРС " + id.ToString("00000"),
                    DateLetter = DateTime.Now
                });
                rp.AddAction(ActionTypeName.LetterAdd, user.Id, null);
                db.SaveChanges();
            }
            else
            {
                throw new Exception("Недостаточно прав для добавления письма");
            }
        }
        [Authorize]
        [HttpPost]
        public void Delete(int letterId)
        {
            var letter = db.Letters.Find(letterId);
            var user = db.Users.FirstOrDefault(u => u.Login == User.Identity.Name);
            if (user.Role.Name == "operator" || user.Role.Name == "administrator")
            {
                letter.RegPoint.AddAction(ActionTypeName.LetterDelete, user.Id, letter.Id + " " + letter.InviteDate.ToShortDateString());
                db.Letters.Remove(letter);
                db.SaveChanges();
            }
            else
            {
                throw new Exception("Недостаточно прав для удаления письма");
            }
        }

        [Authorize]
        [HttpPost]
        public void Update_LetterPrinted(int id, bool newstatus)
        {
            var letter = db.Letters.Find(id);
            var regPoint = db.RegPoints.Find(letter.RegPointId);
            var user = db.Users.FirstOrDefault(u => u.Login == User.Identity.Name);
            if (user.Role.Name == "operator" || user.Role.Name == "administrator")
            {
                letter.Printed = newstatus;
                //Обязательно сохраним это действие
                var action = (newstatus) ? ActionTypeName.FlagSet : ActionTypeName.FlagReset;
                regPoint.AddAction(action, user.Id, "Письмо распечатано id:" + letter.Id + " " + letter.InviteDate);
                db.SaveChanges();
            }
            else
            {
                throw new Exception("Недостаточно прав для изменения флага письма");
            }
        }

        [Authorize]
        [HttpPost]
        public void UpdateTrackNumber(int id, string newTrackNumber)
        {
            var letter = db.Letters.Find(id);
            var regPoint = db.RegPoints.Find(letter.RegPointId);
            var user = db.Users.FirstOrDefault(u => u.Login == User.Identity.Name);
            letter.TrackNumber = newTrackNumber;
            //Обязательно сохраним это действие
            var action = ActionTypeName.LetterTrackNumEdit;
            regPoint.AddAction(action, user.Id, "на " + newTrackNumber);
            db.SaveChanges();
        }
        [Authorize]
        public IActionResult HTML_LetterReestrsList()
        {
            var ltrs = (from l in db.Letters
                        select new 
                        {
                            InviteDate = l.InviteDate.ToString("dd.MM.yyyy"),
                            LetterPrinted = l.Printed,
                        }).GroupBy(l => l.InviteDate).ToList();
            var model = ltrs.Select(gr => (gr.Key, gr.Count(), gr.Count(l => l.LetterPrinted))).ToList();
            return View("_LetterReestrsList", model);
        }
        [Authorize]
        public IActionResult HTML_LettersTable(string inviteDate)
        {
            var date = DateTime.ParseExact(inviteDate, "dd_MM_yyyy", CultureInfo.InvariantCulture);
            var ltrs = from l in db.Letters
                       where l.InviteDate == date
                       orderby l.RegPoint.Consumer.Name // добавила сортировку по ФИО
                       select new LetterInfoInReestr()
                       {
                           LetterId = l.Id,
                           InviteDate = l.InviteDate,
                           OutNumber = l.OutNumber,
                           LetterPrinted = l.Printed,
                           TrackNumber = l.TrackNumber,

                           ConsumerName = l.RegPoint.Consumer.Name,
                           UAdress = RegPointRowViewModel.FormatUAddress(l.RegPoint.Consumer),

                           T1 = l.RegPoint.InstallAct.T1,
                           T2 = l.RegPoint.InstallAct.T2,
                           Tsum = l.RegPoint.InstallAct.Tsum,

                           LinkIsOk = l.RegPoint.RegPointFlags.IsLinkOk,
                           RegPointId = l.RegPointId,
                           SubstationId = l.RegPoint.SubstationId,
                           SubstationName = l.RegPoint.Substation.Name,

                           SerialNumber = "-",
                           PhoneNumber = "-"
                       };
            return View("_LettersTable", ltrs.ToList());
        }
    }
}