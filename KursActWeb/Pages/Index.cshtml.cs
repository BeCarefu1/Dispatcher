using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GIPManager;
using ActGIPelectroWeb.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ActGIPelectroWeb.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly StoreContext db; 
        public IndexModel(StoreContext context)
        {
            db = context;
        }

        public List<ContractCardViewModel> ContractsList { get; set; }
       

        public IActionResult OnGet()
        {
            ContractsList = new List<ContractCardViewModel>();
            foreach (var item in db.Contracts)
            {
                ContractsList.Add(new ContractCardViewModel(item.Id, db.Users.FirstOrDefault(s => s.Login == User.Identity.Name).Role.Name, db.Users.FirstOrDefault(s => s.Login == User.Identity.Name).Id));
                //if (User.IsInRole("master"))
                //    ContractsList.Add(new ContractCardViewModel(item.Id, "master", db.Users.FirstOrDefault(s => s.Login == User.Identity.Name).Id));
                //else if (User.IsInRole("dispatcher"))
                //    ContractsList.Add(new ContractCardViewModel(item.Id, "dispatcher", db.Users.FirstOrDefault(s => s.Login == User.Identity.Name).Id));
                //else
                //{
                //    ContractsList.Add(new ContractCardViewModel(item.Id, "administrator", db.Users.FirstOrDefault(s => s.Login == User.Identity.Name).Id));
                //}
            }
            return Page();
        }
    }
}