using ActGIPelectroWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace ActGIPelectroWeb.EmailServices
{
    public class EmailConfig
    {
        public List<Distribution> DistributionList { get; set; } = new List<Distribution>();
    }

    public class Distribution
    {
        public string Name { get; set; }    //Имя рассылки
        public DateTime LastSendDate { get; set; }
        public EmailSendTimeMode SendTimeMode { get; set; }
        public string SendTimeModeName { get => SendTimeMode.GetAttributeOfType<EnumMemberAttribute>().Value; }
        public EMailContentType ContentType { get; set; } //Тип рассылки рассылки
        public string ContentTypeName { get => ContentType.GetAttributeOfType<EnumMemberAttribute>().Value; }
        public List<string> EMailList { get; set; } = new List<string>(); //Список адресов кому отправлять письма
    }
    
    public enum EmailSendTimeMode
    {
        [EnumMember(Value = "Ручная отправка")]
        Manual = 0,
        [EnumMember(Value = "По будням - 8:00")]
        WorkDays_8AM = 1
    }

    public enum EMailContentType
    {
        [EnumMember(Value = "Не определено")]
        Undefined = 0,
        [EnumMember(Value = "Все точки для Башкирэнерго")]
        AllPointsToBashkirenergo = 1
    }

    //Определяет содержимое письма в зависимости от типа
    public class EmailContent
    {
        public string Subject { get; private set; }
        public string Message { get; private set; }
        public byte[] FileData { get; private set; }
        public string FileName { get; private set; }

        public EmailContent(EMailContentType contentType)
        {
            switch (contentType)
            {
                case EMailContentType.AllPointsToBashkirenergo:
                    Subject = "от ООО 'КУРС' Выполнено нарастающим итогом по Договору подряда №РЭС-1.16.7/Д-00465 от " + DateTime.Now.ToShortDateString();
                    Message = "Здравствуйте." + Environment.NewLine +
                    "Направляю Вам перечень установленных приборов учёта по Договору подряда №РЭС - 1.16.7/Д - 00465 от 05.02.2019г. " +
                    "на выполнение работ по реконструкции, внедрение АСКУЭ на границах балансовой принадлежности и в центрах питания Северного и Западного РЭС ПО УГЭС ООО «Башкирэнерго»." + Environment.NewLine +
                    "Данный перечень будет направляться автоматически каждое утро нарастающим итогом при выполнении работ." + Environment.NewLine + 
                    Environment.NewLine +
                    "Это письмо сформировано автоматически службой уведомлений ООО КУРС. Отвечать на него не нужно.";
                    ExcelManager em = new ExcelManager();
                    //Получаем Excel файл по контракту УГЭС 2019 (Id: 1)
                    var file = em.PointsInContract(1);
                    FileData = file.Data;
                    FileName = file.Name;
                    break;
                default:
                    FileData = null;
                    FileName = null;
                    break;
            }
        }

    }
}
