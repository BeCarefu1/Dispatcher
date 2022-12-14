using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GIPManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ActGIPelectroWeb.Pages
{
    public class DocPageModel : PageModel
    {
        public readonly StoreContext db = new StoreContext();


        public class DocList
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public DateTime Date { get; set; }
            public string UserName { get; set; }
            public int UserId { get; set; }
        }

        public int UserId { get; set; }

        public List<DocList> Docs { get; set; }
        public void OnGet()
        {
            UserId = db.Users.FirstOrDefault(s => s.Login == User.Identity.Name).Id;

            Docs = (from a in db.Documents
                    join b in db.Users on a.UserId equals b.Id
                    select new DocList
                    {
                        Id = a.Id,
                        Name = a.Name,
                        Date = a.Date,
                        UserName = Models.ShortNameModel.ShortName(b.Name)
                    }).ToList();
        }
    }
}
