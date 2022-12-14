using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GIPManager;
using ActGIPelectroWeb.ViewModels;
using System.Security.Cryptography;
using System.Text;

namespace ActGIPelectroWeb.Controllers
{
    public class ShiftController : Controller
    {
        StoreContext db;
        public ShiftController(StoreContext context)
        {
            db = context;
        }
        // GET: ShiftController
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public string PassShift()
        {
            int id_user = db.Users.FirstOrDefault(u => u.Login == User.Identity.Name).Id;
            ShiftSchedule shiftSchedule = new ShiftSchedule();
            shiftSchedule = db.ShiftSchedule.LastOrDefault();
            //if (db.Users.FirstOrDefault(s => s.Login == User.Identity.Name).Password != GetMd5Hash(MD5.Create(), password))
            //    return "Пароли не совпадает!";
            shiftSchedule.PassShift(id_user);
            //commentDispatcher.CreateComment(id_user, 3, comment.Replace("\n", "<br />"));
            OperationalLog operational = new OperationalLog();
            operational.RecoilTime = DateTime.Now;
            operational.UserId = db.Users.FirstOrDefault(s => s.Login == User.Identity.Name).Id;
            operational.Description = "Сдал смену";
            operational.AddCommentAsync();
            return "";
        }

        public string TakeShift()
        {
            int id_user = db.Users.FirstOrDefault(u => u.Login == User.Identity.Name).Id;
            //CommentDispatcher commentDispatcher = new CommentDispatcher();
            ShiftSchedule shiftSchedule = new ShiftSchedule();
            //if (db.Users.FirstOrDefault(s => s.Login == User.Identity.Name).Password != GetMd5Hash(MD5.Create(), password))
            //    return "Пароли не совпадает!";
            //Поправить
            //if(comment != null)
            //{
            //    comment = comment.Replace("\n", "<br />");
            //}
            shiftSchedule.AcceptShift(id_user);
            //string name = db.
            OperationalLog operational = new OperationalLog();
            operational.UserId = db.Users.FirstOrDefault(s => s.Login == User.Identity.Name).Id;
            operational.RecoilTime = DateTime.Now;
            operational.Description = "Принял смену";
            operational.AddCommentAsync();
            //commentDispatcher.CreateComment(id_user, 1, comment);
            return "";
        }

       
    }
}
