using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GIPManager;
using Microsoft.EntityFrameworkCore;

namespace ActGIPelectroWeb.Pages
{
    public class UnreadCommentsModel : PageModel
    {
        private readonly StoreContext db;

        public User CurrentUser { get; set; }

        public List<UnreadCommentModel> Comments { get; set; }

        public UnreadCommentsModel(StoreContext context)
        {
            db = context;
        }

        private User GetUser()
        {
            return db.Users.FirstOrDefault(u => u.Login == User.Identity.Name);
        }

        public void OnGet(int? page)
        {
            //Checking input for correct values
            if (page is null)
            {
                page = 0;
            }

            CurrentUser = GetUser();

            // Getting all unread comments for current user
            Comments = (from c in db.UnreadSubstationComments
                        where c.UserId == CurrentUser.Id
                        join comment in db.CommentSubstations on c.CommentSubstationId equals comment.Id
                        join substation in db.Substations on comment.SubstationId equals substation.Id
                        join user in db.Users on comment.UserId equals user.Id
                        select new UnreadCommentModel()
                        {
                            Id = comment.Id,
                            UserName = user.Name,
                            Comment = comment.Text,
                            TimeStamp = comment.Date,
                            SubstationName = substation.Name,
                            SubstationId = substation.Id
                        }).AsNoTracking().ToList();
        }
    }

    public class UnreadCommentModel{
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Comment { get; set; }
        public DateTime TimeStamp { get; set; }
        public string SubstationName { get; set; }
        public int SubstationId { get; set; }
    }
}
