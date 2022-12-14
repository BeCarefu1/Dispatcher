using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GIPManager;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ClosedXML.Excel;
using Newtonsoft.Json;
using ActGIPelectroWeb.Models;

namespace ActGIPelectroWeb.Controllers
{
    public class UploadFileController : Controller
    {
        StoreContext db;
        IHostingEnvironment _env;
        public UploadFileController(StoreContext context, IHostingEnvironment env)
        {
            db = context;
            _env = env;
        }
        
        [HttpPost]
        public string UploadEnergyData(IFormFile file)
        {
            var data = new List<EnergyDataRow>();
            if (file != null)
            {
                var workbook = new XLWorkbook(file.OpenReadStream());
                var ws = workbook.Worksheet(1);
                int row = 11;

                while(!string.IsNullOrWhiteSpace(ws.Cell(row, 1).Value.ToString()))
                {
                    string serialNum = ws.Cell(row, 22).Value.ToString();
                    var e_sum_d = double.TryParse(ws.Cell(row, 105).Value.ToString(), out double e_sum);
                    var e_t1_d = double.TryParse(ws.Cell(row, 106).Value.ToString(), out double e_t1);
                    var e_t2_d = double.TryParse(ws.Cell(row, 107).Value.ToString(), out double e_t2);
                    var rp = db.RegPoints.FirstOrDefault(r => r.Device.SerialNumber == serialNum);

                    if(rp != null && (e_sum_d || e_t1_d || e_t2_d))
                    {
                        data.Add(new EnergyDataRow() { Id = rp.Id, SerialNumber = serialNum, E_Sum = e_sum, E_T1 = e_t1, E_T2 = e_t2 });
                    }
                    row++;
                }
            }
            return JsonConvert.SerializeObject(data);
        }

        [HttpPost]
        public string UploadConsumerData(IFormFile file)
        {
            var data = new List<ConsumerDataRow>();
            if (file != null)
            {
                var workbook = new XLWorkbook(file.OpenReadStream());
                var ws = workbook.Worksheet(1);
                int row = 2;

                while (!string.IsNullOrWhiteSpace(ws.Cell(row, 1).Value.ToString()))
                {
                    var ID_i = int.TryParse(ws.Cell(row, 4).Value.ToString(), out int ID);
                    string consumerName = ws.Cell(row, 5).Value.ToString();
                    string contractNumber = ws.Cell(row, 6).Value.ToString();
                    string uninstalled_Serial = ws.Cell(row, 7).Value.ToString();
                    //string e_t2_d = ws.Cell(row, 8).Value.ToString();
                    var rp = db.RegPoints.FirstOrDefault(r => r.Consumer.Id == ID);

                    if (rp != null && consumerName != "")
                    {
                        data.Add(new ConsumerDataRow() { Id = rp.Id, C_Name = consumerName, C_ContractNumber = contractNumber, C_Uninstalled_Serial = uninstalled_Serial });
                    }
                    row++;
                }
            }
            return JsonConvert.SerializeObject(data);
        }

        [HttpPost] //Данные по замене ПУ
        public string UploadReplaceData(IFormFile file)
        {
            var data = new List<ReplaceDataRow>();
            if (file != null)
            {
                var workbook = new XLWorkbook(file.OpenReadStream());
                var ws = workbook.Worksheet(1);
                int row = 2;

                while (!string.IsNullOrWhiteSpace(ws.Cell(row, 1).Value.ToString()))
                {
                    string serialNum = ws.Cell(row, 7).Value.ToString();
                    var rp = db.RegPoints.FirstOrDefault(r => r.Device.SerialNumber == serialNum);

                    if (rp != null)
                    {
                        data.Add(new ReplaceDataRow() { Id = rp.Id, C_Serial = serialNum });
                    }
                    row++;
                }
            }
            return JsonConvert.SerializeObject(data);
        }
    }
}