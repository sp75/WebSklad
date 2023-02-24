using Newtonsoft.Json;
using SP.Base;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Http;
using Tranzit_Waybills_OS_DB;
using WebApi.Core;

namespace WebApi.Controllers
{
    [RoutePrefix("api/waybill")]
    public class WayBillImportController : TranzitBaseController
    {
        [HttpGet, Route("import-order")]
        public IHttpActionResult ImportService()
        {
            SyncCompany();

            using (var db = new Tranzit_Waybills_OSEntities())
            {
                var kg_list = db.Shop.Where(w => w.Archived == null || w.Archived == 0).Select(s => s.Id).ToList();
                var prod_list = db.Product.Where(w => w.Archived == null || w.Archived == 0).Select(s => s.Id).ToList();

                using (var sp_base = Database.SPBase())
                {
                    var dt_from = DateTime.Now.Date.AddDays(-10);
                    var dt_to = DateTime.Now.Date.AddDays(1);

                    var wb_det = sp_base.WaybillDet
                        .Where(w => w.WaybillList.Checked == 1 && w.OnDate >= dt_from && w.OnDate < dt_to && kg_list.Contains(w.WaybillList.KaId.Value) && w.WaybillList.WType == -1 && prod_list.Contains(w.MatId))
                        .Select(s => new ProductView
                        {
                            PosId = s.PosId,
                            Amount = s.Amount,
                            DocNum = s.WaybillList.Num,
                            Notes = s.Notes,
                            OnDate = s.WaybillList.OnDate,
                            Price = s.Price.Value,
                            ProductId = s.MatId,
                            ShopId = s.WaybillList.KaId.Value,
                            BasePrice = s.BasePrice.Value,
                            WbillId = s.WaybillList.WbillId,
                            ShipmentDate = s.WaybillList.ShipmentDate,
                            CompanyId = s.WaybillList.EntId
                        }).ToList();

                    string json = JsonConvert.SerializeObject(wb_det, Formatting.Indented);
                    var list = (DataTable)JsonConvert.DeserializeObject(json, (typeof(DataTable)));

                    var param = new SqlParameter("@list", SqlDbType.Structured);
                    param.TypeName = "dbo.WaybillDet_Import";
                    param.Value = list;

                    db.Database.ExecuteSqlCommand("exec SetWaybillDet @list", param);


                    var return_wb_det = sp_base.WaybillDet
                        .Join(sp_base.ReturnRel, wbdet => wbdet.PosId, ret => ret.DPosId, (wbdet, ret) => new
                        {
                            ReturnRel = ret,
                            WaybillDet = wbdet
                        })
                        .Where(w => w.WaybillDet.WaybillList.Checked == 1 && w.WaybillDet.OnDate >= dt_from && kg_list.Contains(w.WaybillDet.WaybillList.KaId.Value) && prod_list.Contains(w.WaybillDet.MatId))
                        .Select(s => new ReturnProductView
                        {
                            PosId = s.ReturnRel.PosId,
                            Amount = s.WaybillDet.Amount,
                            OnDate = s.WaybillDet.WaybillList.OnDate,
                            OutPosId = s.ReturnRel.OutPosId,
                            WbillId = s.WaybillDet.WbillId
                        }).ToList();


                    var param2 = new SqlParameter("@list", SqlDbType.Structured);
                    param2.TypeName = "dbo.ReturnWaybillDet_Import";
                    param2.Value = (DataTable)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(return_wb_det, Formatting.Indented), (typeof(DataTable))); ;

                    db.Database.ExecuteSqlCommand("exec SetReturnWaybillDet @list", param2);

                    return Ok(wb_det);
                }
            }
        }
        private void SyncCompany()
        {
            using (var sp_base = Database.SPBase())
            {
                var company_list = sp_base.Kagent.Where(w => w.KType == 3).ToList();
                using (var db = new Tranzit_Waybills_OSEntities())
                {
                    foreach (var item in company_list)
                    {
                        var c = db.Company.Find(item.KaId);
                        if (c != null)
                        {
                            c.EDRPOU = item.OKPO;
                            c.Name = item.Name;
                            c.FullName = item.FullName;
                        }
                        else
                        {
                            db.Company.Add(new Company
                            {
                                Id = item.KaId,
                                Name = item.Name,
                                FullName = item.FullName,
                                EDRPOU = item.OKPO
                            });
                        }

                    }
                    db.SaveChanges();
                }
            }
        }

        [HttpGet, Route("products")]
        public IHttpActionResult GetProduct()
        {
            using (var db = new Tranzit_Waybills_OSEntities())
            {
                return Ok(db.Product.ToList());
            }
        }


        [HttpGet, Route("shops")]
        public IHttpActionResult GetShops()
        {
            using (var db = new Tranzit_Waybills_OSEntities())
            {
                return Ok(db.Shop.ToList());
            }
        }

        [HttpGet, Route("company")]
        public IHttpActionResult GetCompany()
        {
            using (var db = new Tranzit_Waybills_OSEntities())
            {
                return Ok(db.Company.ToList());
            }
        }


        public class ProductView
        {
            public int PosId { get; set; }
            public DateTime OnDate { get; set; }
            public decimal Amount { get; set; }
            public decimal Price { get; set; }
            public int ProductId { get; set; }
            public int ShopId { get; set; }
            public string DocNum { get; set; }
            public string Notes { get; set; }
            public decimal BasePrice { get; set; }
            public int WbillId { get; set; }
            public DateTime? ShipmentDate { get; set; }
            public int? CompanyId { get; set; }
        }

        public class ReturnProductView
        {
            public int PosId { get; set; }
            public DateTime OnDate { get; set; }
            public decimal Amount { get; set; }
            public int OutPosId { get; set; }
            public int WbillId { get; set; }
        }
    }
}
