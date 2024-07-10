using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApi.Api.OpenStore;
using WebApi.Core;

namespace WebApi.Api.CustomerSales
{
    public class CustomerSalesRepository : BaseRepository
    {
        public List<SalesList> GetCurrentSales(Guid ka_id)
        {
            var ka_sales = db.Database.SqlQuery<SalesList>(@"SELECT 
  v_Sales.ARTID 
 ,v_Sales.ARTCODE
 ,v_Sales.ARTNAME
 ,v_Sales.SESSID
 ,v_Sales.SAREAID
 ,v_Sales.SYSTEMID
 ,v_Sales.SessionStartDate
 ,v_Sales.UNITNAME
 ,m.MatId
 ,mg.Name GrpName
 ,SUM(v_Sales.AMOUNT) Amount
 ,SUM(v_Sales.TOTAL) Total
FROM [SERVER_OS].[Tranzit_OS].[dbo].[v_Sales]
inner join Materials m on m.OpenStoreId = v_Sales.ARTID
inner join MatGroup mg on m.GrpId = mg.GrpId
inner join v_Kagent ka on ka.OpenStoreAreaId = v_Sales.SAREAID
WHERE SESSEND IS null and coalesce( m.Archived,0) = 0 and m.TypeId in (1,5) and ka.Id = {0}
GROUP BY [v_Sales].SESSID,v_Sales.SAREAID, ARTID, ARTCODE, ARTNAME,SessionStartDate, v_Sales.[SYSTEMID], m.MatId, mg.Name, v_Sales.UNITNAME", ka_id).ToList();

            return ka_sales;
        }

        public List<SalesList> GetCurrentReturns(Guid ka_id)
        {
            var ka_sales = db.Database.SqlQuery<SalesList>(@"SELECT 
  v_ReturnSales.ARTID 
 ,v_ReturnSales.ARTCODE
 ,v_ReturnSales.ARTNAME
 ,v_ReturnSales.SESSID
 ,v_ReturnSales.SAREAID
 ,v_ReturnSales.SYSTEMID
 ,v_ReturnSales.SessionStartDate
 ,v_ReturnSales.UNITNAME
 ,m.MatId
 ,mg.Name GrpName
 ,SUM(v_ReturnSales.AMOUNT) Amount
 ,SUM(v_ReturnSales.TOTAL) Total
FROM [SERVER_OS].[Tranzit_OS].[dbo].v_ReturnSales
inner join Materials m on m.OpenStoreId = v_ReturnSales.ARTID
inner join MatGroup mg on m.GrpId = mg.GrpId
inner join v_Kagent ka on ka.OpenStoreAreaId = v_ReturnSales.SAREAID
WHERE SESSEND IS null and coalesce( m.Archived,0) = 0 and m.TypeId in (1,5) and ka.Id = {0}
GROUP BY SESSID,SAREAID, ARTID, ARTCODE, ARTNAME,SessionStartDate, [SYSTEMID], m.MatId, mg.Name, UNITNAME", ka_id).ToList();

            return ka_sales;
        }
    }
}