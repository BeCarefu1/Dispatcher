using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ActGIPelectroWeb.EmailServices;
using Microsoft.AspNetCore.Hosting;

namespace ActGIPelectroWeb.Pages
{
    public class EmailPageModel : PageModel
    {
        public EmailConfig Configs { get; set; }

        readonly IHostingEnvironment _env;
        readonly string ConfigFilePath;

        public EmailPageModel(IHostingEnvironment env)
        {
            _env = env;
            ConfigFilePath = env.ContentRootPath + "/emailconfig.json";
            JObject mailconfig;
            if (System.IO.File.ReadAllText(ConfigFilePath) == "")
            {
                Configs = new EmailConfig();
                System.IO.File.WriteAllText(ConfigFilePath, JsonConvert.SerializeObject(Configs));
            }
            else
            {
                mailconfig = JObject.Parse(System.IO.File.ReadAllText(ConfigFilePath));
                Configs = mailconfig.ToObject<EmailConfig>();
            }
        }

        public void OnGet()
        {
        }
    }
}