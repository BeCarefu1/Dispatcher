using GIPManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ActGIPelectroWeb.ViewModels
{
    public class PointaLettersRowViewModel
    {
        public int Id { get; set; }
        public string CustomerName { get; set; }
        public string Address_U { get; set; }
        public string Address_O { get; set; }
        public string PhoneNumber { get; set; }
        public string PhoneICO { get => string.IsNullOrEmpty(PhoneNumber) ? "" : "<i class='fas fa-phone-square' data-toggle='tooltip' data-placement='top' title='" + PhoneNumber + "'></i>"; }
        public bool LinkIsOk { get; set; }
        public string LinkIsOkICO { get => LinkIsOk ? "<i class='fab fa-gg-circle' style='color:orange;' data-toggle='tooltip' data-placement='top' title='Связь проверена'></i>" : ""; }
        public List<Letter> Letters { get; set; }

        public PointaLettersRowViewModel()
        {
            Letters = new List<Letter>();
        }
    }
}
