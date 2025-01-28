using EDS.tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LDS_SERVICE_EDS.tools
{
    public class Files
    {


        /*Generar a .ini*/
        public static void generarIni(string direccion, string llavePrincipal, Dictionary<object, object> pares)
        {

            string rutaArchivo = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, direccion + ".ini");
            Console.WriteLine("ruta es: " + rutaArchivo);
            try
            {
                Logger.EscribirLog("Se crea el .ini");
                Console.WriteLine("Se crea un .ini");
                Encoding encoding = new UTF8Encoding(false);
                using (StreamWriter writer = new StreamWriter(rutaArchivo, false, encoding))
                {
                    writer.WriteLine("["+llavePrincipal+"]");
                    // Recorrer el diccionario y escribir cada par "clave = valor"
                    foreach (var kvp in pares)
                    {
                        writer.WriteLine($"{kvp.Key}={kvp.Value}");
                    }
                }

            }
            catch (Exception ex)
            {
                Logger.EscribirLog("Error al escribir el archivo INI de error pago: " + ex.Message);
            }

        }

    }
}
