using GIPManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ActGIPelectroWeb.Models
{
    public class PaymentReportItem
    {
        public int Id { get; set; }
        /// <summary> Точка учета, которая привязывается к отчету </summary>
        public int RegPointId { get; set; }
        /// <summary> Отчет к которому привязывается устройство </summary>
        public int PaymentReportId { get; set; }
        /// <summary> Тип работы произведенной монтажником </summary>
        public PaymentReportWorkType WorkType { get; set; }
        /// <summary> Серийный номер устройства </summary>
        public string SerialNumber { get; set; }
        /// <summary> Тип устройства </summary>
        public string DeviceType { get; set; }
        /// <summary> Название района </summary>
        public string RegionName { get; set; }
        /// <summary> Имя подстанции </summary>
        public string SubstationName { get; set; }
        /// <summary> Адрес объекта </summary>
        public string OAddress { get; set; }
        /// <summary> Статус проверки связи при ПНР </summary>
        public bool IsLinkOk { get; set; }
        /// <summary> Цена за СМР ПУ </summary>
        public double CostRUB { get; set; }
    }
}
