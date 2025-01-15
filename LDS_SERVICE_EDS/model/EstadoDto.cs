using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDS.model
{
    public class EstadoDto
    {
        public int IdPosicion { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string Dinero { get; set; } = "0";
        public string Volumen { get; set; } = "0";
        public string NumeroManguera { get; set; } = "0";


    }
}
