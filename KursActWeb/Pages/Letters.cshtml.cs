using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GIPManager;
using ActGIPelectroWeb.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ActGIPelectroWeb.Pages
{
    public class LettersModel : PageModel
    {
        private readonly StoreContext db;
        public LettersModel(StoreContext context)
        {
            db = context;
        }

        public List<(DateTime date, List<LetterInfoInReestr> letters)> Reestrs { get; private set; }

        public void OnGet()
        {
            var ltrs = (from l in db.Letters
                        select new LetterInfoInReestr()
                        {
                            LetterId = l.Id,
                            InviteDate = l.InviteDate,
                            OutNumber = l.OutNumber,
                            LetterPrinted = l.Printed,
                            TrackNumber = l.TrackNumber,

                            ConsumerName = l.RegPoint.Consumer.Name,
                            UAdress = RegPointRowViewModel.FormatUAddress(l.RegPoint.Consumer),

                            LinkIsOk = l.RegPoint.RegPointFlags.IsLinkOk,
                            RegPointId = l.RegPointId,
                            SubstationId = l.RegPoint.SubstationId,
                            SubstationName = l.RegPoint.Substation.Name,

                            SerialNumber = "-",
                            PhoneNumber = "-"
                        }).GroupBy(l => l.InviteDate).ToList();
            Reestrs = ltrs.Select(l => (l.Key, l.ToList())).ToList();
        }
    }
}