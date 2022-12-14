using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ActGIPelectroWeb.Models
{

    public enum SortState
    {
        IdAsc,
        IdDesc,
        NameAsc,
        NameDesc,
        //InitAsc,
        //InitDesc,
        DateAppAsc,
        DateAppDesc
        //DateAsc,
        //DateDesc,
        //SubAsc,
        //SubDesc,
        //StatusAsc,
        //StatusDesc



    }

    /*public class SortState : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }*/
}
