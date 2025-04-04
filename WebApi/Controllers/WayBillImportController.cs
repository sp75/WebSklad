﻿using Newtonsoft.Json;
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
    public class WayBillImportController : BaseApiController
    {
       
        [HttpGet, Route("import-order")]
        public IHttpActionResult ImportService()
        {
            SyncCompany();
            SyncShops();
            SyncProducts();

            using (var db = new Tranzit_Waybills_OSEntities())
            {
                var kg_list = db.Shop.Where(w => w.Archived == null || w.Archived == 0).Select(s => s.Id).ToList();
                var prod_list = db.Product.Where(w => w.Archived == null || w.Archived == 0).Select(s => s.Id).ToList();

                using (var sp_base = SPDatabase.SPBase())
                {
                    var dt_from = DateTime.Now.Date.AddDays(-30);
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


                    if (wb_det.Any())
                    {
                        string json = JsonConvert.SerializeObject(wb_det, Formatting.Indented);
                        var list = (DataTable)JsonConvert.DeserializeObject(json, (typeof(DataTable)));

                        var param = new SqlParameter("@list", SqlDbType.Structured);
                        param.TypeName = "dbo.WaybillDet_Import";
                        param.Value = list;

                        db.Database.ExecuteSqlCommand("exec SetWaybillDet @list", param);
                    }


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

                    if (return_wb_det.Any())
                    {
                        var param2 = new SqlParameter("@list", SqlDbType.Structured);
                        param2.TypeName = "dbo.ReturnWaybillDet_Import";
                        param2.Value = (DataTable)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(return_wb_det, Formatting.Indented), (typeof(DataTable))); ;

                        db.Database.ExecuteSqlCommand("exec SetReturnWaybillDet @list", param2);
                    }

                    return Ok(true);
                }
            }
        }

        [HttpGet, Route("import-price")]
        public IHttpActionResult ImportPrices()
        {
            using (var db = new Tranzit_Waybills_OSEntities())
            {
                var shop_list = db.Shop.Select(s => s.Id).ToList();
                var product_list = db.Product.Select(s => s.Id).ToList();

                using (var sp_base = SPDatabase.SPBase())
                {
                    var prices = sp_base.v_KagentMaterilPrices.Where(w=> shop_list.Contains( w.KaId) && product_list.Contains(w.MatId)).Select(s=> new
                    {
                        s.MatId,
                        s.KaId,
                        s.Price,
                        s.Discount
                    }).ToList();

                    var param2 = new SqlParameter("@list", SqlDbType.Structured);
                    param2.TypeName = "dbo.KagentMaterilPrices";
                    param2.Value = (DataTable)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(prices, Formatting.Indented), (typeof(DataTable))); ;

                    db.Database.ExecuteSqlCommand("exec SetPriceList @list", param2);
                }
            }
            return Ok(true);
        }

        private void SyncCompany()
        {
            using (var sp_base = SPDatabase.SPBase())
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

        private void SyncShops()
        {
            using (var db = new Tranzit_Waybills_OSEntities())
            {
                var shop_list = db.Shop.ToList();
                using (var sp_base = SPDatabase.SPBase())
                {
                    foreach (var item in sp_base.Kagent.Where(w => w.KType == 4 && w.KAU != "1").ToList())
                    {
                        var shop = shop_list.FirstOrDefault(w => w.Id == item.KaId);
                        if (shop != null)
                        {
                            shop.Name = item.Name;
                            shop.EDRPOU = item.Kagent2?.OKPO;
                            shop.Archived = (item.Archived == 1 || item.Deleted == 1) ? 1 : 0;
                            shop.UniqueId = item.Id;
                        }
                        else if (item.Deleted == 0 && (item.Archived == 0 || item.Archived == null))
                        {
                            db.Shop.Add(new Shop { Id = item.KaId, Name = item.Name, EDRPOU = item.Kagent2?.OKPO, CreatedAt = DateTime.Now, UniqueId = item.Id });
                        }
                    }
                }

                db.SaveChanges();
            }
        }

        private void SyncProducts()
        {
            using (var sp_base = SPDatabase.SPBase())
            {
                using (var db = new Tranzit_Waybills_OSEntities())
                {
                    var prod_list = db.Product.ToList();

                    foreach (var item in sp_base.Materials.Where(w => (w.TypeId == 1 || w.TypeId == 5) && w.Artikul.Length > 0).ToList())
                    {
                        var product = prod_list.FirstOrDefault(w => w.Id == item.MatId);
                        if (product != null)
                        {
                            product.Name = item.Name;
                            product.Artikul = item.Artikul;
                            product.Archived = (item.Archived == 1 || item.Deleted == 1) ? 1 : 0;
                        }
                        else if (item.Deleted == 0 && (item.Archived == null || item.Archived == 0))
                        {
                            db.Product.Add(new Product
                            {
                                Id = item.MatId,
                                Name = item.Name,
                                Artikul = item.Artikul,
                                CreatedAt = DateTime.Now
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
