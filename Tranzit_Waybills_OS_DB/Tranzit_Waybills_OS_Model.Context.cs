﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Tranzit_Waybills_OS_DB
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    public partial class Tranzit_Waybills_OSEntities : DbContext
    {
        public Tranzit_Waybills_OSEntities()
            : base("name=Tranzit_Waybills_OSEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<WaybillDet> WaybillDet { get; set; }
        public virtual DbSet<Company> Company { get; set; }
        public virtual DbSet<PriceList> PriceList { get; set; }
        public virtual DbSet<Shop> Shop { get; set; }
        public virtual DbSet<Product> Product { get; set; }
    
        public virtual int SetWaybillDet()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("SetWaybillDet");
        }
    }
}
