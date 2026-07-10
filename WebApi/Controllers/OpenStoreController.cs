using SkladEngine.DBFunction;
using SkladEngine.ExecuteDoc;
using SP.Base;
using SP.Base.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web.Http;
using Tranzit_OS;
using WebApi.Api.CustomerInventory;
using WebApi.Api.CustomerMove;
using WebApi.Api.OpenStore;
using WebApi.Controllers.Models;
using WebApi.Core;
using static WebApi.Api.OpenStore.OpenStoreRepository;

namespace WebApi.Controllers
{
    [RoutePrefix("api/open-store")]
    public class OpenStoreController :  BaseApiController
    {
        private readonly Log4netLogger _log = new Log4netLogger("ErrorNotification");

        /*     [HttpGet, Route("import-sales")]
             public void ImportSales()
             {
                 var ka_list = db.Database.SqlQuery<OpenStoreAreaList>(@"SELECT [KaId]
           ,[Name]
           ,[Id]
           ,[OpenStoreAreaId]
           ,WId
           ,LastInventoryDate
       FROM [dbo].v_Kagent
       where [OpenStoreAreaId] is not null and WId is not null and LastInventoryDate is not null and PTypeId is not null").ToList();

                 foreach (var k_item in ka_list)
                 {
                     var repo=  new OpenStoreRepository();

                     repo.ImportKagentReturns(k_item.KaId, k_item.OpenStoreAreaId.Value, k_item.LastInventoryDate.Value.AddDays(-10), k_item.WId.Value);

                     repo.ImportKagentSales(k_item.KaId, k_item.OpenStoreAreaId.Value, k_item.LastInventoryDate.Value.AddDays(-10), k_item.WId.Value);
                 }
             }*/

        private static readonly object _import_salesLock = new object();

        [HttpGet, Route("import-sales")]
        public void ImportSales()
        {
            // Намагаємося зайти в блок. Якщо інший потік вже там — цей запит просто вийде,
            // не навантажуючи сервер і базу дублюючими процесами.
            if (!Monitor.TryEnter(_import_salesLock))
            {
                _log.LogInfo("Імпорт продажів вже виконується іншим потоком. Поточний запит скасовано.");
                return;
            }

            try
            {
                var ka_list = db.Database.SqlQuery<OpenStoreAreaList>(@"SELECT [KaId]
  ,[Name]
  ,[Id]
  ,[OpenStoreAreaId]
  ,WId
  ,LastInventoryDate
FROM [dbo].v_Kagent
where [OpenStoreAreaId] is not null and WId is not null and LastInventoryDate is not null and PTypeId is not null").ToList();

                foreach (var k_item in ka_list)
                {
                    var repo = new OpenStoreRepository();

                    repo.ImportKagentReturns(k_item.KaId, k_item.OpenStoreAreaId.Value, k_item.LastInventoryDate.Value.AddDays(-10), k_item.WId.Value);

                    repo.ImportKagentSales(k_item.KaId, k_item.OpenStoreAreaId.Value, k_item.LastInventoryDate.Value.AddDays(-10), k_item.WId.Value);
                }
            }
            finally
            {
                // Обов'язково звільняємо lock, навіть якщо всередині впала помилка
                Monitor.Exit(_import_salesLock);
            }
        }

        [HttpGet, Route("import-payments")]
        public void ImportPayment()
        {
            var repo = new OpenStoreRepository();

            repo.ImportKagentPayment();
        }


        [ApiTokenAuthorize]
        [HttpGet, Route("is-session-end")]
        public bool IsSessionEnd()
        {
            /* var ka = db.Kagent.FirstOrDefault(w => w.Id == Context.Token);

             using (var tr_os_db = new Tranzit_OSEntities())
             {
                 return !tr_os_db.v_SESS.Where(w=> w.SAREAID == ka.OpenStoreAreaId).OrderByDescending(o=> o.SESSSTART).Take(10).Any(a => a.SESSEND == null);
             }*/

            return new InventoryRepository().IsClosedCashierShift(context_ka.OpenStoreAreaId);
        }

      
    }
}