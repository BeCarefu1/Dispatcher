using GIPManager;
using ActGIPelectroWeb.ViewModels;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;


namespace ActGIPelectroWeb.Models
{
    public static class CommentsManager
    {
        public static List<CommentViewModel> GetSubstationComments(int substationId, StoreContext db, User user)
        {
            var coms = from c in db.CommentSubstations
                       where c.SubstationId == substationId
                       select new CommentViewModel
                       {
                           Author = c.User.Name,
                           Date = c.Date,
                           Text = c.Text,
                           Id = c.Id,
                           AuthorId = c.UserId,
                           CanDelete = false
                       };
            List<CommentViewModel> comments = coms.ToList();

            foreach (var comment in comments)
            {
                User author = db.Users.Find(comment.AuthorId);
                var span = DateTime.Now - comment.Date;
                if ((author.Id == user.Id && span.TotalMinutes < 30) || user.Role.Name == "administrator")
                {
                    comment.CanDelete = true;
                  
                }
            }
            return comments.ToList();
        }
    }
}
