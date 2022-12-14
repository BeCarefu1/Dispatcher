using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GIPManager;
using ActGIPelectroWeb.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ActGIPelectroWeb.Controllers
{
    public class ImportDataController : Controller
    {
        StoreContext db;
        IHostingEnvironment _env;
        public ImportDataController(StoreContext context, IHostingEnvironment env)
        {
            db = context;
            _env = env;
        }

        [HttpPost]
        public void EnergyDataToRegPoints([FromBody] JArray data)
        {
            var listData = data.ToObject<List<EnergyDataRow>>();

            foreach(var item in listData)
            {
                var act = db.InstallActs.FirstOrDefault(i => i.RegPointId == item.Id);
                if(act != null)
                {
                    act.T1 = item.E_T1.ToString();
                    act.T2 = item.E_T2.ToString();
                    act.Tsum = item.E_Sum.ToString();
                }
            }
            db.SaveChanges();
        }

        [HttpPost]
        public void ConsumerDataToRegPoints([FromBody] JArray data)
        {
            var listData = data.ToObject<List<ConsumerDataRow>>();

            foreach (var item in listData)
            {
                var consumer = db.Consumers.FirstOrDefault(i => i.RegPointId == item.Id);
                if (consumer != null)
                {
                    consumer.Name = item.C_Name.ToString();
                    consumer.ContractNumber = item.C_ContractNumber.ToString();
                }

                var act = db.InstallActs.FirstOrDefault(i => i.RegPointId == item.Id);
                if (act != null)
                {
                    act.Uninstalled_Serial = item.C_Uninstalled_Serial.ToString();
                }
            }
            db.SaveChanges();
        }

        [HttpPost]
        public void ReplaceDataToRegPoints([FromBody] JArray data)
        {
            var listData = data.ToObject<List<ReplaceDataRow>>();
            //Точки учета в БД
            foreach (var item in listData)
            {
                var rp = db.RegPointFlags.FirstOrDefault(p => p.RegPointId == item.Id);
                if (rp != null)
                    rp.IsReplace = true;
            }
            db.SaveChanges();
        }

        [HttpPost]
        public void SetFlagsIsAscueOk([FromBody] JArray serials)
        {
            var serialsList = new List<string>();
            foreach (var item in serials.Children())
            {
                var itemProperties = item.Children<JProperty>();
                var myElement = itemProperties.FirstOrDefault(x => x.Name == "SerialNumber");
                var myElementValue = myElement.Value; //This is a JValue type
                serialsList.Add(myElementValue.ToString());
            }
            //Точки учета в БД
            foreach(var serial in serialsList)
            {
                var rp = db.RegPoints.FirstOrDefault(p => p.Device.SerialNumber == serial);
                if (rp != null)
                    rp.RegPointFlags.IsAscueOk = true;
            }
            db.SaveChanges();
        }

        [HttpPost]
        public string ParceSerialsIsAscueOk(IFormFile file)
        {
            //Загруженные пользователем серийники для импорта
            var inputSerials = new List<string>();
            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                while (reader.Peek() >= 0)
                    inputSerials.Add(reader.ReadLine());
            }
            //Серийники в базе и флаги "Работает в Аскуэ"
            var bdSerials = (from rp in db.RegPoints
                            select new
                            {
                                rp.Device.SerialNumber,
                                rp.RegPointFlags.IsAscueOk
                            }).ToList();
            //List<Tuple<string, bool>> outputSerials = new List<Tuple<string, bool>>();
            var outputSerials = new List<object>().Select(t => new { SerialNumber = default(string), IsAscueOk = default(bool) }).ToList();
            foreach (var serial in inputSerials)
            {
                var tmp = bdSerials.FirstOrDefault(s => s.SerialNumber == serial);
                if (tmp != null)
                {
                    outputSerials.Add(tmp);
                    bdSerials.Remove(tmp);
                }
            }
            return JsonConvert.SerializeObject(outputSerials);
        }
    }
}