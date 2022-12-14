using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ActGIPelectroWeb.Models
{
    /// <summary>
    /// Модель данных строки для импорта данных по замене ПУ
    /// </summary>
    public class ReplaceDataRow
    {
        public int Id { get; set; }
        public string C_Serial { get; set; }
    }
}