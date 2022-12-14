using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ActGIPelectroWeb.Models;
using ActGIPelectroWeb.EmailServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Hosting;

namespace ActGIPelectroWeb.Controllers
{
    public class EmailConfigController : Controller
    {
        readonly IHostingEnvironment _env;
        readonly string ConfigFilePath;
        public EmailConfigController(IHostingEnvironment env)
        {
            _env = env;
            ConfigFilePath = env.ContentRootPath + "/emailconfig.json";
        }

        [Authorize]
        [HttpPost]
        public async void SendEmailDistribution(string name)
        {
            //Загрузим конфигурацию из файла .json
            EmailConfig config = JsonConvert.DeserializeObject<EmailConfig>(System.IO.File.ReadAllText(ConfigFilePath));
            var dist = config.DistributionList.FirstOrDefault(d => d.Name == name);
            if (dist != null)
            {
                
                //Если не определен контент письма, то не отправляем
                if (dist.ContentType == EMailContentType.Undefined) return;

                EmailService es = new EmailService();
                EmailContent content = new EmailContent(dist.ContentType);
                await es.SendEmailAsync(dist.EMailList, content.Subject, content.Message, content.FileData, content.FileName);
                //Сохраним дату последней отправки
                dist.LastSendDate = DateTime.Now;
                System.IO.File.WriteAllText(ConfigFilePath, JsonConvert.SerializeObject(config));
            }
        }

        [Authorize]
        [HttpPost]
        public void AddEmailDistribution(string name)
        {
            //Загрузим конфигурацию из файла .json
            EmailConfig config = JsonConvert.DeserializeObject<EmailConfig>(System.IO.File.ReadAllText(ConfigFilePath));
            if(config.DistributionList.FirstOrDefault(d => d.Name == name) == null)
            {
                config.DistributionList.Add(new Distribution() { Name = name });
                //Запишем в файл
                System.IO.File.WriteAllText(ConfigFilePath, JsonConvert.SerializeObject(config));
            }
        }
        [Authorize]
        [HttpPost]
        public void DeleteEmailDistribution(string name)
        {
            //Загрузим конфигурацию из файла .json
            EmailConfig config = JsonConvert.DeserializeObject<EmailConfig>(System.IO.File.ReadAllText(ConfigFilePath));
            var dist = config.DistributionList.FirstOrDefault(d => d.Name == name);
            if (dist != null)
            {
                config.DistributionList.Remove(dist);
                //Запишем в файл
                System.IO.File.WriteAllText(ConfigFilePath, JsonConvert.SerializeObject(config));
            }
        }
        [Authorize]
        [HttpPost]
        public void AddEmail(string name, string email)
        {
            //Загрузим конфигурацию из файла .json
            EmailConfig config = JsonConvert.DeserializeObject<EmailConfig>(System.IO.File.ReadAllText(ConfigFilePath));
            var dist = config.DistributionList.FirstOrDefault(d => d.Name == name);
            if (dist != null)
            {
                if(dist.EMailList.FirstOrDefault(m => m == email) == null)
                {
                    dist.EMailList.Add(email);
                    //Запишем в файл
                    System.IO.File.WriteAllText(ConfigFilePath, JsonConvert.SerializeObject(config));
                }
            }
        }
        [Authorize]
        [HttpPost]
        public void DeleteEmail(string name, string email)
        {
            //Загрузим конфигурацию из файла .json
            EmailConfig config = JsonConvert.DeserializeObject<EmailConfig>(System.IO.File.ReadAllText(ConfigFilePath));
            var dist = config.DistributionList.FirstOrDefault(d => d.Name == name);
            if (dist != null)
            {
                dist.EMailList.Remove(email);
                //Запишем в файл
                System.IO.File.WriteAllText(ConfigFilePath, JsonConvert.SerializeObject(config));
            }
        }
        [Authorize]
        [HttpGet]
        public IActionResult GetEmailDistributionsTable()
        {
            //Загрузим конфигурацию из файла .json
            string config = System.IO.File.ReadAllText(ConfigFilePath);
            EmailConfig deserializedConfig = JsonConvert.DeserializeObject<EmailConfig>(config);
            return View("_EmailDistributionsTable", deserializedConfig.DistributionList);
        }
        //Получить список типов рассылки (тип определяет содержание письма)
        [Authorize]
        [HttpGet]
        public string GetContentTypes()
        {
            return typeof(EMailContentType).EnumToJson();
        }
        [Authorize]
        [HttpPost]
        public void SetContentType(string name, EMailContentType type)
        {
            //Загрузим конфигурацию из файла .json
            EmailConfig config = JsonConvert.DeserializeObject<EmailConfig>(System.IO.File.ReadAllText(ConfigFilePath));
            var dist = config.DistributionList.FirstOrDefault(d => d.Name == name);
            if (dist != null)
            {
                dist.ContentType = type;
                //Запишем в файл
                System.IO.File.WriteAllText(ConfigFilePath, JsonConvert.SerializeObject(config));
            }
        }
        //Получить список режимов рассылки (вручную или по расписанию)
        [Authorize]
        [HttpGet]
        public string GetSendTimeMode()
        {
            return typeof(EmailSendTimeMode).EnumToJson();
        }
        //Установить режим рассылки (вручную или по расписанию)
        [Authorize]
        [HttpPost]
        public void SetSendTimeMode(string name, EmailSendTimeMode mode)
        {
            //Загрузим конфигурацию из файла .json
            EmailConfig config = JsonConvert.DeserializeObject<EmailConfig>(System.IO.File.ReadAllText(ConfigFilePath));
            var dist = config.DistributionList.FirstOrDefault(d => d.Name == name);
            if (dist != null)
            {
                dist.SendTimeMode = mode;
                //Запишем в файл
                System.IO.File.WriteAllText(ConfigFilePath, JsonConvert.SerializeObject(config));
            }
        }
    }
}