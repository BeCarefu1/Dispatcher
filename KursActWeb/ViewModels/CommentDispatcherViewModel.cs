using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ActGIPelectroWeb.ViewModels
{
    public class CommentDispatcherViewModel
    {
        public int Id { get; set; }
        public string Event { get; set; }
        public int Id_User { get; set; }
        public string User { get; set; }
        public DateTime Date { get; set; }
        public DateTime Time { get; set; }
        public string Comment { get; set; }
        public string StatusComment { get; set; }

        ////ShiftSchedule
        //public DateTime StartDate { get; set; }
        //public DateTime EndDate { get; set; }
        //public int Id_Dispatcher { get; set; }


    }
}
