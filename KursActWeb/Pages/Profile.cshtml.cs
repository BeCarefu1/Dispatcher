using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using GIPManager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ActGIPelectroWeb.Pages
{
    [Authorize]
    public class ProfileModel : PageModel
    {
        private readonly StoreContext db;
        public ProfileModel(StoreContext context)
        {
            db = context;
        }

        public string Name { get; set; }
        public string Role { get; set; }
        public string Email { get; set; }
        public string Login { get; set; }
        public string PathImage { get; set; }

        public string GetRole(int? roleId)
        {
            switch (roleId)
            {
                case 1:
                    return "Администратор";
                    break;
                case 14:
                    return "Диспетчер";
                    break;
                case 15:
                    return "Мастер";
                    break;
                case 18:
                    return "Главный диспетчер";
                    break;
                case 19:
                    return "Главный инженер";
                    break;
                case 20:
                    return "Главный ПТО";
                    break;
                default:
                    return "Error";
                    break;
            }
        }

        public void OnGet()
        {
            var user = db.Users.FirstOrDefault(u => u.Login == User.Identity.Name);
            Name = user.Name;
            Role = GetRole(user.RoleId);
            Email = user.Email;
            Login = user.Login;
            if (user.Image != null)
            {
                PathImage = "data:image/jpeg;base64," + Convert.ToBase64String(user.Image);
            }
        }
    }
}