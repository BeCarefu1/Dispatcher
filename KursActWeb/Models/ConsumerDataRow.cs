using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ActGIPelectroWeb.Models
{
    /// <summary>
    /// Модель данных строки для импорта потребителей
    /// </summary>
    public class ConsumerDataRow
    {
        public int Id { get; set; }
        public string C_Name { get; set; }
        public string C_ContractNumber { get; set; }
        public string C_Uninstalled_Serial { get; set; }
    }
}
