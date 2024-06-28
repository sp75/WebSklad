using SkladEngine.DBFunction;
using SkladEngine.ExecuteDoc;
using SP.Base;
using SP.Base.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
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
        [HttpGet, Route("import-sales")]
        public void ImportSales()
        {
            var ka_list = db.Database.SqlQuery<OpenStoreAreaList>(@"select * from
(
  SELECT [KaId]
      ,[Name]
      ,[Id]
      ,[OpenStoreAreaId]
      ,WId
	  , (SELECT MAX(wl.ondate) FROM v_WaybillInventory wl WHERE wl.FromWId = [Kagent].WId ) LastInventoryDate
  FROM [dbo].[Kagent]
  where [OpenStoreAreaId] is not null and WId is not null
  )x
where x.LastInventoryDate is not null").ToList();

            foreach (var k_item in ka_list)
            {
                new OpenStoreRepository().ImportKagentSales(k_item.KaId, k_item.OpenStoreAreaId.Value, k_item.LastInventoryDate.Value, k_item.WId.Value);
            }
        }
    

     



        [ApiTokenAuthorize]
        [HttpGet, Route("is-session-end")]
        public bool IsSessionEnd()
        {
            var ka = db.Kagent.FirstOrDefault(w => w.Id == Context.Token);

            using (var tr_os_db = new Tranzit_OSEntities())
            {
                return !tr_os_db.v_SESS.Any(a => a.SAREAID == ka.OpenStoreAreaId && a.SESSEND == null);
            }
        }

      
    }
}