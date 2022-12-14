using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GIPManager;

namespace ActGIPelectroWeb.ViewModels
{
    public class OrderListViewModel
    {
        public int Id { get; set; }
        public string OrderType { get; set; } // Тип заявки
        public string NetRegionName { get; set; } // Наименования подразделения
        public string WorkerTypeDescription { get; set; } // Описание работника
        public string WorkerName { get; set; } // Имя работника
        public string WorkerSurname { get; set; } // Фамилия работника
        public string WorkerMiddlename { get; set; } // Отчества работника
        public string DateApplication { get; set; } // Запросить дату добавления
        public DateTime StartDate { get; set; } // Начало периода
        public DateTime EndDate { get; set; } // Конец периода
        public string SubstationName { get; set; } // Имя подстанции
        public string DescriptionApplication { get; set; } // Описание работ
        public string WorkerSubName { get; set; } // Имя исполнителя
        public string WorkerSubSurname { get; set; } // Фамилия исполнителя
        public string OrderStatus { get; set; } // Статус запроса

        public string TermApplication { get; set; } // Срок запроса

        public string OrderInitiator { get; set; } // Инициатор заявки
        public int Id_OrderInitiator { get; set; }
        public string WorkTime { get; set; } // Период произ. раб.
        public string ShortDescription { get; set; } // 
        public string Brigade { get; set; } // Бригада

        public string Worker1 { get; set; }
        public string Worker2 { get; set; }
        public string Worker3 { get; set; }

        public int Id_OrderType { get; set; }
        public int Id_OrderStatus { get; set; }
        public int Id_Substation { get; set; }

        public string CommentDispatcher { get; set; }
        public string CommentPOne { get; set; }
        public string CommentPTwo { get; set; }
        public string CommentPThree { get; set; }
        public string Comment { get; set; }

        //--------------
        public int IdOrder { get; set; }
        public int IdDispatcher { get; set; }
        public int IdPOne { get; set; }
        public int IdPTwo { get; set; }
        public int IdPThree { get; set; }
        public DateTime Term { get; set; }

        //SubList

        public string Name { get; set; }
        public int NetRegionId { get; set; }

        //
        public int Id_Worker { get; set; }

        //
        public string time { get; set; }


        //
        public DateTime Date { get; set; }
        public string DispatcherComment { get; set; }
        public int DispatcherId { get; set; }
        public string PodpisantOne { get; set; }
        public string PodpisantOneComment { get; set; }
        public int PodpisantOneId { get; set; }
        public string PodpisantTwo { get; set; }
        public string PodpisantTwoComment { get; set; }
        public int PodpisantTwoId { get; set; }
        public string PodpisantThree { get; set; }
        public string PodpisantThreeComment { get; set; }
        public int PodpisantThreeId { get; set; }
        public string Podpisant { get; set; }

        //Цвет
        public string Color { get; set; }

        //
        public int IdMaster { get; set; }
        public string NameMaster { get; set; }

        

    }
}
