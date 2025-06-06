﻿using SkladEngine.ExecuteDoc;
using SP.Base;
using SP.Base.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using WebApi.Api.CustomerInventory;
using WebApi.Core;

namespace WebApi.Api.CustomerReturns
{
    public class CustomerReturnsRepository : BaseRepository
    {
        private readonly Log4netLogger _log = new Log4netLogger("ErrorNotification");

        public List<Guid> GreateReturnToSupplier(Guid customer_id)
        {
            var result = new List<Guid>();

            using (var sp_base = SPDatabase.SPBase())
            {
                var pos_in_list = GetRemoteCustomerReturned(customer_id).ToList();
                var ka = sp_base.v_Kagent.FirstOrDefault(w => w.Id == customer_id);
                var _wb = sp_base.WaybillList.Add(new WaybillList()
                {
                    Id = Guid.NewGuid(),
                    WType = -5,
                    OnDate = DateTime.Now,
                    Num = sp_base.GetDocNum("wb_write_off").FirstOrDefault(),
                    CurrId = 2,
                    OnValue = 1,
                    Notes = "віддалене повернення",
                    EntId = ka.EnterpriseId,
                    ReportingDate = DateTime.Now,
                    Nds = 0,
                    WaybillMove = new WaybillMove { SourceWid = ka.WId.Value },
                    AdditionalDocTypeId = 6 //Повернення
                });
                sp_base.SaveChanges();

                int num = 1;
         //       int error = 0;
                foreach (var pos_in in pos_in_list)
                {
                    var wbd = sp_base.WaybillDet.Add(new WaybillDet()
                    {
                        WbillId = _wb.WbillId,
                        Price = pos_in.Price,
                        BasePrice = pos_in.Price,
                        Nds = _wb.Nds,
                        CurrId = _wb.CurrId,
                        OnDate = _wb.OnDate,
                        WId = ka.WId.Value,
                        Num = ++num,
                        Amount = pos_in.ReturnedAmount,
                        MatId = pos_in.MatId,
                        OnValue = _wb.OnValue
                    });
                    sp_base.SaveChanges();

               /*     try
                    {
                        sp_base.WMatTurn.Add(new WMatTurn
                        {
                            PosId = pos_in.PosId,
                            WId = pos_in.WId,
                            MatId = pos_in.MatId,
                            OnDate = _wb.OnDate,
                            TurnType = 2,
                            Amount = pos_in.Amount,
                            SourceId = wbd.PosId
                        });

                        sp_base.SaveChanges();*/

                        var rcr = sp_base.RemoteCustomerReturned.Find(pos_in.RemoteId);
                        rcr.WbillIdOut = _wb.WbillId;

                        sp_base.SaveChanges();
              /*      }
                    catch
                    {
                        error = 1;

                        var message = string.Format("| Віддалене повернення | Помилка резервування товару mat_id {1} | Торгова точка {0} | Error", customer_id, wbd.MatId);

                        _log.LogInfo(message);
                    }*/

                }

                if (/*error == 0 && */sp_base.WaybillDet.Any(a => a.WbillId == _wb.WbillId))
                {
                    _wb.UpdatedAt = DateTime.Now;
                    result.Add(_wb.Id);

                    sp_base.SaveChanges();


                    new ExecuteWayBill().CorrectDocument(_wb.WbillId, $"Корегування віддаленого повернення товрів по документу №{_wb.Num}", true);

                    var list = new InventoryRepository().ReservedAllosition(_wb.WbillId, true);

                    if (list.Any())
                    {
                        var message = $"Віддалене повернення товарів №{_wb.Num } | Не всі товари зарезервовані в документі | WbillId: {_wb.WbillId} | Номенклатура | {string.Join(",", list)} | Error";
                        _log.LogInfo(message);
                    }
                }
                else
                {
                    sp_base.DeleteWhere<WaybillList>(w => w.WbillId == _wb.WbillId);
                }

            }
            return result;
        }

        /*
        public List<Guid> GreateReturnToSupplierOld(Guid customer_id)
        {
            var result = new List<Guid>();

            using (var sp_base = SPDatabase.SPBase())
            {
                var pos_in_list = GetReturnetPosIn(customer_id).Where(w => (w.TotalRemain - w.Amount) >= 0).ToList();
                var ka = sp_base.v_Kagent.FirstOrDefault(w => w.Id == customer_id);

                foreach (var wb_item in pos_in_list.GroupBy(g => g.KaId).ToList())
                {
                    var _wb = sp_base.WaybillList.Add(new WaybillList()
                    {
                        Id = Guid.NewGuid(),
                        WType = -6,
                        OnDate = DateTime.Now,
                        Num = sp_base.GetDocNum("wb_return_out").FirstOrDefault(),
                        KaId = wb_item.Key,
                        CurrId = 2,
                        OnValue = 1,
                        Notes = "віддалене повернення",
                        EntId = ka.EnterpriseId,
                        ReportingDate = DateTime.Now,
                        PTypeId = 1,
                        Nds = 0,

                    });
                    sp_base.SaveChanges();

                    int num = 1;
                    foreach (var pos_in in wb_item.Select(s => new { s.Id, s.PosId, s.TotalRemain, s.Amount, s.BasePrice, s.MatId, s.WId, s.Price }))
                    {

                        var wbd = sp_base.WaybillDet.Add(new WaybillDet()
                        {
                            WbillId = _wb.WbillId,
                            Price = pos_in.Price,
                            BasePrice = pos_in.BasePrice,
                            Nds = _wb.Nds,
                            CurrId = _wb.CurrId,
                            OnDate = _wb.OnDate,
                            WId = pos_in.WId,
                            Num = ++num,
                            Amount = pos_in.Amount,
                            MatId = pos_in.MatId,
                            OnValue = _wb.OnValue
                        });
                        sp_base.SaveChanges();

                        try
                        {
                            sp_base.WMatTurn.Add(new WMatTurn
                            {
                                PosId = pos_in.PosId,
                                WId = pos_in.WId,
                                MatId = pos_in.MatId,
                                OnDate = _wb.OnDate,
                                TurnType = 2,
                                Amount = pos_in.Amount,
                                SourceId = wbd.PosId
                            });

                            sp_base.SaveChanges();

                            var rcr = sp_base.RemoteCustomerReturned.Find(pos_in.Id);
                            rcr.WbillIdOut = _wb.WbillId;

                            sp_base.SaveChanges();
                        }
                        catch
                        {
                            sp_base.UndoChanges();

                            var message = string.Format("| Віддалене повернення | Помилка резервування товару mat_id {1} | Торгова точка {0} | Error", customer_id, wbd.MatId);

                            _log.LogInfo(message);
                        }

                    }

                    if (sp_base.WaybillDet.Any(a => a.WbillId == _wb.WbillId))
                    {
                        _wb.UpdatedAt = DateTime.Now;
                        result.Add(_wb.Id);
                    }
                    else
                    {
                        sp_base.WaybillList.Remove(sp_base.WaybillList.Find(_wb.WbillId));
                    }

                    sp_base.SaveChanges();

                    sp_base.ExecuteWayBill(_wb.WbillId, null, null).ToList().FirstOrDefault();
                }
            }
            return result;
        }
*/
        public List<ReturnetPosInView> GetRemoteCustomerReturned(Guid customer_id)
        {
            return db.Database.SqlQuery<ReturnetPosInView>(@"
  select RemoteCustomerReturned.Id RemoteId, 
		 wbl.OnDate, 
		 wbd.Price, 
		 RemoteCustomerReturned.Amount RemoteAmount,
		 wbd.MatId,
		 RemoteCustomerReturned.Amount  ReturnedAmount
  from WaybillDet wbd
  join WaybillList wbl on wbl.wbillid=wbd.wbillid
  join Materials m on m.matid=wbd.matid
  left join (select rr.OutPosId, 
	               sum(wbd_r.amount) ReturnAmount 
	         from ReturnRel rr, WaybillDet wbd_r 
		     where  wbd_r.PosId = rr.PosId 
			 group by  rr.OutPosId) r on r.OutPosId =wbd.PosId
 left join Kagent k on k.WId = wbd.WId
 join RemoteCustomerReturned on RemoteCustomerReturned.OutPosId = wbd.PosId

  where RemoteCustomerReturned.WbillId is null 
        and wbl.Checked = 1
	    and wbl.WType  = -1 
        and (wbd.Amount - coalesce( r.ReturnAmount,0) ) >= RemoteCustomerReturned.Amount
		and RemoteCustomerReturned.CustomerId = {0}", customer_id).ToList();
        }



        public List<ReturnetPosInView> GetReturnetPosIn(Guid customer_id)
        {
            return db.Database.SqlQuery<ReturnetPosInView>(@"
            select rcr.*, /*(CurRemain - (select coalesce( sum([Amount]), 0 ) from RemoteCustomerReturned where PosId = item.PosId and id <> rcr.Id ) ) TotalRemain, */
                   item.BasePrice, item.KaId, item.WId, item.MatId, item.Price, item.CurRemain
            from
            (
               select pr.PosId, (pr.remain-pr.rsv) as CurRemain,  wbd.OnDate, wbd.Price, wbl.WType, wbd.BasePrice, pr.SupplierId KaId, pr.MatId, pr.WId
               from posremains pr
               left outer join serials s on s.posid=pr.posid
               join waybilldet wbd on wbd.posid=pr.posid
               join waybilllist wbl on wbl.wbillid=wbd.wbillid
               where pr.ondate=(select max(ondate)
                                from posremains
                                where posid=pr.posid )
               and pr.remain > 0 
            ) item 
            inner join RemoteCustomerReturned rcr on rcr.PosId = item.PosId
            where rcr.CustomerId = {0} and rcr.WbillIdOut is null ", customer_id).ToList();
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
	                and WAYBILLDET.wbillid = WAYBILLLIST.wbillid and wmt.TURNTYPE in ( -1 , 1 )
               )item", pos_in).ToList();
        }

        public List<GetPosOutView> GetReturnetPosOut(Guid customer_id)
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
         RemoteCustomerReturned.Id RemoteId,
         RemoteCustomerReturned.WbillIdOut,
		 RemoteCustomerReturned.Amount ReturnedAmount
  from WaybillDet wbd
  join WaybillList wbl on wbl.wbillid=wbd.wbillid
  join Materials m on m.matid=wbd.matid
  left join (select rr.OutPosId, 
	               sum(wbd_r.amount) ReturnAmount 
	         from ReturnRel rr, WaybillDet wbd_r 
		     where  wbd_r.PosId = rr.PosId 
			 group by  rr.OutPosId) r on r.OutPosId =wbd.PosId
 left join Kagent k on k.WId = wbd.WId
 join RemoteCustomerReturned on RemoteCustomerReturned.OutPosId = wbd.PosId

  where RemoteCustomerReturned.WbillId is null 
        and wbl.Checked = 1
	    and ( wbl.WType  = -1 or k.WId is not null )
        and (wbd.Amount - coalesce( r.ReturnAmount,0) ) >= RemoteCustomerReturned.Amount
		and RemoteCustomerReturned.CustomerId = {0}", customer_id).ToList();

        }
    }
}