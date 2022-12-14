using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ActGIPelectroWeb.Models
{
    /// <summary>
    /// Модель данных строки для импорта показаний в акты
    /// </summary>
    public class EnergyDataRow
    {
        public int Id { get; set; }
        public string SerialNumber { get; set; }
        public double E_Sum { get; set; }
        public double E_T1 { get; set; }
        public double E_T2 { get; set; }
    }
}
