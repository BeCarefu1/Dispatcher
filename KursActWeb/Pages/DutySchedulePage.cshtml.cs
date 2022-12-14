using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GIPManager;
using ActGIPelectroWeb.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using System.Text;
using System.Net;
using Spire.Xls;

namespace ActGIPelectroWeb.Pages
{
    public class DutySchedulePageModel : PageModel
    {
        private readonly StoreContext db;
        public DutySchedulePageModel(StoreContext context)
        {
            db = context;
        }

        public string HtmlFile { get; set; }
        public class DispatcherListViewModal
        {
            public int Id_Dispatcher { get; set; }
            public string Name_Dispatcher { get; set; }
        }

        public bool CheckRole()
        {
            string role = db.Users.FirstOrDefault(s => s.Login == User.Identity.Name).Role.Name;
            if (role == "administrator") return true;
            else if (role == "chief_dispatcher") return true;
            else return false;
        }

        //public List<DutyScheduleViewModel> DispatcherList { get; set; }
        //public List<DutyScheduleViewModel> DateTimeList { get; set; }
        //public List<DutyScheduleViewModel> DutyList { get; set; }
        //

        public class FileList
        {
            //public IFormFile File { get; set; }
            public IFormFile File { get; set; }
            public string PathFile { get; set; }
        }

        public void XLSToHTML(byte[] file)
        {
            Workbook workbook = new Workbook();
            //Load an Excel sample document

            workbook.LoadFromFile("C:\\Users\\User\\Desktop\\Уразбахтин Рустам\\Docs\\График дежурства ЦУС на 2022 г.xlsx");

            //Get the first worksheet of the document

            Worksheet sheet = workbook.Worksheets[0];
            //Save the worksheet to HTML
            MemoryStream m = new MemoryStream();
            workbook.SaveToStream(m);
            //sheet.SaveToHtml("ExcelToHTML.html");

            HtmlFile += File(m, "text/html");
            
        }

        //public string PathFile { get; set; }
        //public IFormFile File { get; set; }
        
        //public Workbook Workbooks { get; set; }

        //public List<FileList> Files = new List<FileList>();



        public void OnGet()
        {
            //Workbook workbook = new Workbook();
            //Load an Excel sample document



            //XLSToHTML(db.ExcelFileTable.LastOrDefault().ExFile);

            //byte[] responseArray = WebClient.UploadValues(SubmitURL, myNameValueCollection);
            //htmlFile = XlsxToHTML(db.ExcelFileTable.LastOrDefault().ExFile);
            //Stream stream = new MemoryStream(db.ExcelFileTable.Find(1).ExFile);
            //XLSToHTML();
            //Workbooks = new Workbook(stream);
            //Workbooks.Save("Excel-to-HTML-Tooltip.html");
            //Workbook workbook = new Workbook(GetPathFile(File(db.ExcelFileTable.Find(1).ExFile, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", db.ExcelFileTable.Find(1).Name + ".xlsx")));
            //htmlFile = workbook.ToString();
            //PathFile = Convert.ToBase64String("" db.ExcelFileTable.FirstOrDefault().ExFile);
            //File = new FormFile(new MemoryStream(db.ExcelFileTable.FirstOrDefault().ExFile), 0, db.ExcelFileTable.FirstOrDefault().ExFile.Length, "name", "Name");

            //File.OpenReadStream();

            //"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" + db.ExcelFileTable.Find(1).Name + ".xlsx"
            //htmlFile = Encoding.Default.GetString(db.ExcelFileTable.Find(1).ExFile);
            //FileResult file = File(db.ExcelFileTable.Find(1).ExFile, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", db.ExcelFileTable.Find(1).Name + ".xlsx");
            //tmlFile = Encoding.Default.GetString(file);

            //PathImage = "data:image/jpeg;base64," + Convert.ToBase64String(user.Image);
        }

    }
}
