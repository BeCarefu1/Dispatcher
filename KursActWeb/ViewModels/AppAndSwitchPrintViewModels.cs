using GIPManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ActGIPelectroWeb.ViewModels
{
    public class AppAndSwitchPrintViewModels
    {
        private readonly StoreContext _db = new StoreContext();
        public List<ApplicationListViewModel> _appList = new List<ApplicationListViewModel>();

        public string GetRole(int? roleId)
        {
            switch (roleId)
            {
                case 1:
                    return "администратор";
                    break;
                case 14:
                    return "диспетчер";
                    break;
                case 15:
                    return "мастер";
                    break;
                case 18:
                    return "главный диспетчер";
                    break;
                case 19:
                    return "главный инженер";
                    break;
                case 20:
                    return "главный ПТО";
                    break;
                default:
                    return "Error";
                    break;
            }
        }

        public AppAndSwitchPrintViewModels(int id)
        {
            string name = "";
            string role = "";
            if (_db.AppSignature.FirstOrDefault(s => s.Id_App == id) != null)
            {
                var signature = _db.Users.Find(_db.AppSignature.FirstOrDefault(s => s.Id_App == id).Id_User);
                name = Models.ShortNameModel.ShortName(signature.Name);
                role = GetRole(signature.RoleId);
            }
            _appList = (from a in _db.ApplicationTable
                        join b in _db.ApplicationAG on a.Id_AG equals b.Id
                        join d in _db.ApplicationCategory on a.Id_Category equals d.Id
                        join e in _db.ApplicationRepair on a.Id_Repair equals e.Id
                        join f in _db.AppStatus on a.Id_Status equals f.Id
                        join g in _db.NetRegions on a.Id_NetRegion equals g.Id
                        where a.Id == id
                        select new ApplicationListViewModel
                        {
                            Id = a.Id,
                            Date = a.Date,
                            NetRegion = g.Name,
                            Category = d.Name,
                            Repair = e.Name,
                            Ag = b.Time,
                            ObjectName = a.ObjectName,
                            Equipment = a.Equipment,
                            Description = a.Description,
                            RequestedDate = a.StartReqTime,
                            RequestedStartTime = a.StartReqTime,
                            RequestedEndTime = a.EndReqTime,
                            ActualDate = a.StartActualTime,
                            ActualStartTime = a.StartActualTime,
                            ActualEndTime = a.EndActualTime,
                            OpenTime = a.OpenTime,
                            CloseTime = a.CloseTime,
                            Notes = a.Notes,
                            Security = a.SecurityDescription,
                            StatusId = a.Id_Status,
                            NambId = a.NumbApp,
                            State = f.Name,
                            StateId = f.Id,
                            UserId = a.Id_User,
                            NamePodpisant = name,
                            RolePodpisant = role
                        }).ToList();
        }
    }
}
