using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Web;
using WebApi.Core;

namespace WebApi.Api.CustomerInventory
{
    public class InventoryRepository: BaseRepository
    {
        public List<string> ReservedAllosition(int wbill_id, bool execute)
        {
            var list = new List<string>();

            var r = new ObjectParameter("RSV", typeof(Int32));
            var wb_list = db.v_WayBillOutDet.Where(w => w.WbillId == wbill_id && w.Rsv != 1).Select(s => new { s.PosId, s.MatName }).ToList();

            foreach (var i in wb_list)
            {
                db.ReservedPositionV2(i.PosId);

                if (r.Value != null && (int)r.Value == 0)
                {
                    list.Add(i.MatName);
                }
            }

            if (execute && !list.Any())
            {
                db.ExecuteWayBill(wbill_id, null, null).ToList().FirstOrDefault();
            }

            return list;
        }
    }
}