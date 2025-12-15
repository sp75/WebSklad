using SP.Base;
using SP.Base.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApi.Core;

namespace WebApi.Api.CustomerPayments
{
    public class CustomerPaymentsRepository : BaseRepository
    {
        public List<PayDocView> GetPayDocList(Guid? customer_id, int doc_type)
        {
            var from_dt = DateTime.Now.Date.AddDays(-30);
            var to_dt = DateTime.Now.Date.AddDays(1);

            var ka = db.Kagent.FirstOrDefault(w => w.Id == customer_id);

            return db.v_PayDoc.Where(w => w.DocType == doc_type && (w.KagentId == customer_id || w.CashDeskKaId == ka.KaId) && w.OnDate >= from_dt && w.OnDate < to_dt)
                .OrderByDescending(o => o.OnDate)
                .Select(s => new PayDocView
                {
                    PayDocId = s.PayDocId,
                    DocNum = s.DocNum,
                    Checked = s.Checked,
                    OnDate = s.OnDate,
                    Total = s.Total,
                    Notes = s.Notes,
                    Reason = s.Reason,
                    PersonName = s.PersonName,
                    ChargeName = s.ChargeName,
                    SourceType = s.SourceType,
                    PayTypeName = s.PayTypeName,
                    KaName = s.KaName
                }).ToList();

        }

        public PayDocView GetPayDoc(int PayDocId)
        {
            return db.v_PayDoc.Where(w => w.PayDocId == PayDocId)
                .Select(s => new PayDocView
                {
                    PayDocId = s.PayDocId,
                    DocNum = s.DocNum,
                    Checked = s.Checked,
                    OnDate = s.OnDate,
                    Total = s.Total,
                    Notes = s.Notes,
                    Reason = s.Reason,
                    PersonName = s.PersonName,
                    ChargeName = s.ChargeName,
                    SourceType = s.SourceType,
                    PayTypeName = s.PayTypeName,
                    KaName = s.KaName
                }).FirstOrDefault();
        }


        public object GetChargeType()
        {
            return db.ChargeType.Where(w => w.Mode == 1).Select(s=> new { s.CTypeId, s.Name}).ToList();
        }

        public bool MoveMoney(Guid? customer_id, decimal total)
        {
            var doc_num = db.GetDocNum("pay_doc").FirstOrDefault();
            var on_date = DateTime.Now;
            var oper_id = Guid.NewGuid();
            var ka = db.v_Kagent.FirstOrDefault(w => w.Id == customer_id);
            var CashId = db.CashDesks.FirstOrDefault(w => w.KaId == ka.KaId)?.CashId; 
            if (!CashId.HasValue)
            {
                return false;
            }

            using (var sp_base = SPDatabase.SPBase())
            {
                var _pd_from = sp_base.PayDoc.Add(new PayDoc
                {
                    Id = Guid.NewGuid(),
                 //   Checked = 1,
                    DocNum = doc_num,
                    OnDate = on_date,
                    Total = total,
                    CTypeId = 1,// За товар
                    WithNDS = 1,// З НДС
                    PTypeId = 1,// Наличкой
                    CashId = CashId,// Каса по умолчанию
                    CurrId = 2,
                    OnValue = 1,//Курс валюти
                                //     MPersonId = DBHelper.CurrentUser.KaId,
                    DocType = -3,
                    //   UpdatedBy = DBHelper.CurrentUser.UserId,
                    EntId = ka.EnterpriseId,
                    OperId = oper_id,
                    Reason = $"Інкасація списання коштів",
                    Notes = $"Переміщення через віддалений робочий стіл касира"
                });

                var _pd_to = sp_base.PayDoc.Add(new PayDoc
                {
                    Id = Guid.NewGuid(),
                 //   Checked = 1,
                    DocNum = doc_num,
                    OnDate = on_date,
                    Total = total,
                    CTypeId = 1,// За товар
                    WithNDS = 1,// З НДС
                    PTypeId = 1,// Наличкой
                    CashId = db.CashDesks.Where(w => w.Def == 1).Select(s => s.CashId).FirstOrDefault(),// Каса по умолчанию
                    CurrId = 2, //Валюта по умолчанию
                    OnValue = 1,//Курс валюти
                                //    MPersonId = DBHelper.CurrentUser.KaId,
                    DocType = 3,
                    //      UpdatedBy = DBHelper.CurrentUser.UserId,
                    EntId = ka.EnterpriseId,
                    OperId = oper_id,
                    Reason = $"Інкасація списання коштів",
                    Notes = $"Переміщення через віддалений робочий стіл касира"
                });

                sp_base.SaveChanges();
            }

            return true;
        }

        public bool NewPayDoc(Guid? customer_id, decimal total, int ctypeid, int? ka_id, string notes)
        {
            var on_date = DateTime.Now;
            var ka = db.v_Kagent.FirstOrDefault(w => w.Id == customer_id);
            var CashId = db.CashDesks.FirstOrDefault(w => w.KaId == ka.KaId)?.CashId;
            if (!CashId.HasValue)
            {
                return false;
            }

            using (var sp_base = SPDatabase.SPBase())
            {
                var _pd = sp_base.PayDoc.Add(new PayDoc
                {
                    Id = Guid.NewGuid(),
                    Checked = 1,
                    DocNum = db.GetDocNum("pay_doc").FirstOrDefault(),
                    OnDate = on_date,
                    Total = total,
                    CTypeId = ctypeid,// За товар
                    WithNDS = 1,// З НДС
                    PTypeId = 1,// Наличкой
                    CashId = CashId,// Каса по умолчанию
                    CurrId = 2, //Валюта по умолчанию
                    OnValue = 1,//Курс валюти
                    MPersonId = ka.KaId,
                    DocType = -2,
                    //         UpdatedBy = UserSession.UserId,
                    KaId = ka_id,
                    EntId = ka.EnterpriseId,
                    ReportingDate = on_date,
                    Notes = notes
                });

                sp_base.SaveChanges();
            }

            return true;
        }

    }
}