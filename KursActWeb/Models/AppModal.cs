using ActGIPelectroWeb.ViewModels;
using GIPManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ActGIPelectroWeb.Models
{
    public class AppModal
    {
        public StoreContext db = new StoreContext();


        public List<ApplicationListViewModel> OrderList { get; set; }
        //public List<SwitchList> Switchs { get; set; }

        public string nameMaster { get; set; }
        public string roleMaster { get; set; }
        public string namePodpisant { get; set; }
        public string rolePodpisant { get; set; }
        public string nameDisp { get; set; }
        public string roleDisp { get; set; }
        

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
                    return "старший диспетчер";
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

        public AppModal(int id)
        {
            OrderList = (from a in db.ApplicationTable
                         join b in db.ApplicationAG on a.Id_AG equals b.Id
                         join d in db.ApplicationCategory on a.Id_Category equals d.Id
                         join e in db.ApplicationRepair on a.Id_Repair equals e.Id
                         join f in db.AppStatus on a.Id_Status equals f.Id
                         join g in db.NetRegions on a.Id_NetRegion equals g.Id
                         //join h in db.Substations on g.Id equals h.NetRegionId
                         //join i in db.EquipmentTable on a.Id_Equipment equals i.Id
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
                             RequestedEndDate = a.EndReqTime,
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
                             UserId = a.Id_User
                         }).ToList();


            //Switchs = (from a in db.SwitchFormTable
            //           join b in db.ApplicationTable on a.idApp equals b.Id
            //           //join c in db.ContentOfOperations on a.Id equals c.IdSwitchForm
            //           join d in db.NetRegions on b.Id_NetRegion equals d.Id
            //           where a.idApp == id
            //           select new SwitchList
            //           {
            //               id = a.Id,
            //               idNetRegion = b.Id_NetRegion,
            //               netRegion = d.Name,
            //               objectName = b.ObjectName,
            //               job = b.Description,
            //               numbApp = b.NumbApp,
            //               idApp = b.Id,
            //               Contents = (from f in db.ContentOfOperations
            //                           where f.IdSwitchForm == a.Id
            //                           select new ContentOperationList()
            //                           {
            //                               id = f.Id,
            //                               Content = f.Content,
            //                               idSwitchForm = f.IdSwitchForm
            //                           }).ToList()
            //           }).ToList();

            //nameMaster = ShortName(db.Users.Find(OrderList[0].userId).Name);
            //roleMaster = GetRole(db.Users.Find(OrderList[0].userId).RoleId);
            var signature = db.AppSignature.SingleOrDefault(s => s.Id_App == id);
            //var signatureDisp = db.AppCheckDisp.SingleOrDefault(s => s.Id_App == id);
            var signatureDisp = db.SwitchFormSignature.SingleOrDefault(s => s.idSwitch == db.SwitchFormTable.SingleOrDefault(f => f.idApp == id).Id);
            if (signature != null)
            {
                namePodpisant = Models.ShortNameModel.ShortName(db.Users.Find(signature.Id_User).Name);
                rolePodpisant = GetRole(db.Users.Find(signature.Id_User).RoleId);
                //roleMaster = GetRole(db.Users.Find(signature.Id_User).RoleId);
            }
            if (signatureDisp != null)
            {
                nameDisp = Models.ShortNameModel.ShortName(db.Users.Find(signatureDisp.idUser).Name);
                roleDisp = GetRole(db.Users.Find(signatureDisp.idUser).RoleId);
                //nameDisp = ShortName(db.Users.Find(signatureDisp.Id_User).Name);
                //roleDisp = GetRole(db.Users.Find(signatureDisp.Id_User).RoleId);
            }

        }
    }
}
