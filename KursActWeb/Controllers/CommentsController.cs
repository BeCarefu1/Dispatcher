using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GIPManager;
using ActGIPelectroWeb.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using ActGIPelectroWeb.Models;

namespace ActGIPelectroWeb.Controllers
{
    public class CommentsController : Controller
    {
        private StoreContext db;
        readonly IHostingEnvironment _env;
        public CommentsController(StoreContext context, IHostingEnvironment env)
        {
            db = context;
            _env = env;
        }

        private User GetUser()
        {
            return db.Users.FirstOrDefault(u => u.Login == User.Identity.Name);
        }

        [HttpPost]
        [Authorize]
        public IActionResult AddSubstationComment(int substationId, string text)
        {

            User user = db.Users.FirstOrDefault(u => u.Login == User.Identity.Name);
            Substation substation = db.Substations.Find(substationId);
            //Добавление комментария
            CommentSubstation comment = new CommentSubstation { UserId = user.Id, SubstationId = substation.Id, Text = text, Date = DateTime.Now };
            db.CommentSubstations.Add(comment);
            db.SaveChanges();

            //Добавление Экшона
            substation.AddAction(ActionTypeName.AddComment, user.Id, null);
            db.SaveChanges();

            //Добавление в таблицу непрочитанных для всех кроме автора
            var usr = from u in db.Users
                      where u.Id != user.Id
                      select u;
            var users = usr.ToList();
            foreach (var item in users)
            {
                db.UnreadSubstationComments.Add(new UnreadSubstationComment { UserId = item.Id, CommentSubstationId = comment.Id });
            }
            db.SaveChanges();

            return PartialView("/Views/Shared/_Comments.cshtml", CommentsManager.GetSubstationComments(substationId, db, user));
        }

        [HttpPost]
        [Authorize]
        public IActionResult DeleteSubstationComment(int substationId, int commentId)
        {
            User user = db.Users.FirstOrDefault(u => u.Login == User.Identity.Name);
            CommentSubstation comment = db.CommentSubstations.Find(commentId);
            Substation substation = db.Substations.Find(substationId);
            var span = DateTime.Now - comment.Date;



            //Удаление комментария
            if (user.Role.Name == "administrator" || (span.TotalMinutes < 30 && comment.UserId == user.Id))
            {
                //Удаление из списка непрочитанных
                db.UnreadSubstationComments.RemoveRange(db.UnreadSubstationComments.Where(c => c.CommentSubstationId == commentId));
                db.SaveChanges();

                db.CommentSubstations.Remove(comment);
                db.SaveChanges();

                //Добавление экшона
                substation.AddAction(ActionTypeName.DeleteComment, user.Id, null);
                db.SaveChanges();

            }
            return PartialView("/Views/Shared/_Comments.cshtml", CommentsManager.GetSubstationComments(substationId, db, user));
        }

        public string GetUnreadCommentsCount(string name)
        {
            // Input checking
            if (name is null)
            {
                return "🖕🖕🖕🖕🖕🖕🖕🖕🖕🖕";
            }

            var user = db.Users.FirstOrDefault(u => u.Login == name);
            if (user is null)
            {
                return "🖕🖕🖕🖕🖕 Нет такого пользователя 🖕🖕🖕🖕🖕";
            }

            // Counting unread comments for current user
            int count = db.UnreadSubstationComments.Where(u => u.UserId == user.Id).Count();

            return Convert.ToString(count);
        }

        [HttpPost]
        [Authorize]
        public string MarkCommentAsRead(int? id)
        {
            if (id is null)
            {
                return "🖕🖕🖕🖕🖕🖕🖕🖕🖕🖕";
            }

            var comment = db.UnreadSubstationComments.Where(u => u.UserId == GetUser().Id).FirstOrDefault(s => s.CommentSubstationId == id);

            if (comment is null)
            {
                return "🖕🖕🖕🖕🖕 Нет такого коммента 🖕🖕🖕🖕🖕";
            }

            db.UnreadSubstationComments.Remove(comment);
            db.SaveChanges();

            return "OK";
        }

        [Authorize]
        public string MarkAllCommentsAsRead()
        {
            var comments = db.UnreadSubstationComments.Where((u => u.UserId == GetUser().Id)).ToList();
            if (comments is null)
            {
                return "🖕🖕🖕🖕🖕 Что-то пошло не так 🖕🖕🖕🖕🖕";
            }

            db.UnreadSubstationComments.RemoveRange(comments);
            db.SaveChanges();

            return "OK";
        }
    }
}