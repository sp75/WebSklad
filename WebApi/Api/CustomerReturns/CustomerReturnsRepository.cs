using SP.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApi.Core;

namespace WebApi.Api.CustomerReturns
{
    public class CustomerReturnsRepository : BaseRepository
    {
        public object GetReturnetPosIn(Guid customer_id)
        {

        }


        public List<GetPosInView> GetPosIn(int pos_in)
        {
                return db.Database.SqlQuery<GetPosInView>(@"
                select item.PosId,  item.Amount,   item.Price, item.ReturnAmount, (item.Amount - coalesce( item.ReturnAmount,0 )) Remain
                from  
                (	
                   SELECT wmt.PosId,  
                          wmt.Amount,
                          WaybillDet.Price,
                          (SELECT   SUM(wbd_r.Amount) 
                           FROM   dbo.WaybillDet AS wbd_r 
                           INNER JOIN ReturnRel AS rr ON wbd_r.PosId = rr.PosId
                           WHERE (rr.OutPosId = wmt.SourceId) AND (rr.PPosId = wmt.PosId)) AS ReturnAmount

                    from WMATTURN wmt, WAYBILLLIST, WAYBILLDET  
	                where wmt.SOURCEID = {0} and WAYBILLDET.posid = wmt.posid 
	                and WAYBILLDET.wbillid = WAYBILLLIST.wbillid and wmt.TURNTYPE = -1
               )item", pos_in).ToList();
        }

        public List<GetPosOutView> GetPosOut(Guid customer_id)
        {
            return db.Database.SqlQuery<GetPosOutView>(@"
   select wbd.PosId, 
		 wbl.OnDate, 
         wbd.Amount, 
		 wbd.Price, 
		 wbd.BasePrice,
		 wbd.Nds,
		 wbd.CurrId,
		 wbd.OnValue,
		 wbd.Discount,
     	 (wbd.Amount - coalesce( r.ReturnAmount,0) ) Remain,
		 RemoteCustomerReturned.Amount RemoteAmount,
		 wbd.WId,
		 wbd.MatId,
         RemoteCustomerReturned.Id RemoteId
  from WaybillDet wbd
  join WaybillList wbl on wbl.wbillid=wbd.wbillid
  join Materials m on m.matid=wbd.matid
  join Kagent ka on ka.kaid=wbl.kaid
  left join (select rr.OutPosId, 
	               sum(wbd_r.amount) ReturnAmount 
	         from ReturnRel rr, WaybillDet wbd_r 
		     where  wbd_r.PosId = rr.PosId 
			 group by  rr.OutPosId) r on r.OutPosId =wbd.PosId
 join RemoteCustomerReturned on RemoteCustomerReturned.OutPosId = wbd.PosId

  where RemoteCustomerReturned.WbillId is null      
        and wbl.Checked = 1
	    and wbl.WType = -1
        and (wbd.Amount - coalesce( r.ReturnAmount,0) ) >= RemoteCustomerReturned.Amount
		and RemoteCustomerReturned.CustomerId = {0}", customer_id).ToList();

        }
    }
}