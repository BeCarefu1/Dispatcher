using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GIPManager;
using ActGIPelectroWeb.ViewModels;

namespace ActGIPelectroWeb.Pages
{
    public class CommentPageModel : PageModel
    {
        private readonly StoreContext db;
        public CommentPageModel(StoreContext context)
        {
            db = context;
        }

        public string User_Login_Check { get; set; }
        public DateTime User_EndWork_Chek { get; set; }
        public int positionScroll { get; set; }


        public List<CommentDispatcherViewModel> CommentDispatcherList { get; set; }

        public void OnGet()
        {

            positionScroll = Models.GlobalModel.GetPositionScroll() ;

            if (db.ShiftSchedule.Count() != 0)
            {
                User_Login_Check = (from a in db.ShiftSchedule
                                    join b in db.Users on a.Id_Dispatcher equals b.Id
                                    select b.Login).LastOrDefault();
                User_EndWork_Chek = db.ShiftSchedule.LastOrDefault().EndDate;
            }

            DateTime sDate = Models.GlobalModel.sDate;
            DateTime eDate = Models.GlobalModel.eDate;
            
            /*if(sDate != Convert.ToDateTime("01.01.0001 00:00:00") && eDate == Convert.ToDateTime("01.01.0001 00:00:00"))
                CommentDispatcherList = (from a in db.CommentDispatcher
                                         join b in db.Users on a.Id_User equals b.Id
                                         join c in db.EventTable on a.Id_Event equals c.Id
                                         where a.Date >= sDate
                                         select new CommentDispatcherViewModel
                                         {
                                             Id = a.Id,
                                             Event = c.Event,
                                             Id_User = b.Id,
                                             User = b.Name,
                                             Date = a.Date,
                                             Time = a.Date,
                                             Comment = a.Comment,
                                             StatusComment = a.Status
                                         }).ToList();
            if (eDate != Convert.ToDateTime("01.01.0001 00:00:00") && sDate == Convert.ToDateTime("01.01.0001 00:00:00"))
                CommentDispatcherList = (from a in db.CommentDispatcher
                                         join b in db.Users on a.Id_User equals b.Id
                                         join c in db.EventTable on a.Id_Event equals c.Id
                                         where a.Date <= eDate
                                         select new CommentDispatcherViewModel
                                         {
                                             Id = a.Id,
                                             Event = c.Event,
                                             Id_User = b.Id,
                                             User = b.Name,
                                             Date = a.Date,
                                             Time = a.Date,
                                             Comment = a.Comment,
                                             StatusComment = a.Status
                                         }).ToList();

            if (sDate != Convert.ToDateTime("01.01.0001 00:00:00") && eDate != Convert.ToDateTime("01.01.0001 00:00:00"))
                 CommentDispatcherList = (from a in db.CommentDispatcher
                                         join b in db.Users on a.Id_User equals b.Id
                                         join c in db.EventTable on a.Id_Event equals c.Id
                                         where a.Date >= sDate && a.Date <= eDate
                                         select new CommentDispatcherViewModel
                                         {
                                             Id = a.Id,
                                             Event = c.Event,
                                             Id_User = b.Id,
                                             User = b.Name,
                                             Date = a.Date,
                                             Time = a.Date,
                                             Comment = a.Comment,
                                             StatusComment = a.Status
                                         }).ToList();
            if (sDate == Convert.ToDateTime("01.01.0001 00:00:00") && eDate == Convert.ToDateTime("01.01.0001 00:00:00"))
                CommentDispatcherList = (from a in db.CommentDispatcher
                                         join b in db.Users on a.Id_User equals b.Id
                                         join c in db.EventTable on a.Id_Event equals c.Id
                                         select new CommentDispatcherViewModel
                                         {
                                             Id = a.Id,
                                             Event = c.Event,
                                             Id_User = b.Id,
                                             User = b.Name,
                                             Date = a.Date,
                                             Time = a.Date,
                                             Comment = a.Comment,
                                             StatusComment = a.Status
                                         }).ToList();
            */
            
        }
    }
}
