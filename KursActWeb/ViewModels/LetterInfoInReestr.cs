using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ActGIPelectroWeb.ViewModels
{
    public class LetterInfoInReestr
    {
        public int LetterId { get; set; }
        public string OutNumber { get; set; }
        public DateTime InviteDate { get; set; }
        public bool LetterPrinted { get; set; }
        public string TrackNumber { get; set; }
        public string ConsumerName { get; set; }
        public string UAdress { get; set; }

        public string T1 { get; set; }
        public string T2 { get; set; }
        public string Tsum { get; set; }
        public int RegPointId { get; set; }
        public int SubstationId { get; set; }
        public string SubstationName { get; set; }

        public string SerialNumber { get; set; }
        public string PhoneNumber { get; set; }

        public bool LinkIsOk { get; set; }
        public string LinkIsOkICO { get => LinkIsOk ? "<i class='fab fa-gg-circle' style='color:orange;' data-toggle='tooltip' data-placement='top' title='Связь проверена'></i>" : ""; }
    }
}
