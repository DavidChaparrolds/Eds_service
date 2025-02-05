namespace LDS_SERVICE_EDS.model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class LDS_PAGAR_EDS
    {
        [Key]
        [Column(Order = 0)]
        public int Id { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(100)]
        public string Gasolina { get; set; }

        [Key]
        [Column(Order = 2, TypeName = "numeric")]
        public decimal PrecioVenta { get; set; }

        [Key]
        [Column(Order = 3, TypeName = "numeric")]
        public decimal Volumen { get; set; }

        [Key]
        [Column(Order = 4)]
        [StringLength(100)]
        public string TipoProgramacion { get; set; }

        [Key]
        [Column(Order = 5, TypeName = "numeric")]
        public decimal Importe { get; set; }

        [Key]
        [Column(Order = 6)]
        public bool Facturado { get; set; }

        [Key]
        [Column(Order = 7)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int IdPosicion { get; set; }

        [Key]
        [Column(Order = 8)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int NumeroManguera { get; set; }
    }
}
