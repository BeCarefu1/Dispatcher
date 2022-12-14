using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ActGIPelectroWeb.Models
{
    public class SortViewModel
    {
        public SortState IdSort { get; private set; } // значение для сортировки по id
        public SortState NameSort { get; private set; } // значение для сортировки по имени
        public SortState DateAppSort { get; private set; }    // значение для сортировки по возрасту
        public SortState Current { get; private set; }     // текущее значение сортировки

        public SortViewModel(SortState sortOrder)
        {
            IdSort = sortOrder == SortState.IdAsc ? SortState.IdDesc : SortState.IdAsc;
            NameSort = sortOrder == SortState.NameAsc ? SortState.NameDesc : SortState.NameAsc;
            DateAppSort = sortOrder == SortState.DateAppAsc ? SortState.DateAppDesc : SortState.DateAppAsc;
            Current = sortOrder;
        }
    }
}
