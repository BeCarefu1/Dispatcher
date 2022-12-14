using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GIPManager;

namespace ActGIPelectroWeb.ViewModels
{
    public class SubstationRowViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string StateName { get; set; }
        public string PhoneNumber { get; set; }
        public int CountRegPoints { get; set; }
        public int CountUSPD { get; set; }
        public int CountForImportConsumer { get; set; }
        public int CountComments { get; set; }
        public DateTime LastChanges { get; set; } = DateTime.MinValue;

        // Флаги
        public bool IsInstallationDone { get; set; }
        public bool IsPropSchemeDone { get; set; }
        public bool IsBalanceDone { get; set; }
        public bool IsKS2Done { get; set; }

        //TODO: Добавить статистику
        public SubstationRowViewModel()
        {

        }
        public SubstationRowViewModel(Substation substation)
        {
            if (substation != null)
            {
                Id = substation.Id;
                Name = substation.Name;
                StateName = substation.SubstationState.Name;
                CountRegPoints = substation.RegPoints.Count();
                CountForImportConsumer = substation.RegPoints.Count(r => !r.RegPointFlags.ImportConsummerData);
                CountComments = substation.Comments.Count();
                var link = substation.SubstationLinks.LastOrDefault();
                PhoneNumber = (link != null) ? link.PhoneNumber : "-";
                //Дата последних изменений
                var lastAction = substation.SubstationActions.LastOrDefault();
                if (lastAction != null)
                    LastChanges = lastAction.Date;
                //TODO: Добавить статистику

                //Флаги
                IsInstallationDone = substation.IsInstallationDone;
                IsPropSchemeDone = substation.IsPropSchemeDone;
                IsBalanceDone = substation.IsBalanceDone;
                IsKS2Done = substation.IsKS2Done;
            }
            else
                throw new Exception("Нулевая ссылка на объект Substation при создании модели SubstationViewModel");
        }
    }
}
