using EDS.model.EDS.model;
using EDS.tools;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Policy;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace LDS_SERVICE_EDS
{
    internal static class Program
    {





        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        public static async Task Main(string[] args)
        {

            try
            {
                if (args.Length > 0)
                {
                    string dataSource = args[0];
                    string initialCatalog = args[1];
                    string userId = args[2];
                    string password = args[3];

                    // Imprimir los valores de las variables
                    Logger.EscribirLog("DataSource: " + dataSource);
                    Logger.EscribirLog("InitialCatalog: " + initialCatalog);
                    Logger.EscribirLog("UserId: " + userId);
                    Logger.EscribirLog("Password: " + password);
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
                    Logger.EscribirLog("url : " + url);
                    string timeout = args[5];
                    Logger.EscribirLog("timeout : " + timeout);
                    int delay = int.Parse(args[6]);
                    Logger.EscribirLog("delay : " + delay);
                    string type = args[7];
                    Logger.EscribirLog("type : " + type);
                    string posicion;



                    if (type.ToLower() == "console")
                    {

                        Lds_service_eds program = new Lds_service_eds(connectionString, url, int.Parse(timeout), delay, false);
                        string option = args[8];
                        Logger.EscribirLog("option : " + option);
                        if (option.ToLower() == "posiciones")
                        {
                            Logger.EscribirLog("************************** Obtener posiciones disponibles ***********************************");
                            await program.obtenerPosicionesDisponibles(url, int.Parse(timeout));
                        }
                        else if (option.ToLower() == "combustible")
                        {

                            Logger.EscribirLog("************************** Obtener combustible por posicion ***********************************");
                            posicion = args[9];
                            Logger.EscribirLog($"Posicion: {posicion}");
                            await program.obtenerCombustiblePorPosicion(url, int.Parse(timeout), int.Parse(posicion));
                        }
                        else if (option.ToLower() == "totales")
                        {

                            Logger.EscribirLog("************************** Obtener Totales ***********************************");
                            posicion = args[9];
                            Logger.EscribirLog($"Posicion: {posicion}");
                            string manguera = args[10];
                            Logger.EscribirLog($"Manguera: {manguera}");
                            await program.ObtenerTotalesPorMangueraAsync(url, int.Parse(posicion), int.Parse(timeout), int.Parse(manguera));

                        }
                        else if (option.ToLower() == "precios")
                        {
                            Logger.EscribirLog("************************** Obtener precios por tipo gasolina ***********************************");
                            string gasolina = args[11];
                            Logger.EscribirLog($"Manguera: {gasolina}");
                            await program.obtenerPreciosPorTipoGasolina(url, int.Parse(timeout), gasolina);
                        }
                        else if (option.ToLower() == "manguera")
                        {
                            Console.WriteLine("************************** Obtener manguera por tipo gasolina ***********************************");
                            posicion = args[9];
                            Logger.EscribirLog($"Posicion: {posicion}");
                            string gasolina = args[11];
                            Logger.EscribirLog($"Gasolina: {gasolina}");
                            await program.obtenerManguera(url, int.Parse(timeout), int.Parse(posicion), gasolina);
                        }
                        else if (option.ToLower() == "preset")
                        {
                            /*Realizar Preset*/
                            Logger.EscribirLog("************************** Realizar preset ***********************************");
                            posicion = args[9];
                            Logger.EscribirLog($"Posicion: {posicion}");
                            string manguera = args[10];
                            Logger.EscribirLog($"Manguera: {manguera}");
                            double valorProgramado = double.Parse(args[12]);
                            Logger.EscribirLog($"valor programado: {valorProgramado}");
                            double precioProducto = double.Parse(args[13]);
                            Logger.EscribirLog($"precio producto: {precioProducto}");
                            string TipoProgramacion = args[14];
                            Logger.EscribirLog($"Tipo programacion: {TipoProgramacion}");
                            PresetDto presetDto = new PresetDto
                            {
                                Manguera = int.Parse(manguera),
                                Posicion = int.Parse(posicion),
                                ValorProgramado = valorProgramado,
                                PrecioProducto = precioProducto,
                                TipoProgramacion = TipoProgramacion
                            };
                            string result = await program.RealizarPresetAsync(url, int.Parse(timeout), presetDto);
                            Logger.EscribirLog("El estado es: " + result);
                        }
                    }
                   
                }
                else
                {
                    string dataSource = ConfigurationManager.AppSettings["DataSource"];
                    string initialCatalog = ConfigurationManager.AppSettings["InitialCatalog"];
                    string userId = ConfigurationManager.AppSettings["UserId"];
                    string password = ConfigurationManager.AppSettings["Password"];
                    string url = ConfigurationManager.AppSettings["Url"];
                    string timeout = ConfigurationManager.AppSettings["Timeout"];
                    string delay = ConfigurationManager.AppSettings["Delay"];
                    // Imprimir los valores de las variables
                    Logger.EscribirLog("DataSource: " + dataSource);
                    Logger.EscribirLog("InitialCatalog: " + initialCatalog);
                    Logger.EscribirLog("UserId: " + userId);
                    Logger.EscribirLog("Password: " + password);
                    string connectionString = $"Data Source={dataSource};Initial Catalog={initialCatalog};User ID={userId};Password={password};encrypt=False;MultipleActiveResultSets=True";



                    ServiceBase[] ServicesToRun;
                    ServicesToRun = new ServiceBase[]
                    {
                         new Lds_service_eds(connectionString,url,int.Parse(timeout),int.Parse(delay),true)
                    };
                    ServiceBase.Run(ServicesToRun);
                }

            }
            catch (Exception ex) {
                Logger.EscribirLog($"Ocurrio un error al iniciar la aplicacion: {ex.Message}");
                Logger.EscribirLog($"Detalle del error: {ex.StackTrace}");

            }
            

        }
    }
}
