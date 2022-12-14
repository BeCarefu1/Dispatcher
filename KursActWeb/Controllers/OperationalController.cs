using GIPManager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ActGIPelectroWeb.Controllers
{
    public class OperationalController : Controller
    {
        public StoreContext db = new StoreContext();

        [Authorize]

        public string AddComment(string comment)
        {
            OperationalLog operational = new OperationalLog();
            operational.RecoilTime = DateTime.Now;
            operational.UserId = db.Users.FirstOrDefault(s => s.Login == User.Identity.Name).Id;
            operational.Description = comment;
            //operational.Numb = db.OperationalLog.LastOrDefault().Numb + 1;
            return JsonConvert.SerializeObject(operational.AddComment());
        }

        [Authorize]

        public string AddVisa(int id, string comment)
        {
            OperationalLog operational = db.OperationalLog.Find(id);
            operational.VisaUserId = db.Users.FirstOrDefault(s => s.Login == User.Identity.Name).Id;
            operational.TineOfApplication = DateTime.Now;
            operational.Remarks = comment;
            return JsonConvert.SerializeObject(operational.AddVisa());
        }

        public string DelEvent(int id)
        {
            OperationalLog operational = db.OperationalLog.Find(id);
            return JsonConvert.SerializeObject(operational.DelComment());
        }

        public IActionResult Index(string sortOrder)
        {
            
            return View();
        }
    }
}
