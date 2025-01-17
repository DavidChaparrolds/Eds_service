using EDS.model;
using EDS.model.EDS.model;
using EDS.tools;
using LDS_EDS.model;
using LDS_SERVICE_EDS.model;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LDS_SERVICE_EDS
{
    public partial class Lds_service_eds : ServiceBase
    {


        private string cadena;
        private string url;
        private int timeout;
        private int delay;
        private static ConcurrentDictionary<int, Task> posicionesEnProceso = new ConcurrentDictionary<int, Task>();

        public Lds_service_eds(string cadena, string url, int timeout, int delay)
        {
            this.cadena = cadena;
            this.url = url;
            this.timeout = timeout; 
            this.delay = delay;
            InitializeComponent();
            LogEventos = new System.Diagnostics.EventLog();


            if (!System.Diagnostics.EventLog.SourceExists("LdsServiceEds"))
            {
                System.Diagnostics.EventLog.CreateEventSource("LdsServiceEds", "Application");
            }

            LogEventos.Source = "LdsServiceEds";
            LogEventos.Log = "Application";

        }

        /*Obtener Venta*/
        public async Task<List<VentaDto>> ObtenerVentasAsync(string url, int numPosicion, int timeout)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    // Configurar el timeout
                    httpClient.Timeout = TimeSpan.FromSeconds(timeout);

                    // Construir la URL completa con el parámetro `NumPosicion`
                    string fullUrl = $"{url}Venta?NumPosicion={numPosicion}";

                    // Realizar la petición GET
                    using (HttpResponseMessage response = await httpClient.GetAsync(fullUrl))
                    {
                        // Verificar la respuesta
                        if (response.IsSuccessStatusCode)
                        {
                            string responseBody = await response.Content.ReadAsStringAsync();
                            var ventas = JsonConvert.DeserializeObject<List<VentaDto>>(responseBody);
                            Logger.EscribirLog("Respuesta exitosa:");
                            Logger.EscribirLog($"Código: {response.StatusCode}");
                            return ventas;
                        }
                        else
                        {
                            Logger.EscribirLog($"Error en la solicitud de ventas. Código: {response.StatusCode}");
                            Logger.EscribirLog($"Detalle: {await response.Content.ReadAsStringAsync()}");
                            return null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.EscribirLog($"Ocurrió un error al obtener las ventas: {ex.Message}");
                return null;
            }
            finally
            {
                posicionesEnProceso.TryRemove(numPosicion, out _);
                Logger.EscribirLog($"Hilo liberado para posición {numPosicion}.");
            }
        }



        public async Task<EstadoDto> ObtenerEstadoAsync(string url, int timeoutMs, int numPosicion)
        {
            using (var httpClient = new HttpClient())
            {
                // Construir la URL
                string fullUrl = $"{url}Estado?NumPosicion={numPosicion}";

                // Creamos un CancellationTokenSource con el tiempo en milisegundos
                using (var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(timeoutMs)))
                {
                    try
                    {
                        // Iniciamos la petición usando el token
                        var requestTask = httpClient.GetAsync(fullUrl, cts.Token);

                        // Esperamos la finalización de la tarea o la cancelación por timeout
                        var response = await requestTask.ConfigureAwait(false);

                        // Verificamos si fue exitosa la respuesta
                        if (response.IsSuccessStatusCode)
                        {
                            string responseBody = await response.Content.ReadAsStringAsync();
                            return JsonConvert.DeserializeObject<EstadoDto>(responseBody);
                        }
                        else
                        {
                            Logger.EscribirLog($"Error en la solicitud de estado. Código: {response.StatusCode}");
                            Logger.EscribirLog($"Detalle: {await response.Content.ReadAsStringAsync()}");
                            return null;
                        }
                    }
                    catch (TaskCanceledException tcex)
                    {
                        // Puede significar que se cumplió el timeout o se canceló manualmente
                        Logger.EscribirLog($"Solicitud cancelada o timeout: {tcex.Message}");
                        return null;
                    }
                    catch (Exception ex)
                    {
                        Logger.EscribirLog($"Ocurrió un error al obtener el estado: {ex.Message}");
                        return null;
                    }
                }
            }
        }



        /*Realizar peticiones repetitivas*/
        public async Task ConsultarEstadoPeriodicamenteAsync(string url, int timeout, int numPosicion, int delay, ResultadoVentaDto resultadoVentaDto)
        {
            try
            {
                int numeroIntentos = 0;
                while (true)
                {

                    await Task.Delay(delay);
                    // Llamar al método ObtenerEstadoAsync
                    var estado = await ObtenerEstadoAsync(url, timeout, numPosicion);

                    if (estado != null)
                    {
                       Logger.EscribirLog($"Estado recibido: {estado.Estado}, Para posicion {numPosicion}");

                        // Si el estado es 'reporte' o 'espera', finalizar el bucle
                        if (estado.Estado.Equals("reporte", StringComparison.OrdinalIgnoreCase) ||
                            estado.Estado.Equals("espera", StringComparison.OrdinalIgnoreCase))
                        {

                            List<VentaDto> ventas = await ObtenerVentasAsync(url, numPosicion, timeout);
                           Logger.EscribirLog("************************** Obtener Venta ***********************************");

                            if (ventas != null && ventas.Count > 0)
                            {
                                foreach (var venta in ventas)
                                {
                                   Logger.EscribirLog($"Idventa: {venta.IdVenta}, FechaVenta {venta.FechaVenta}, Dinero {venta.Dinero}, Volumen {venta.Volumen}, TotalDineroInicial {venta.TotalDineroInicial}, TotalDineroFinal {venta.TotalDineroFinal}, TotalVolumenInicial  {venta.TotalVolumenInicial}, TotalVolumenFinal {venta.TotalVolumenFinal}, IdPosicion {venta.IdPosicion},  NumeroManguera {venta.NumeroManguera}, PrecioVenta {venta.PrecioVenta}, IdProgramacion {venta.IdProgramacion}, IdProducto {venta.IdProducto}");
                                }

                                VentaDto ventaResultado = ventas.Where(x =>
                                x.IdPosicion == resultadoVentaDto.IdPosicion &&
                                x.PrecioVenta == resultadoVentaDto.PrecioVenta &&
                                x.NumeroManguera == resultadoVentaDto.NumeroManguera &&
                                x.TotalDineroInicial == resultadoVentaDto.TotalDineroInicial &&
                                x.TotalVolumenInicial == resultadoVentaDto.TotalVolumenInicial).First();

                               Logger.EscribirLog($"Idventa: {ventaResultado.IdVenta}, FechaVenta {ventaResultado.FechaVenta}, Dinero {ventaResultado.Dinero}, Volumen {ventaResultado.Volumen}, TotalDineroInicial {ventaResultado.TotalDineroInicial}, TotalDineroFinal {ventaResultado.TotalDineroFinal}, TotalVolumenInicial  {ventaResultado.TotalVolumenInicial}, TotalVolumenFinal {ventaResultado.TotalVolumenFinal}, IdPosicion {ventaResultado.IdPosicion},  NumeroManguera {ventaResultado.NumeroManguera}, PrecioVenta {ventaResultado.PrecioVenta}, IdProgramacion {ventaResultado.IdProgramacion}, IdProducto {ventaResultado.IdProducto}");

                                await AgregarTablaPagos(resultadoVentaDto.Gasolina, ventaResultado.PrecioVenta, ventaResultado.Volumen, resultadoVentaDto.TipoProgramacion, ventaResultado.Dinero);

                            }
                            else
                            {
                                Logger.EscribirLog("No se obtuvieron ventas o la lista está vacía.");

                            }

                           Logger.EscribirLog("Estado final alcanzado. Terminando consulta periódica.");
                            break;
                        }
                    }
                    else
                    {
                       Logger.EscribirLog("No se pudo obtener el estado. Intentando nuevamente...");
                        if (numeroIntentos > 5)
                        {
                           Logger.EscribirLog("Terminando el programa excedio el numero de intentos");
                            break;
                        }
                        numeroIntentos++;
                    }
                }
            }
            catch (Exception ex)
            {
               Logger.EscribirLog($"Ocurrió un error durante la consulta periódica: {ex.Message}");
            }
        }



        /*Llamado Concurrente*/

        public async Task IniciarProcesoConcurrente(string url, int timeout, int numPosicion, int delay, ResultadoVentaDto resultadoVentaDto1, int idRegistro)
        {
            if (!posicionesEnProceso.ContainsKey(numPosicion))
            {
                await ActualizarProcesadoAsync(idRegistro);
                Task nuevaTarea = ConsultarEstadoPeriodicamenteAsync(url, timeout, numPosicion, delay, resultadoVentaDto1);

                if (posicionesEnProceso.TryAdd(numPosicion, nuevaTarea))
                {
                   Logger.EscribirLog($"Hilo iniciado para posición {numPosicion}.");
                }
                else
                {
                   Logger.EscribirLog($"No se pudo iniciar el hilo para posición {numPosicion}.");
                }
            }
            else
            {
               Logger.EscribirLog($"La posición {numPosicion} ya está siendo procesada.");
            }
        }

        /*Obtener datos de base de datos*/

        private async Task<List<LDS_VENTAS_EDS>> obtenerDatosPreset()
        {
            List<LDS_VENTAS_EDS> listaDatos = new List<LDS_VENTAS_EDS>();

            try
            {
                using (SqlConnection conn = new SqlConnection(cadena))
                {
                    await conn.OpenAsync();

                    // EJEMPLO de consulta a la tabla LDS_VENTAS_EDS, ajusta según tu necesidad
                    // por ejemplo: "SELECT Id, Posicion, Manguera, ValorProgramado, PrecioProducto FROM LDS_VENTAS_EDS"
                    // o las columnas que realmente necesites leer
                    string query = @"SELECT 
                                    id,
                                    IdPosicion,
	                                NumeroManguera,
	                                PrecioVenta,
	                                TotalDineroInicial,
	                                TotalVolumenInicial
	                                 FROM LDS_VENTAS_EDS
                             WHERE Procesado = 0"; // Ajusta la condición que necesites

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        // Ejecución asíncrona del comando
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                // Crear objeto y mapear columnas
                                LDS_VENTAS_EDS dato = new LDS_VENTAS_EDS
                                {
                                    // Ajusta los GetOrdinal / nombres de columna según tu tabla
                                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                    IdPosicion = reader.GetInt32(reader.GetOrdinal("IdPosicion")),
                                    NumeroManguera = reader.GetInt32(reader.GetOrdinal("NumeroManguera")),
                                    PrecioVenta = reader.GetDecimal(reader.GetOrdinal("PrecioVenta")),
                                    TotalDineroInicial = reader.GetDecimal(reader.GetOrdinal("TotalDineroInicial")),
                                    TotalVolumenInicial = reader.GetDecimal(reader.GetOrdinal("TotalVolumenInicial"))
                                };

                                // Agregar a la lista
                                listaDatos.Add(dato);
                            }
                        }
                    }
                }

               Logger.EscribirLog($"Se obtuvieron {listaDatos.Count} registros de la tabla LDS_VENTAS_EDS.");
            }
            catch (Exception ex)
            {
               Logger.EscribirLog($"Error al consultar la tabla LDS_VENTAS_EDS: {ex.Message}");
                // Manejo adicional de la excepción (log, throw, etc.)
            }

            return listaDatos;

        }

        /*Cambiar valor a procesado  cuando un hilo empieza la operacion*/
        private async Task ActualizarProcesadoAsync(int idPosicion)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(cadena))
                {
                    await conn.OpenAsync();

                    string query = @"UPDATE LDS_VENTAS_EDS
                             SET Procesado = 1
                             WHERE Id = @IdPosicion";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdPosicion", idPosicion);
                        int rowsAffected = await cmd.ExecuteNonQueryAsync();

                        if (rowsAffected > 0)
                        {
                           Logger.EscribirLog($"Registro con IdPosicion {idPosicion} marcado como Procesado.");
                        }
                        else
                        {
                           Logger.EscribirLog($"No se encontró el registro con IdPosicion {idPosicion} para actualizar.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
               Logger.EscribirLog($"Error al actualizar la columna Procesado para IdPosicion {idPosicion}: {ex.Message}");
            }
        }


        /*Agregara tabla de LDS_PAGAR_EDS*/
        private async Task AgregarTablaPagos(string gasolina, double precioVenta, double volumen, string tipoProgramacion, double importe)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(cadena))
                {
                    await conn.OpenAsync();

                    string query = @"INSERT INTO LDS_PAGAR_EDS (Gasolina, PrecioVenta, Volumen, TipoProgramacion, Importe, Facturado)
                             VALUES (@Gasolina, @PrecioVenta, @Volumen, @TipoProgramacion, @Importe, 0)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        // Asignar los parámetros de la consulta
                        cmd.Parameters.AddWithValue("@Gasolina", gasolina);
                        cmd.Parameters.AddWithValue("@PrecioVenta", precioVenta);
                        cmd.Parameters.AddWithValue("@Volumen", volumen);
                        cmd.Parameters.AddWithValue("@TipoProgramacion", tipoProgramacion);
                        cmd.Parameters.AddWithValue("@Importe", importe);

                        // Ejecutar la consulta de forma asíncrona
                        int rowsAffected = await cmd.ExecuteNonQueryAsync();

                        if (rowsAffected > 0)
                        {
                           Logger.EscribirLog("Registro agregado exitosamente a la tabla LDS_PAGAR_EDS.");
                        }
                        else
                        {
                           Logger.EscribirLog("No se pudo agregar el registro a la tabla LDS_PAGAR_EDS.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
               Logger.EscribirLog($"Error al agregar registro a la tabla LDS_PAGAR_EDS: {ex.Message}");
            }
        }





        protected override void OnStart(string[] args)
        {
            Logger.EscribirLog("Inicia proceso ...");

        }




        protected override void OnStop()
        {
            Logger.EscribirLog("Terminar proceso OnStop...");
        }

        private async void ProcesoConteo_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            LogEventos.WriteEntry($"ProcesoConteo_Elapsed se ejecutó a las {DateTime.Now}",
                       EventLogEntryType.Information);
            try
            {
                Logger.EscribirLog("Iniciando ProcesoConteo_Elapsed...");

                // 1. Obtener registros pendientes de la tabla LDS_VENTAS_EDS
                List<LDS_VENTAS_EDS> ventasPendientes = await obtenerDatosPreset();

                if (ventasPendientes != null && ventasPendientes.Count > 0)
                {
                    ResultadoVentaDto resultadoVentaDto;
                    Logger.EscribirLog($"Se encontraron {ventasPendientes.Count} registros pendientes en LDS_VENTAS_EDS.");

              

                    foreach (var venta in ventasPendientes)
                    {

                         resultadoVentaDto = new ResultadoVentaDto()
                        {
                            PrecioVenta = (double)venta.PrecioVenta,
                            NumeroManguera = venta.NumeroManguera,
                            IdPosicion = venta.IdPosicion,
                            TotalDineroInicial = (double)venta.TotalDineroInicial,
                            TotalVolumenInicial = (double)venta.TotalVolumenInicial

                        };


                        // 'venta' ya es un ResultadoVentaDto con la info necesaria
                        // _ = IniciarProcesoConcurrente(url, timeout, venta.IdPosicion, delay, resultadoVentaDto1);
                        // El guion bajo (_) indica que no hacemos 'await' de esa tarea
                        // y dejamos que cada proceso se ejecute en segundo plano.
                        await IniciarProcesoConcurrente(url, timeout, venta.IdPosicion, delay, resultadoVentaDto, venta.Id);
                    }


                    // Esperar a que todas las tareas de posiciones en proceso finalicen, usando para gestionar el diccionario
                    await Task.WhenAll(posicionesEnProceso.Values);

                }
                else
                {
                    Logger.EscribirLog("No hay registros pendientes en la tabla LDS_VENTAS_EDS.");
                }
            }
            catch (Exception ex)
            {
                Logger.EscribirLog($"Error en ProcesoConteo_Elapsed: {ex.Message}");
                // Manejo adicional de la excepción (Log, Event Log, etc.)
            }

        }

        private void eventLog1_EntryWritten(object sender, EntryWrittenEventArgs e)
        {

        }
    }
}
