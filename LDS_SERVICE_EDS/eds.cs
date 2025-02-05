using EDS.model;
using EDS.model.EDS.model;
using EDS.tools;
using LDS_EDS.model;
using LDS_SERVICE_EDS.model;
using LDS_SERVICE_EDS.tools;
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

        public Lds_service_eds(string cadena, string url, int timeout, int delay,bool iniciarServicio)
        {
            this.cadena = cadena;
            this.url = url;
            this.timeout = timeout; 
            this.delay = delay;
            if (iniciarServicio)
            {
                InitializeComponent();
            }
            
            /*
            LogEventos = new System.Diagnostics.EventLog();


            if (!System.Diagnostics.EventLog.SourceExists("LdsServiceEds"))
            {
                System.Diagnostics.EventLog.CreateEventSource("LdsServiceEds", "Application");
            }

            LogEventos.Source = "LdsServiceEds";
            LogEventos.Log = "Application";
            */

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
                    Logger.EscribirLog("Ingresa a consultar ventas: "+fullUrl + "***************");

                    // Realizar la petición GET
                    using (HttpResponseMessage response = await httpClient.GetAsync(fullUrl))
                    {

                        // Verificar la respuesta
                        if (response.IsSuccessStatusCode)
                        {
                            string responseBody = await response.Content.ReadAsStringAsync();
                            // Log del cuerpo completo de la respuesta
                            Logger.EscribirLog($"Cuerpo de la respuesta para posición {numPosicion}: {responseBody}");
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
            
        }


        /*Obtener totales*/
        public async Task<List<TotalesDto>> ObtenerTotalesAsync(string url, int numPosicion, int timeout)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    // Configurar el timeout
                    httpClient.Timeout = TimeSpan.FromSeconds(timeout);

                    // Construir la URL completa con el parámetro `NumPosicion`
                    string fullUrl = $"{url}Totales?NumPosicion={numPosicion}";

                    // Realizar la petición GET
                    using (HttpResponseMessage response = await httpClient.GetAsync(fullUrl))
                    {
                        // Verificar la respuesta
                        if (response.IsSuccessStatusCode)
                        {
                            string responseBody = await response.Content.ReadAsStringAsync();
                            var totales = JsonConvert.DeserializeObject<List<TotalesDto>>(responseBody);
                             Logger.EscribirLog("Respuesta exitosa:");
                             Logger.EscribirLog($"Código: {response.StatusCode}");
                            return totales;
                        }
                        else
                        {
                             Logger.EscribirLog($"Error en la solicitud de totales. Código: {response.StatusCode}");
                             Logger.EscribirLog($"Detalle: {await response.Content.ReadAsStringAsync()}");
                            return null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                 Logger.EscribirLog($"Ocurrió un error al obtener los totales: {ex.Message}");
                return null;
            }
        }

        /*Filtrar totales por posicion y manguera*/
        public async Task<TotalesDto> ObtenerTotalesPorMangueraAsync(string url, int numPosicion, int timeout, int manguera)
        {
            try
            {

                var totales = await ObtenerTotalesAsync(url, numPosicion, timeout);
                if (totales != null)
                {
                    var total = totales.Where(x => x.IdPosicion == numPosicion && x.NumeroManguera == manguera).First();
                    // 2. Crear el diccionario con la clave "Posiciones" y el valor de la cadena
                    var pares = new Dictionary<object, object>
                    {
                        { "TotalDinero", total.TotalDinero },
                        { "TotalVolumen", total.TotalVolumen }
                    };
                    Files.generarIni("Totales", "TotalesDisponibles", pares);
                    return total;
                }
                else
                {
                     Logger.EscribirLog($"No se tuvo informacion de totales");

                    return null;
                }


            }
            catch (Exception ex)
            {
                 Logger.EscribirLog($"Ocurrió un error al obtener los totales: {ex.Message}");
                return null;
            }
        }


        /*Obtener precios*/
        public async Task<List<PrecioGasolinaDto>> ObtenerPreciosAsync(string url, int timeout)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    // Configurar el timeout
                    httpClient.Timeout = TimeSpan.FromSeconds(timeout);

                    // Realizar la petición GET
                    using (HttpResponseMessage response = await httpClient.GetAsync(url + "Precios"))
                    {
                        // Verificar la respuesta
                        if (response.IsSuccessStatusCode)
                        {
                            string responseBody = await response.Content.ReadAsStringAsync();
                            var precios = JsonConvert.DeserializeObject<List<PrecioGasolinaDto>>(responseBody);
                             Logger.EscribirLog("Respuesta exitosa:");
                             Logger.EscribirLog($"Código: {response.StatusCode}");
                            return precios;
                        }
                        else
                        {
                             Logger.EscribirLog($"Error en la solicitud de precios. Código: {response.StatusCode}");
                             Logger.EscribirLog($"Detalle: {await response.Content.ReadAsStringAsync()}");
                            return null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                 Logger.EscribirLog($"Ocurrió un error al obtener los precios: {ex.Message}");
                return null;
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
                            Logger.EscribirLog($"Código: {response.StatusCode}");
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

        /*Obtener Mapeo*/
        public async Task<List<PosicionesMapeoDto>> RealizarMapeoAsync(string url, int timeout)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    // Configurar el timeout
                    httpClient.Timeout = TimeSpan.FromSeconds(timeout);
                    // Realizar la petición GET
                    using (HttpResponseMessage response = await httpClient.GetAsync(url + "Mapeo"))
                    {
                        // Verificar la respuesta
                        if (response.IsSuccessStatusCode)
                        {
                            string responseBody = await response.Content.ReadAsStringAsync();
                            var mapeos = JsonConvert.DeserializeObject<List<PosicionesMapeoDto>>(responseBody);
                             Logger.EscribirLog("Respuesta exitosa:");
                             Logger.EscribirLog($"Código: {response.StatusCode}");
                            return mapeos;
                        }
                        else
                        {
                             Logger.EscribirLog($"Error en la solicitud de mapeo. Código: {response.StatusCode}");
                             Logger.EscribirLog($"Detalle: {await response.Content.ReadAsStringAsync()}");
                            return null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                 Logger.EscribirLog($"Ocurrió un error al realizar el mapeo: {ex.Message}");
                return null;
            }
        }

        /*Realizar preset*/

        public async Task<string> RealizarPresetAsync(string url, int timeout, PresetDto presetDto)
        {
            try
            {
                string result = null;
                using (var httpClient = new HttpClient())
                {
                    ;
                    // Configurar el timeout
                    httpClient.Timeout = TimeSpan.FromSeconds(timeout);

                    // Serializar el DTO a JSON
                    var json = JsonConvert.SerializeObject(presetDto);
                    string responseBody;
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    // Realizar la petición POST
                    using (HttpResponseMessage response = await httpClient.PostAsync(url + "Preset", content))
                    {
                        // Verificar la respuesta
                        if (response.IsSuccessStatusCode)
                        {
                            responseBody = await response.Content.ReadAsStringAsync();
                             Logger.EscribirLog("Respuesta exitosa:");
                             Logger.EscribirLog($"Código: {response.StatusCode}");
                             Logger.EscribirLog($"Mensaje: {responseBody}");
                            // Extraer el mensaje desde el JSON de respuesta

                            result = responseBody;

                        }
                        else
                        {
                            responseBody = await response.Content.ReadAsStringAsync();
                             Logger.EscribirLog($"Error en la solicitud. Código: {response.StatusCode}");
                             Logger.EscribirLog($"Detalle: {responseBody}");
                            result = null;
                        }


                        var jsonResponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseBody);
                        if (jsonResponse != null && jsonResponse.ContainsKey("Message"))
                        {
                            string extractedMessage = jsonResponse["Message"];
                             Logger.EscribirLog($"Mensaje Extraído: {extractedMessage}");

                            // Escribir el mensaje extraído en el archivo INI
                            var pares = new Dictionary<object, object>
                                {
                                    { "Respuesta", extractedMessage }
                                };
                            Files.generarIni("Preset", "Preset", pares);

                            result = extractedMessage;
                        }
                    }
                }


                return result;
            }
            catch (Exception ex)
            {
                 Logger.EscribirLog($"Ocurrió un error: {ex.Message}");
                return null;
            }
        }


        /*Realizar peticiones repetitivas*/
        public async Task ConsultarEstadoPeriodicamenteAsync(string url, int timeout, int numPosicion, int delay, ResultadoVentaDto resultadoVentaDto)
        {
            try
            {
                int numeroIntentosNull = 0;
                int numeroIntentosOtroEstado = 0;
                bool pasoSurtiendo = false;
                while (true)
                {

                    await Task.Delay(delay);
                    // Llamar al método ObtenerEstadoAsync si es autorizado siga preguntando el solo sale del estado de lo contrario
                    // si es ventazero sale de una y si es otro estado diferente pruebe diez veces y termine
                    var estado = await ObtenerEstadoAsync(url, timeout, numPosicion);

                    if (estado != null)
                    {
                       Logger.EscribirLog($"Estado recibido: {estado.Estado}, Para posicion {numPosicion}");

                        // Si el estado es 'Surtiendo', esperar hasta que cambie a 'Reporte' o 'Espera'
                        while (estado.Estado.Equals("Surtiendo", StringComparison.OrdinalIgnoreCase))
                        {
                            pasoSurtiendo = true;
                            Logger.EscribirLog($"Posición {numPosicion} en estado 'Surtiendo'. Esperando cambio de estado...");
                            await Task.Delay(delay);
                            estado = await ObtenerEstadoAsync(url, timeout, numPosicion);

                            // Si por algún error el estado se vuelve null, terminar el proceso
                            if (estado == null)
                            {
                                Logger.EscribirLog($"No se pudo obtener el estado de la posición {numPosicion}. Terminando...");
                                break;
                            }
                        }
                        // Si el estado es 'reporte' o 'espera', finalizar el bucle
                        if (pasoSurtiendo && (estado.Estado.Equals("reporte", StringComparison.OrdinalIgnoreCase) || estado.Estado.Equals("espera", StringComparison.OrdinalIgnoreCase)))
                        {
                           // await Task.Delay(2000);
                            // List<VentaDto> ventas = await ObtenerVentasAsync(url, numPosicion, timeout);
                            List<VentaDto> ventas = null;
                            Logger.EscribirLog("************************** Obtener Venta ***********************************");
                            int intentosVentas = 0;
                            // Intentar obtener ventas hasta 5 veces
                            while (intentosVentas < 5)
                            {
                                var inicioIntento = DateTime.UtcNow;

                               await Task.Delay(delay);//cambiando este task.delay
                                ventas = await ObtenerVentasAsync(url, numPosicion, timeout);

                                if (ventas != null && ventas.Count > 0)
                                {
                                    Logger.EscribirLog("Ventas obtenidas correctamente.");
                                    break;
                                }

                                Logger.EscribirLog($"Intento {intentosVentas + 1}/5: No se obtuvieron ventas. Reintentando en {delay/1000} segundos...");
                                intentosVentas++;
                                
                            }
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

                                /*Manguera y posicion*/
                                await AgregarTablaPagos(resultadoVentaDto.Gasolina, ventaResultado.PrecioVenta, ventaResultado.Volumen, resultadoVentaDto.TipoProgramacion, ventaResultado.Dinero, ventaResultado.IdPosicion, ventaResultado.NumeroManguera);

                            }
                            else
                            {
                                Logger.EscribirLog("No se obtuvieron ventas o la lista está vacía, alcanzo el limite de intentos");

                            }

                           Logger.EscribirLog("Estado final alcanzado. Terminando consulta periódica.");
                           break;

                        }else if (estado.Estado.Equals("VentaZero", StringComparison.OrdinalIgnoreCase))
                        {
                            Logger.EscribirLog($"Termina por venta zero");
                            break;
                        }
                        else{

                            /*
                            if(!estado.Estado.Equals("autorizado", StringComparison.OrdinalIgnoreCase)){
                                Logger.EscribirLog($"Entro a validar intentos: El estado es {estado.Estado}, Para posicion {numPosicion}");
                                if (numeroIntentosOtroEstado > 10)
                                {
                                    Logger.EscribirLog("Terminando el programa excedio el numero de intentos");
                                    break;
                                }
                                numeroIntentosOtroEstado++;
                            }*/
                            
                            if (estado.Estado.Equals("espera", StringComparison.OrdinalIgnoreCase) || 
                                estado.Estado.Equals("listo", StringComparison.OrdinalIgnoreCase) || 
                                estado.Estado.Equals("error", StringComparison.OrdinalIgnoreCase))
                     
                            {
                                Logger.EscribirLog($"Entro a validar intentos: El estado es {estado.Estado}, Para posicion {numPosicion}");
                                if (numeroIntentosOtroEstado > 10)
                                {
                                    Logger.EscribirLog("Terminando el programa excedio el numero de intentos");
                                    break;
                                }
                                numeroIntentosOtroEstado++;
                            }

                        }
                       
                    }
                    else
                    {
                       Logger.EscribirLog("No se pudo obtener el estado. Intentando nuevamente...");
                        if (numeroIntentosNull > 5)
                        {
                           Logger.EscribirLog("Terminando el programa excedio el numero de intentos");
                            break;
                        }
                        numeroIntentosNull++;
                    }
                }
            }
            catch (Exception ex)
            {
               Logger.EscribirLog($"Ocurrió un error durante la consulta periódica: {ex.Message}");
            }
            finally
            { 
                if (posicionesEnProceso.TryRemove(numPosicion, out _))
                {
                    Logger.EscribirLog($"Hilo liberado correctamente para posición {numPosicion}.");
                }
                else
                {
                    Logger.EscribirLog($"No se encontró el hilo para posición {numPosicion} en posicionesEnProceso.");
                }
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

        /*PRIMERA PARTE INTEGRADOR*/
        /*Solicitar posiciones disponibles
         debe guardar esa informacion para la interfaz observar si se guarda en un archivo o 
        directamente en base de datos
        */

        public async Task<List<int>> obtenerPosicionesDisponibles(string url, int timeout)
        {
            EstadoDto estado;
            List<PosicionesMapeoDto> posicionesMapeadas = await RealizarMapeoAsync(url, timeout);
            List<int> posicionesDisponibles = new List<int>();
            // Para extraer únicamente los valores de "Posicion" SIN repeticiones:
            var posicionesUnicas = posicionesMapeadas
                .Select(x => x.Posicion)
                .Distinct()
                .OrderBy(pos => pos)
                .ToList();

            // Ahora "posicionesUnicas" es una lista de int que contiene solo los números de posición diferentes.
            foreach (var posicion in posicionesUnicas)
            {
                 Logger.EscribirLog($"Posición única: {posicion}");
                estado = await ObtenerEstadoAsync(url, timeout, posicion);

                if (estado != null)
                {
                     Logger.EscribirLog($"Estado paraposicion, : {posicion}, el estado es: {estado.Estado} ");
                    if (estado.Estado.Equals("espera", StringComparison.OrdinalIgnoreCase))
                    {
                        posicionesDisponibles.Add(posicion);
                    }
                }

            }
            // 1. Unir las posiciones en una sola cadena separada por comas
            string posicionesComoCadena = string.Join(",", posicionesDisponibles);

            // 2. Crear el diccionario con la clave "Posiciones" y el valor de la cadena
            var pares = new Dictionary<object, object>
                {
                    { "Posiciones", posicionesComoCadena }
                };
            Files.generarIni("Posiciones", "PosicionesDisponibles", pares);
            return posicionesDisponibles;
        }


        /*Obtener tipos de combustible de posicion
         
         debe guardar esa informacion para la interfaz*/
        public async Task<List<string>> obtenerCombustiblePorPosicion(string url, int timeout, int posicion)
        {
            List<PosicionesMapeoDto> posicionesMapeadas = await RealizarMapeoAsync(url, timeout);
            List<string> posicionesMapeadasString = posicionesMapeadas
            .Where(x => x.Posicion == posicion)
            .Select(x => x.NombreProducto)
            .ToList(); string posicionesComoCadena = string.Join(",", posicionesMapeadasString);
            var pares = new Dictionary<object, object>
                {
                    { "TiposCombustible", posicionesComoCadena }
                };
            Files.generarIni("CombustiblePorPosicion", "PosicionesDisponibles", pares);
            return posicionesMapeadasString;
        }



        /*Obtener datos de base de datos*/

        public async Task<List<LDS_VENTAS_EDS>> obtenerDatosPreset()
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
	                                TotalVolumenInicial,
                                    Gasolina,
                                    TipoProgramacion
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
                                    TotalVolumenInicial = reader.GetDecimal(reader.GetOrdinal("TotalVolumenInicial")),
                                    Gasolina = reader.GetString(reader.GetOrdinal("Gasolina")),
                                    TipoProgramacion = reader.GetString(reader.GetOrdinal("TipoProgramacion")) 

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
        public async Task ActualizarProcesadoAsync(int idPosicion)
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
        public async Task AgregarTablaPagos(string gasolina, double precioVenta, double volumen, string tipoProgramacion, double importe, int idPosicion, int numeroManguera)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(cadena))
                {
                    await conn.OpenAsync();
                    string query = @"INSERT INTO LDS_PAGAR_EDS (Gasolina, PrecioVenta, Volumen, TipoProgramacion, Importe, Facturado, IdPosicion, NumeroManguera)
                             VALUES (@Gasolina, @PrecioVenta, @Volumen, @TipoProgramacion, @Importe, 0, @IdPosicion, @NumeroManguera)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        // Asignar los parámetros de la consulta
                        cmd.Parameters.AddWithValue("@Gasolina", gasolina);
                        cmd.Parameters.AddWithValue("@PrecioVenta", precioVenta);
                        cmd.Parameters.AddWithValue("@Volumen", volumen);
                        cmd.Parameters.AddWithValue("@TipoProgramacion", tipoProgramacion);
                        cmd.Parameters.AddWithValue("@Importe", importe);
                        cmd.Parameters.AddWithValue("@IdPosicion", idPosicion);
                        cmd.Parameters.AddWithValue("@NumeroManguera", numeroManguera);

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



        /*Obtener precios por tipo de gasolina*/
        public async Task obtenerPreciosPorTipoGasolina(string url, int timeout, string gasolina)
        {
            var lista = await ObtenerPreciosAsync(url, timeout);
            if (lista != null)
            {
                var listaResult = lista.Where(x => x.Nombre.ToLower().Trim() == gasolina.ToLower().Trim()).Select(x => new { x.Nombre, x.Precio }).First();
                var pares = new Dictionary<object, object>
                {
                    { "PrecioGasolina", listaResult.Precio }
                };
                Files.generarIni("Gasolina", "Gasolina", pares);

            }
            else
            {
                Logger.EscribirLog("No se obtuvo resultado de la gasolina repisada");
            }

     

        }



        /*Obtener Manguera*/
        public async Task<int> obtenerManguera(string url, int timeout, int posicion, string gasolina)
        {

            try
            {
                List<PosicionesMapeoDto> posicionesMapeadas = await RealizarMapeoAsync(url, timeout);

                int posicionesUnicas = posicionesMapeadas
                               .Where(x => x.Posicion == posicion && x.NombreProducto.ToLower().Trim() == gasolina.ToLower().Trim())
                               .Select(x => x.Manguera)
                               .First();
                var pares = new Dictionary<object, object>
                {
                    { "Manguera", posicionesUnicas }
                };
                Files.generarIni("Manguera", "MangueraPorTipoGasolina", pares);

                return posicionesUnicas;
            }
            catch (Exception ex)
            {
                Logger.EscribirLog("No se puede obtener la manguera por tipo gasolina: "+ ex.Message);
                return 0;
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
           /* LogEventos.WriteEntry($"ProcesoConteo_Elapsed se ejecutó a las {DateTime.Now}",
                       EventLogEntryType.Information);*/
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
                            TotalVolumenInicial = (double)venta.TotalVolumenInicial,
                            Gasolina = venta.Gasolina,
                            TipoProgramacion = venta.TipoProgramacion
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
