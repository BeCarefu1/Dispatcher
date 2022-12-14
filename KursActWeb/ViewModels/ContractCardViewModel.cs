using GIPManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ActGIPelectroWeb.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IO;
using System.Threading;

namespace ActGIPelectroWeb.ViewModels
{
    public class ContractCardViewModel
    {
        public int Id { get; set; }
        //public string Name { get; set; }
        //public string Description { get; set; }
        //
        ////Статистика по ТУ
        //public int CountSubstations { get; set; }
        //public int CountRegPoints { get; set; }
        //public int CountLinkOk { get; set; }
        //public int CountAscueChecked { get; set; }
        //public int CountAscueOk { get; set; }
        //public int CountOther { get; set; }
        ////Проценты
        //public int PercentLinkOk { get; set; }
        //public int PercentAscueChecked { get; set; }
        //public int PercentAscueOk { get; set; }
        //public int PercentOther { get; set; }

        //Регионы
        public class RegionListViewModal
        {
            public int IdRegion { get; set; }
            public string NameRegion { get; set; }
        }

        public class UserListViewModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class RoleListViewModel
        {
            public string Name { get; set; }
            public List<UserListViewModel> UserList { get; set; }
        }


        public string LoginUser { get; set; }
        public string RoleName { get; set; }
        public DateTime EndDate { get; set; }



        // *** Количество не обработанных заявок ***
        public int Amount { get; set; }
        // *** Все заявки в БЗ ***
        public int All { get; set; }
        // *** Заявки в архиве
        public int AllArchive { get; set; }
        // *** Количество документов ***
        //public int CountDocs { get; set; }
        // *** Количество записей за сегодня ***
        //public int CountOperToday { get; set; }


        public string ColorClass
        {
            get
            {
                int colorIdx = Id % 10;
                if (colorIdx == 0) return "bg-primary";
                if (colorIdx == 1) return "bg-info";
                if (colorIdx == 2) return "bg-success";
                if (colorIdx == 3) return "bg-secondary";
                if (colorIdx == 4) return "bg-danger";
                if (colorIdx == 5) return "bg-warning";
                if (colorIdx == 6) return "bg-success";
                if (colorIdx == 7) return "bg-dark";
                if (colorIdx == 8) return "bg-info";
                if (colorIdx == 9) return "bg-primary";
                return "bg-info";
            }
        }

        public List<RegionListViewModal> RegionList { get; set; }
        public List<RoleListViewModel> RoleList { get; set; }
        public ContractCardViewModel(int contractId, string role, int id_worker)
        {
            using (StoreContext db = new StoreContext())
            {
                var contract = db.Contracts.Find(contractId);
                if (contract != null)
                {
                    Id = contract.Id;
                    //Name = contract.Name;
                    //Description = contract.Description;
                    ////Всего объектов и точек учета
                    //CountSubstations = db.Substations.Count(s => s.NetRegion.ContractId == Id);
                    //CountRegPoints = db.RegPoints.Count(rp => rp.Substation.NetRegion.ContractId == Id && rp.Status == RegPointStatus.Default);
                    ////Статистика по ТУ
                    //CountLinkOk = db.RegPoints.Count(rp => rp.Substation.NetRegion.ContractId == Id && rp.RegPointFlags.IsLinkOk && !rp.RegPointFlags.IsAscueChecked && !rp.RegPointFlags.IsAscueOk);
                    //CountAscueChecked = db.RegPoints.Count(rp => rp.Substation.NetRegion.ContractId == Id && rp.RegPointFlags.IsAscueChecked && !rp.RegPointFlags.IsAscueOk);
                    //CountAscueOk = db.RegPoints.Count(rp => rp.Substation.NetRegion.ContractId == Id && rp.RegPointFlags.IsAscueOk);
                    //CountOther = CountRegPoints - (CountLinkOk + CountAscueChecked + CountAscueOk);
                    ////Статистика Проценты
                    //if (CountRegPoints > 0)
                    //{
                    //    PercentLinkOk = CountLinkOk * 100 / CountRegPoints;
                    //    //PercentAscueChecked = CountAscueChecked * 100 / CountRegPoints;
                    //    //PercentAscueOk = CountAscueOk * 100 / CountRegPoints;
                    //    //PercentOther = 100 - (PercentLinkOk + PercentAscueChecked + PercentAscueOk);
                    //}

                    char[] MyChar = { 'О', 'П', ' ', 'с', 'г', '.' };
                    //Регионы
                    RegionList = new List<RegionListViewModal>();
                    RegionList = (from a in db.NetRegions select new RegionListViewModal { IdRegion = a.Id, NameRegion = a.Name.TrimStart(MyChar) }).ToList();

                    switch (role)
                    {
                        
                        case "master":
                            All = db.ApplicationTable.Count(s => s.Id_User == id_worker && (s.Id_Status != 3 && s.Id_Status != 5 && s.Id_Status != 7 && s.Id_Status != 8));
                            Amount = db.ApplicationTable.Count(s => (s.Id_Status == 6 || s.Id_Status == 10) && s.Id_User == id_worker);
                            break;
                        case "dispatcher":
                            //Amount = db.OrderTablePermission.Count(s => s.Notification == 2);
                            All = db.ApplicationTable.Count(s => s.Id_Status != 3 && s.Id_Status != 5 && s.Id_Status != 7 && s.Id_Status != 8);
                            //Amount = All - db.ApplicationTable.Count(s => s.Id_Status == 1); //.Count(s => s.Id_OrderStatus == 1);
                            Amount = db.ApplicationTable.Count(s => s.Id_Status == 1 || s.Id_Status == 4 || s.Id_Status == 9 || s.Id_Status == 10);
                            //All = db.OrderTable.Count(s => s.Id_Worker == id_worker);
                            break;
                        case "chief_dispatcher":
                            //Amount = db.OrderTablePermission.Count(s => s.Notification == 2);
                            All = db.ApplicationTable.Count(s => s.Id_Status != 3 && s.Id_Status != 5 && s.Id_Status != 7 && s.Id_Status != 8);
                            Amount = db.ApplicationTable.Count(s => s.Id_Status == 1 || s.Id_Status == 4 || s.Id_Status == 9 || s.Id_Status == 10); //.Count(s => s.Id_OrderStatus == 1);
                            //All = db.OrderTable.Count(s => s.Id_Worker == id_worker);
                            break;
                        case "сhiefEngineer":
                            //Amount = db.OrderTablePermission.Count(s => s.Notification == 2);
                            All = db.ApplicationTable.Count(s => s.Id_Status != 3 && s.Id_Status != 5 && s.Id_Status != 7 && s.Id_Status != 8);
                            Amount = db.ApplicationTable.Count(s => s.Id_Status == 2); //.Count(s => s.Id_OrderStatus == 1);
                            
                            //All = db.OrderTable.Count(s => s.Id_Worker == id_worker);
                            break;
                        case "bossPTO":
                            //Amount = db.OrderTablePermission.Count(s => s.Notification == 2);
                            Amount = db.ApplicationTable.Count(s => s.Id_Status == 2); //.Count(s => s.Id_OrderStatus == 1);
                            All = db.ApplicationTable.Count(s => s.Id_Status != 3 && s.Id_Status != 5 && s.Id_Status != 7 && s.Id_Status != 8);
                            //All = db.OrderTable.Count(s => s.Id_Worker == id_worker);
                            break;
                      

                        default:
                            Amount = 0;
                            All = db.ApplicationTable.Count(s => s.Id_Status != 3 && s.Id_Status != 5 && s.Id_Status != 7 && s.Id_Status != 8);
                            break;
                    }

                    AllArchive = db.ApplicationTable.Count(s => s.Id_Status == 3 || s.Id_Status == 5 || s.Id_Status == 7 || s.Id_Status == 8);

                    RoleName = role;
                    if (db.ShiftSchedule.Count() != 0)
                    {
                        LoginUser = db.Users.LastOrDefault(s => s.Id == db.ShiftSchedule.LastOrDefault().Id_Dispatcher).Login;
                        EndDate = db.ShiftSchedule.LastOrDefault().EndDate;
                    }
                    else
                    {
                        LoginUser = null;
                        
                    }
                    //CountDocs = db.Documents.Count();
                    RoleList = (from a in db.Roles select new RoleListViewModel { Name = Models.GetRuRoleModel.GetRuRole(a.Id), UserList = (from b in db.Users where b.RoleId == a.Id select new UserListViewModel { Id = b.Id, Name = Models.ShortNameModel.ShortName(b.Name) }).ToList() }).ToList();
                    //CountOperToday = db.OperationalLog.Count(s => s.RecoilTime.ToShortDateString() == DateTime.Now.ToShortDateString());
                }
                else
                    throw new Exception("Нулевая ссылка на объект 'Контракт'");
            }
        }
    }
}
