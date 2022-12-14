using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using GIPManager;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ActGIPelectroWeb.ViewModels;
using ActGIPelectroWeb.Models;

namespace ActGIPelectroWeb
{
    public class Program
    {
        public static void Main(string[] args)
        {
            EnumsHelper.SyncAllEnums();
            Task.Factory.StartNew(ImportManager.ImportReports); // Импорт отчетов со статусом "для импорта" выполняется раз в час
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
