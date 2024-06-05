using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApi.Core;

namespace WebApi.Api.CustomerMove
{
    public class WaybillMoveRepository : BaseRepository
    {
        public List<PosInView> GetPosIn(Guid customer_id, int mat_id)
        {
            return db.Database.SqlQuery<PosInView>(@"
            select item.* , cast (0 as numeric(15,4)) as Amount
from
(
    select pr.PosId, (pr.remain-pr.rsv) as CurRemain, pr.Rsv, wbd.OnDate, wbd.Price, wbd.OnValue,
           wbl.num as DocNum, wbl.OnDate as DocDate, 
           wbl.WType, wbl.WbillId, wbd.BasePrice, pr.SupplierId KaId, pr.Remain,  pr.MatId, pr.WId, wbd.PosParent
    from posremains pr
         left outer join serials s on s.posid=pr.posid
         join waybilldet wbd on wbd.posid=pr.posid
         join waybilllist wbl on wbl.wbillid=wbd.wbillid
    where pr.ondate=(select max(ondate)
                     from posremains
                     where posid=pr.posid )
          and pr.matid = {0}
          and pr.remain > 0 
) item 
inner join Kagent k on k.WId = item.WId
where k.id = {1} ", mat_id, customer_id).ToList();
        }

    }
}