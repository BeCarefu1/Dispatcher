using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using GIPManager;
using ActGIPelectroWeb.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace ActGIPelectroWeb.Controllers
{
    public class RegionController : Controller
    {
        StoreContext db;
        public RegionController(StoreContext context)
        {
            db = context;
        }

        [HttpPost]
        [Authorize]
        public string SubstationsList(int id, int pageNum, int countRows)
        {
            int skipCount = (pageNum - 1) * countRows;
            SqlParameter paramSkipCount = new SqlParameter("@skipCount", skipCount);
            SqlParameter paramCountRows = new SqlParameter("@countRows", countRows);
            SqlParameter paramId = new SqlParameter("@Id", id);
            var subList = db.SubstationRowQuery.FromSql(@"
                SELECT
                Id,
                Name,
                ('') As StateName,
                (SELECT COUNT(Id) From RegPoints Where SubstationId = Substations.Id and RegPoints.Status = 0) As CountRegPoints,
                (SELECT COUNT(Id) From RegPoints Where SubstationId = Substations.Id and RegPoints.Status = 2) As CountUSPD,

                /* Количество ТУ для импорта данных о потребителе */
                (SELECT COUNT(RegPoints.Id)
                From RegPoints, RegPointFlags
                Where SubstationId = Substations.Id and RegPointFlags.RegPointId = RegPoints.Id and RegPointFlags.ImportConsummerData = 0 and RegPoints.Status = 0)
                As CountForImportConsumer,

                /* Количество принято в АИИСКУЭ*/
                (SELECT COUNT(RegPoints.Id) 
                From RegPoints, RegPointFlags 
                Where SubstationId = Substations.Id and RegPointFlags.RegPointId = RegPoints.Id and RegPointFlags.IsAscueOk = 1 and RegPoints.Status = 0) 
                As CountRegPointsAscueOk,

                /* Количество добавлено в АИИСКУЭ*/
                (SELECT COUNT(RegPoints.Id)
                From RegPoints, RegPointFlags
                Where SubstationId = Substations.Id and RegPointFlags.RegPointId = RegPoints.Id and RegPointFlags.IsAscueChecked = 1 and RegPoints.Status = 0)
                As CountRegPointsAscueChecked,

                /* Количество связь проверена*/
                (SELECT COUNT(RegPoints.Id)
                From RegPoints, RegPointFlags 
                Where SubstationId = Substations.Id and RegPointFlags.RegPointId = RegPoints.Id and RegPointFlags.IsLinkOk = 1 and RegPoints.Status = 0)
                As CountRegPointsLinkOk,

                /* Номер телефона модема */
                (SELECT TOP 1 PhoneNumber From SubstationLinks Where SubstationId = Substations.Id ORDER BY SubstationLinks.Id DESC) As PhoneNumber,

                /* Флаги подстанции */
                IsInstallationDone,
                IsPropSchemeDone,
                IsBalanceDone,
                IsKS2Done,

                /* Дата последнего изменения */
                (SELECT TOP 1 [Date] From SubstationActions Where SubstationId = Substations.Id ORDER BY Id DESC) As LastChanges

                From Substations
                Where NetRegionId = @Id
                /* ORDER BY Substations.Id */
                ORDER BY CASE WHEN Substations.Name LIKE '%[0-9]%' THEN 1 ELSE 0 END ASC, SUBSTRING(Substations.Name, 1, 2) ASC, right('0000'+SUBSTRING(Substations.Name, 4, 4),5) ASC
                OFFSET @skipCount ROWS 
                Fetch Next @countRows Rows Only", paramSkipCount, paramCountRows, paramId);

            return JsonConvert.SerializeObject(subList);
        }
    }
}