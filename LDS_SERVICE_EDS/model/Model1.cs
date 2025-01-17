using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace LDS_SERVICE_EDS.model
{
    public partial class Model1 : DbContext
    {
        public Model1()
            : base("name=Model13")
        {
        }

        public virtual DbSet<LDS_VENTAS_EDS> LDS_VENTAS_EDS { get; set; }
        public virtual DbSet<LDS_PAGAR_EDS> LDS_PAGAR_EDS { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LDS_VENTAS_EDS>()
                .Property(e => e.Gasolina)
                .IsUnicode(false);

            modelBuilder.Entity<LDS_VENTAS_EDS>()
                .Property(e => e.TipoProgramacion)
                .IsUnicode(false);
        }
    }
}
