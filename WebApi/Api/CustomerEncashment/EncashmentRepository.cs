using SP.Base;
using SP.Base.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApi.Core;

namespace WebApi.Api.CustomerEncashment
{
    public class EncashmentRepository : BaseRepository
    {
        public List<v_Encashment> GetEncashment(int ka_id)
        {
            return db.v_Encashment.Where(w => w.KaId == ka_id).ToList();
        }

        public v_Encashment GetEncashmentItem(Guid id)
        {
            return db.v_Encashment.FirstOrDefault(w => w.Id == id);
        }

        public bool SetEncashment(int ka_id, Encashment new_item)
        {
            using (var sp_base = SPDatabase.SPBase())
            {
                var r = sp_base.RouteListOrders.FirstOrDefault(w => w.KaId == ka_id && w.RouteList.Checked == 0);
                if (r != null && !sp_base.Encashment.Any(a => a.RouteId == r.RouteListId && a.KaId == ka_id))
                {
                    sp_base.Encashment.Add(new Encashment
                    {
                        AmountMoney = new_item.AmountMoney,
                        Checked = 0,
                        DocType = 40,
                        CreatedAt = DateTime.Now,
                        Id = Guid.NewGuid(),
                        KaId = ka_id,
                        Notes = new_item.Notes,
                        Num = db.GetDocNum("encashment").FirstOrDefault(),
                        RouteId = r.RouteListId
                    });

                    sp_base.SaveChanges();

                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool DelEncashment(Guid id)
        {
            using (var sp_base = SPDatabase.SPBase())
            {
                var i = sp_base.Encashment.FirstOrDefault(w => w.Id == id && w.Checked == 0);
                if(i!= null)
                {
                    sp_base.Encashment.Remove(i);
                    sp_base.SaveChanges();

                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public RouteView GetActiveRoute(int ka_id)
        {
            return db.RouteListOrders.Where(w => w.KaId == ka_id && w.RouteList.Checked == 0).Select(s => new RouteView
            {
                Uid = s.Uid,
                Name = s.RouteList.RouteName,
                StartDate = s.StartDate,
                EndDate = s.EndDate,
                Num = s.Num,
                DriverName = s.RouteList.Kagent.Name,
                DriverPhone = s.RouteList.Kagent.Phone,

            }).FirstOrDefault();
        }
    }
}
