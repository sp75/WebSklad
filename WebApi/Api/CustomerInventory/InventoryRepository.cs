using SP.Base;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Web;
using Tranzit_OS;
using WebApi.Core;

namespace WebApi.Api.CustomerInventory
{
    public class InventoryRepository: BaseRepository
    {
        private readonly Log4netLogger _log = new Log4netLogger("ErrorNotification");

        public List<string> ReservedAllosition(int wbill_id, bool execute)
        {
            var list = new List<string>();

            using (var sp_base = SPDatabase.SPBase())
            {
                var wb_list = sp_base.v_WayBillOutDet.Where(w => w.WbillId == wbill_id && w.Rsv != 1).Select(s => new { s.PosId, s.MatName }).ToList();

                foreach (var i in wb_list)
                {
                    int error = 1;
                    int atempt = 1;
                    while (error == 1 && atempt <= 3)
                    {
                        try
                        {
                            var rsv = sp_base.ReservedPositionV2(i.PosId).FirstOrDefault();
                            if (rsv.HasValue && rsv.Value == 0)
                            {
                                list.Add(i.MatName);
                            }
                            error = 0;
                        }
                        catch (Exception ex)
                        {
                            _log.LogException(ex, $"Помилка резервування товару | MatName:{i.MatName} | ");
                            ++atempt;
                        }

                    }

                    if(error == 1)
                    {
                        list.Add(i.MatName);
                    }
                }

                if (execute && !list.Any())
                {
                    sp_base.ExecuteWayBill(wbill_id, null, null).ToList().FirstOrDefault();
                }
            }

            return list;
        }
      
    }
}