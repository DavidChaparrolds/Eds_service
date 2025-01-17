namespace LDS_SERVICE_EDS.model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class LDS_VENTAS_EDS
    {
        public int Id { get; set; }

        [Column(TypeName = "numeric")]
        public decimal TotalDineroInicial { get; set; }

        [Column(TypeName = "numeric")]
        public decimal TotalVolumenInicial { get; set; }

        public int IdPosicion { get; set; }

        public int NumeroManguera { get; set; }

        [Column(TypeName = "numeric")]
        public decimal PrecioVenta { get; set; }

        [Required]
        [StringLength(100)]
        public string Gasolina { get; set; }

        [Required]
        [StringLength(100)]
        public string TipoProgramacion { get; set; }

        public bool Procesado { get; set; }
    }
}
