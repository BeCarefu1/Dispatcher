using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using GIPManager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ActGIPelectroWeb.Controllers
{
    public class UsersController : Controller
    {
        StoreContext db;
        public UsersController(StoreContext context)
        {
            db = context;
        }

      

        [Authorize]
        public IActionResult UsersList()
        {
            //Выпадающий список с ролями
            return View();
        }

        [Authorize]
        public string GetRoles()
        {
            Dictionary<int, string> roles = new Dictionary<int, string>();
            db.Roles.ToList().ForEach(r =>
            {
                string roleNameRU = r.Name;
                if (roleNameRU == "administrator") roleNameRU = "Администраторы";
                if (roleNameRU == "storekeeper") roleNameRU = "Кладовщики";
                if (roleNameRU == "tuner") roleNameRU = "Настройщики";
                if (roleNameRU == "engineer") roleNameRU = "Инженеры";
                if (roleNameRU == "mounter") roleNameRU = "Монтажники";
                if (roleNameRU == "curator") roleNameRU = "Кураторы";

                if (roleNameRU == "dispatcher") roleNameRU = "Диспетчер";
                if (roleNameRU == "master") roleNameRU = "Мастер";
                if (roleNameRU == "podpisant") roleNameRU = "Подписант";
                if (roleNameRU == "worker") roleNameRU = "Рабочий";
                if (roleNameRU == "chief_dispatcher") roleNameRU = "Гл. диспетчер";

                roles.Add(r.Id, roleNameRU + " [" + r.Users.Count() + "]");
            });
            return JsonConvert.SerializeObject(roles);
        }

        [Authorize]
        public IActionResult GetRoleUsersTable(int id)
        {
            return View("_usersTable", db.Roles.Find(id).Users);
        }

        public IActionResult GetMaster(int id)
        {
            return View("_masterList", db.Roles.Find(id).Users);
        }

        public IActionResult GetRegion(int id)
        {
            return View("_regionList", db.NetRegions);
        }

        public IActionResult GetTypeWorker(int id)
        {
            return View("_typeworkerList", db.WorkerTypes);
        }

        [Authorize]
        public string UserInfo(int id)
        {
            var user = db.Users.Find(id);
            if (user != null)
                return JsonConvert.SerializeObject(new { user.Id, user.Name, user.Login, user.Email, user.RoleId, user.WorkerID });
            else
                return "{ }";
        }
        public class PostUserModel
        {
            public string Email { get; set; }
            public int Id { get; set; }
            public string Login { get; set; }
            public string Name { get; set; }
        }

        [Authorize]
        [HttpPost]
        public void UpdateUserInfo([FromBody] JObject data)
        {
            var user = data.ToObject<User>();
            UsersManager um = new UsersManager(db);
            um.UpdateUserInfo(user.Id, user.Login, user.Email, user.Name);
            var users = db.Users.Find(user.Id);
            var worker = db.Workers.Find(users.WorkerID);
            um.UpdateWorkerInfo(worker.Id, user.Name);
        }

        [Authorize]
        [HttpPost]
        public void UpdateUserRole([FromBody] JObject data)
        {
            var user = data.ToObject<User>();
            UsersManager um = new UsersManager(db);
            um.UpdateRole(user.Id, (int)user.RoleId);
            um.UpdateType(user.Id, (int)user.RoleId);
        }
        [Authorize]
        [HttpPost]
        public void NewPass([FromBody] JObject data)
        {
            var user = data.ToObject<User>();
            UsersManager um = new UsersManager(db);
            um.UpdatePass(user.Id, user.Password);
        }

        public class RegAndMast
        {

        }

        [Authorize]
        [HttpPost]
        //Добавление данных пользователя
        public void AddNewUser([FromBody] JObject data)
        {
            try
            {
                var user = data.ToObject<User>();
                var worker = data.ToObject<Worker>();
                int idWorker = 0;
                UsersManager um = new UsersManager(db);
                if (worker == null)
                {
                    um.AddUser(user, idWorker);
                }
                else
                {
                    string[] fioList = user.Name.Split(' ');
                    switch(fioList.Length)
                    {
                        case 1:
                            worker.Surname = fioList[0];
                            break;

                        case 2:
                            worker.Surname = fioList[0];
                            worker.Name = fioList[1];
                            break;
                        case 3:
                            worker.Surname = fioList[0];
                            worker.Name = fioList[1];
                            worker.MIddlename = fioList[2];
                            break;
                        default:
                            break;
                    }
                    
                    if (worker.WorkerTypeId == 0) worker.WorkerTypeId = 1;
                    if (worker.ID_NetRegion == 0) worker.ID_NetRegion = (from a in db.NetRegions
                                                                         join b in db.Workers on a.Id equals b.ID_NetRegion
                                                                         join c in db.Users on b.Id equals c.WorkerID
                                                                         where c.Id == worker.WorkerId
                                                                         select a.Id).FirstOrDefault();

                    idWorker = um.AddWorker(worker);
                    um.AddUser(user, idWorker);


                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            //um.AddUser(user);
        }

        [Authorize]
        [HttpPost]
        public void DeleteUser([FromBody] JObject data)
        {
            var user = data.ToObject<User>();
            var users = db.Users.Find(user.Id);
            UsersManager um = new UsersManager(db);
            um.DeleteUser(user.Id);
            if(users.RoleId == 15 || users.RoleId == 17)
                DeleteWorker((int)users.WorkerID);
        }

        public void DeleteWorker(int userId)
        {
            UsersManager um = new UsersManager(db);
            um.DeleteWorker(userId);
        }

        //Sисправить
        [Authorize]
        public string GetDisp()
        {
            string NameDispatcher = "";
            int id_disp = db.ShiftSchedule.LastOrDefault(s => s.EndDate.ToString("dd/MM/yyyy") == "01.01.0001").Id_Dispatcher;
            if (db.ShiftSchedule.Count(s => s.EndDate == Convert.ToDateTime("01.01.0001 00:00:00")) != 0)
                NameDispatcher = db.Users.LastOrDefault(u => u.Id == id_disp).Name;
            string[] arr_disp = NameDispatcher.Split(' ');
            NameDispatcher = "На смене: " + arr_disp[0] + " ";
            for (int i = 1; i < arr_disp.Length; i++)
            {
                NameDispatcher += (arr_disp[i])[0] + ".";
            }
            return JsonConvert.SerializeObject(NameDispatcher);
        }


        [Authorize]
        public string AddImage(IFormFile Image)
        {
            int userId = db.Users.FirstOrDefault(s => s.Login == User.Identity.Name).Id;
            User user = db.Users.Find(userId);

            try
            {
                byte[] files = null;
                using (var binaryReader = new BinaryReader(Image.OpenReadStream()))
                {
                    files = binaryReader.ReadBytes((int)Image.Length);
                }
                user.Image = files;
                UsersManager um = new UsersManager(db);
                um.AddImageForUser(user);
                // добавить в User поле image
                return "ok";
            }
            catch(Exception ex)
            {
                return ex.Message;
            }

           
        }

        /*public IActionResult AddImage(IFormFile Image)
        {
            int userId = db.Users.FirstOrDefault(s => s.Login == User.Identity.Name).Id;
            User user = db.Users.Find(userId);

            try
            {
                byte[] files = null;
                using (var binaryReader = new BinaryReader(Image.OpenReadStream()))
                {
                    files = binaryReader.ReadBytes((int)Image.Length);
                }
                user.Image = files;
                UsersManager um = new UsersManager(db);
                um.AddImageForUser(user);
                // добавить в User поле image
            }
            catch
            {
                return RedirectToAction("Index", "Home");
            }

            return RedirectToAction("Index", "Home");
        }*/

        [Authorize]
        public string GetUserImage()
        {
            if (db.Users.FirstOrDefault(s => s.Login == User.Identity.Name).Image != null)
            {
                string pathImage = "data:image/jpeg;base64," + Convert.ToBase64String(db.Users.FirstOrDefault(s => s.Login == User.Identity.Name).Image);
                return JsonConvert.SerializeObject(pathImage);
            }
            else
            {
                return JsonConvert.SerializeObject("/images/avatar.jpg");
            }
            
        }

    }
}