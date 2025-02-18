﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LDS_EDS.model
{
    public class ResultadoVentaDto
    {
        public double TotalDineroInicial { get; set; }
        public double TotalVolumenInicial { get; set; }
        public int IdPosicion { get; set; }
        public int NumeroManguera { get; set; }
        public double PrecioVenta { get; set; }

        [StringLength(100)]
        public string Gasolina { get; set; }

        [StringLength(100)]
        public string TipoProgramacion { get; set; }


    }
}
