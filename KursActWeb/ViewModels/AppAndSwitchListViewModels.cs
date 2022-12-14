using GIPManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ActGIPelectroWeb.ViewModels
{
    public class AppAndSwitchListViewModels
    {
        public readonly StoreContext db = new StoreContext();

        public List<ApplicationListViewModel> OrderList { get; set; }
        public int N { get; set; }
        public AppAndSwitchListViewModels(int id)
        {
            N = 0;
            string name = "";
            string role = "";
            if (db.AppSignature.FirstOrDefault(s => s.Id_App == id) != null)
            {
                var signature = db.Users.Find(db.AppSignature.FirstOrDefault(s => s.Id_App == id).Id_User);
                name = Models.ShortNameModel.ShortName(signature.Name);
                role = GetRole(signature.RoleId);
            }

            //------------------
            /*if (db.SwitchFormTable.FirstOrDefault(s => s.idApp == id) == null)
            {
                SwitchFormTable switchForm = new SwitchFormTable();
                switchForm.idApp = id;
                switchForm.CreateSwitchForm();
            }*/
            string nameDisp = "";
            string roleDisp = "";

            //if (db.SwitchFormSignature.SingleOrDefault(s => s.idSwitch == db.SwitchFormTable.SingleOrDefault(f => f.idApp == id).Id) != null)
            //{
            //    var signatureDisp = db.SwitchFormSignature.SingleOrDefault(s => s.idSwitch == db.SwitchFormTable.SingleOrDefault(f => f.idApp == id).Id);
            //    nameDisp = Models.ShortNameModel.ShortName(db.Users.Find(signatureDisp.idUser).Name);
            //    roleDisp = GetRole(db.Users.Find(signatureDisp.idUser).RoleId);
            //}
            //--------------------


            OrderList = (from a in db.ApplicationTable
                         join b in db.ApplicationAG on a.Id_AG equals b.Id
                         join d in db.ApplicationCategory on a.Id_Category equals d.Id
                         join e in db.ApplicationRepair on a.Id_Repair equals e.Id
                         join f in db.AppStatus on a.Id_Status equals f.Id
                         join g in db.NetRegions on a.Id_NetRegion equals g.Id
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
                             RolePodpisant = role,
                             Switches = (from aa in db.SwitchFormTable
                                         join bb in db.ApplicationTable on aa.idApp equals bb.Id
                                         //join c in db.ContentOfOperations on a.Id equals c.IdSwitchForm
                                         join dd in db.NetRegions on bb.Id_NetRegion equals dd.Id
                                         where aa.idApp == id
                                         select new SwitchListViewModel
                                         {
                                             Id = aa.Id,
                                             IdNetRegion = bb.Id_NetRegion,
                                             NetRegion = dd.Name,
                                             ObjectName = bb.ObjectName,
                                             Job = bb.Description,
                                             NumbApp = bb.NumbApp,
                                             IdApp = bb.Id,
                                             NameDisp = nameDisp,
                                             RoleDisp = roleDisp,
                                             IdStatusApp = bb.Id_Status,
                                             SourceSchema = aa.SourceSchema,
                                             StartDate = aa.StartDate,
                                             EndDate = aa.EndDate,
                                             Contents = (from ff in db.ContentOfOperations
                                                         where ff.IdSwitchForm == aa.Id
                                                         select new SwitchListViewModel.ContentOperationList
                                                         {
                                                             Id = ff.Id,
                                                             Content = ff.Content,
                                                             IdSwitchForm = ff.IdSwitchForm
                                                         }).ToList()
                                         }).ToList()
                        }).ToList();


            
        }

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
    }
}
