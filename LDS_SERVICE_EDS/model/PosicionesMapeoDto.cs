using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDS.model
{
    internal class PosicionesMapeoDto
    {
        public int Dispensador { get; set; }
        public int Posicion { get; set; }
        public int Manguera { get; set; }
        public string NombreProducto { get; set; }
        public int IdProducto { get; set; }
    }
}

