using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using GIPManager;
using ActGIPelectroWeb.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ActGIPelectroWeb.Controllers
{
    public class AccountController : Controller
    {
        StoreContext db;
        public AccountController(StoreContext context)
        {
            db = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                User user = new User();
                using (MD5 md5hash = MD5.Create())
                {
                    user = await db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Login == model.Login && u.Password == GetMd5Hash(md5hash, model.Password));
                    //user.StatusId = 2;
                    //user.UpdateUserStatus();
                    //um.UpdateUserInfo(user.Id, null, null, null, 2);
                }
                if (user != null)
                {
                    await Authenticate(user); // аутентификация
                    await Models.EventHistoryModel.AddEvent("Вошёл в систему", user.Login);
                    return RedirectToAction("Index", "Home");
                }
                ViewData["Error"] = "Некорректные логин и(или) пароль";
            }
            return View(model);
        }
        private async Task Authenticate(User user)
        {
            // создаем один claim
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, user.Login),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, user.Role?.Name)
                //new Claim(ClaimsIdentity.)
            };
            // создаем объект ClaimsIdentity
            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);
            // установка аутентификационных куки
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }

        public async Task<IActionResult> Logout()
        {
            //User user = db.Users.FirstOrDefault(s => s.Login == User.Identity.Name);
            //user.StatusId = 1;
            //user.UpdateUserStatus();
            await HttpContext.SignOutAsync();
            await Models.EventHistoryModel.AddEvent("Вышел из системы", User.Identity.Name);
            return Redirect("~/Index");
        }

        static string GetMd5Hash(MD5 md5Hash, string input)
        {

            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }
	}
}