using EDS.tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace LDS_SERVICE_EDS
{
    internal static class Program
    {
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        static void Main(string[] args)
        {
            string dataSource = args[0];
            string initialCatalog = args[1];
            string userId = args[2];
            string password = args[3];

            // Imprimir los valores de las variables
            Console.WriteLine("DataSource: " + dataSource);
            Console.WriteLine("InitialCatalog: " + initialCatalog);
            Console.WriteLine("UserId: " + userId);
            Console.WriteLine("Password: " + password);
            string connectionString;
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(userId))
            {

                Logger.EscribirLog("paso para la cadena de conexion con autenticacion de windows");
                connectionString = $"Data Source={dataSource};Initial Catalog={initialCatalog};Integrated Security=True";
            }
            else
            {
                connectionString = $"Data Source={dataSource};Initial Catalog={initialCatalog};User ID={userId};Password={password};encrypt=False;MultipleActiveResultSets=True";

            }

      
            string url = args[4];
            Console.WriteLine("url : " + url);
            string timeout = args[5];
            Console.WriteLine("timeout : " + timeout);
            int delay = 1000;
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new Lds_service_eds(connectionString,url,int.Parse(timeout),delay)
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
