﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Tranzit_OS
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class Tranzit_OSEntities : DbContext
    {
        public Tranzit_OSEntities()
            : base("name=Tranzit_OSEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<SESS_EXPOTR> SESS_EXPOTR { get; set; }
        public virtual DbSet<v_SESS> v_SESS { get; set; }
    }
}
