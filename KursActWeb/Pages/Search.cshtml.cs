using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GIPManager;
using ActGIPelectroWeb.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ActGIPelectroWeb.Models;

namespace ActGIPelectroWeb.Pages
{
    public class SearchModel : PageModel
    {
        private readonly StoreContext _db;
        public SearchModel(StoreContext db)
        {
            _db = db;
            FoundRegPoints = new List<RegPoint>();
            FoundSubstations = new List<Substation>();
        }

        public string Query { get; set; } //Строка запроса
        public List<Substation> FoundSubstations { get; set; } //Найденные подстанции
        public List<RegPoint> FoundRegPoints { get; set; } //Найденные точки учета

        public void OnGet(string query)
        {
            Query = query;

            if (Query.All(Char.IsDigit)) //Если в запросе только цифры то искать только по ТУ и Подстанциям
            {
                SearchRegPointsBySerial(Query);
                SearchSubstations(Query);
                return;
            }

            if(Query.ToLower().Contains("тп") || Query.ToLower().Contains("ng") || Query.ToLower().Contains("рп") || Query.ToLower().Contains("hg")) //Если в запросе присутствует сочетание тп рп ng hg
            {
                string formatedQuery = Query.ToUpper().Replace("N", "Т").Replace("H", "Р").Replace("G", "П").Replace("-", " ");
                SearchSubstations(formatedQuery);
                return;
            }

            SearchRegPointsByAdress(Query);           
        }

        private void SearchRegPointsBySerial (string query) //Поиск в RegPoints по заводскому номеру
        {
            
            var points = from p in _db.RegPoints
                         where p.Device.SerialNumber.Contains(query)
                         select p;
            FoundRegPoints = points.ToList();
  
        }

        private void SearchSubstations (string query) //Поиск подстанций
        {
            var substations = from p in _db.Substations
                         where p.Name.Contains(query)
                         select p;
            FoundSubstations = substations.ToList();
        }

        private void SearchRegPointsByAdress (string query) //Поиск в RegPoints по адресу
        {

            var points = from p in _db.RegPoints
                         select new
                         {
                             RegPoint = p,
                             Adress = p.Consumer.O_Local.ToLower() + " " + p.Consumer.O_Street.ToLower() + " " + p.Consumer.O_House.ToLower() + " " + p.Consumer.O_Build.ToLower() + p.Consumer.O_Flat.ToLower()
                         };

            var searchResult = from p in points
                               where p.Adress.Contains(query.ToLower())
                               select p.RegPoint;

            FoundRegPoints = searchResult.ToList();
        }
    }
}