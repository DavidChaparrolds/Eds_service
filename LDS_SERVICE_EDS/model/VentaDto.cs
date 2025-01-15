using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDS.model
{
    public class VentaDto
    {
        public int IdVenta { get; set; }
        public DateTime FechaVenta { get; set; }
        public double Dinero { get; set; }
        public double Volumen { get; set; }
        public double TotalDineroInicial { get; set; }
        public double TotalDineroFinal { get; set; }
        public double TotalVolumenInicial { get; set; }
        public double TotalVolumenFinal { get; set; }
        public int IdPosicion { get; set; }
        public int NumeroManguera { get; set; }
        public double PrecioVenta { get; set; }
        public int IdProgramacion { get; set; }
        public int IdProducto { get; set; }
    }
}
