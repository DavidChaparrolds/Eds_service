using EDS.model;
using LDS_EDS.model;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace LDS_SERVICE_EDS
{
    public partial class Lds_service_eds : ServiceBase
    {


        private static ConcurrentDictionary<int, Task> posicionesEnProceso = new ConcurrentDictionary<int, Task>();

        public Lds_service_eds()
        {
            InitializeComponent();
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
                            Console.WriteLine("Respuesta exitosa:");
                            Console.WriteLine($"Código: {response.StatusCode}");
                            return ventas;
                        }
                        else
                        {
                            Console.WriteLine($"Error en la solicitud de ventas. Código: {response.StatusCode}");
                            Console.WriteLine($"Detalle: {await response.Content.ReadAsStringAsync()}");
                            return null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ocurrió un error al obtener las ventas: {ex.Message}");
                return null;
            }
            finally
            {
                posicionesEnProceso.TryRemove(numPosicion, out _);
                Console.WriteLine($"Hilo liberado para posición {numPosicion}.");
            }
        }



        public async Task<EstadoDto> ObtenerEstadoAsync(string url, int timeout, int numPosicion)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    // Configurar el timeout
                    httpClient.Timeout = TimeSpan.FromSeconds(timeout);

                    // Construir la URL con el parámetro numPosicion
                    string fullUrl = $"{url}Estado?NumPosicion={numPosicion}";

                    // Realizar la petición GET
                    using (HttpResponseMessage response = await httpClient.GetAsync(fullUrl))
                    {
                        // Verificar la respuesta
                        if (response.IsSuccessStatusCode)
                        {
                            string responseBody = await response.Content.ReadAsStringAsync();
                            var estado = JsonConvert.DeserializeObject<EstadoDto>(responseBody);
                            Console.WriteLine("Respuesta exitosa:");
                            Console.WriteLine($"Código: {response.StatusCode}");
                            return estado;
                        }
                        else
                        {
                            Console.WriteLine($"Error en la solicitud de estado. Código: {response.StatusCode}");
                            Console.WriteLine($"Detalle: {await response.Content.ReadAsStringAsync()}");
                            return null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ocurrió un error al obtener el estado: {ex.Message}");
                return null;
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
                        Console.WriteLine($"Estado recibido: {estado.Estado}, Para posicion {numPosicion}");

                        // Si el estado es 'reporte' o 'espera', finalizar el bucle
                        if (estado.Estado.Equals("reporte", StringComparison.OrdinalIgnoreCase) ||
                            estado.Estado.Equals("espera", StringComparison.OrdinalIgnoreCase))
                        {

                            List<VentaDto> ventas = await ObtenerVentasAsync(url, numPosicion, timeout);
                            Console.WriteLine("************************** Obtener Venta ***********************************");

                            if (ventas != null && ventas.Count > 0)
                            {
                                foreach (var venta in ventas)
                                {
                                    Console.WriteLine($"Idventa: {venta.IdVenta}, FechaVenta {venta.FechaVenta}, Dinero {venta.Dinero}, Volumen {venta.Volumen}, TotalDineroInicial {venta.TotalDineroInicial}, TotalDineroFinal {venta.TotalDineroFinal}, TotalVolumenInicial  {venta.TotalVolumenInicial}, TotalVolumenFinal {venta.TotalVolumenFinal}, IdPosicion {venta.IdPosicion},  NumeroManguera {venta.NumeroManguera}, PrecioVenta {venta.PrecioVenta}, IdProgramacion {venta.IdProgramacion}, IdProducto {venta.IdProducto}");
                                }

                                VentaDto ventaResultado = ventas.Where(x =>
                                x.IdPosicion == resultadoVentaDto.IdPosicion &&
                                x.PrecioVenta == resultadoVentaDto.PrecioVenta &&
                                x.NumeroManguera == resultadoVentaDto.NumeroManguera &&
                                x.TotalDineroInicial == resultadoVentaDto.TotalDineroInicial &&
                                x.TotalVolumenInicial == resultadoVentaDto.TotalVolumenInicial).First();

                                Console.WriteLine($"Idventa: {ventaResultado.IdVenta}, FechaVenta {ventaResultado.FechaVenta}, Dinero {ventaResultado.Dinero}, Volumen {ventaResultado.Volumen}, TotalDineroInicial {ventaResultado.TotalDineroInicial}, TotalDineroFinal {ventaResultado.TotalDineroFinal}, TotalVolumenInicial  {ventaResultado.TotalVolumenInicial}, TotalVolumenFinal {ventaResultado.TotalVolumenFinal}, IdPosicion {ventaResultado.IdPosicion},  NumeroManguera {ventaResultado.NumeroManguera}, PrecioVenta {ventaResultado.PrecioVenta}, IdProgramacion {ventaResultado.IdProgramacion}, IdProducto {ventaResultado.IdProducto}");


                            }
                            else
                            {
                                Console.WriteLine("No se obtuvieron ventas o la lista está vacía.");

                            }

                            Console.WriteLine("Estado final alcanzado. Terminando consulta periódica.");
                            break;
                        }
                    }
                    else
                    {
                        Console.WriteLine("No se pudo obtener el estado. Intentando nuevamente...");
                        if (numeroIntentos > 5)
                        {
                            Console.WriteLine("Terminando el programa excedio el numero de intentos");
                            break;
                        }
                        numeroIntentos++;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ocurrió un error durante la consulta periódica: {ex.Message}");
            }
        }

        /*Llamado Concurrente*/

        public async Task IniciarProcesoConcurrente(string url, int timeout, int numPosicion, int delay, ResultadoVentaDto resultadoVentaDto1)
        {
            if (!posicionesEnProceso.ContainsKey(numPosicion))
            {
                Task nuevaTarea = ConsultarEstadoPeriodicamenteAsync(url, timeout, numPosicion, delay, resultadoVentaDto1);

                if (posicionesEnProceso.TryAdd(numPosicion, nuevaTarea))
                {
                    Console.WriteLine($"Hilo iniciado para posición {numPosicion}.");
                }
                else
                {
                    Console.WriteLine($"No se pudo iniciar el hilo para posición {numPosicion}.");
                }
            }
            else
            {
                Console.WriteLine($"La posición {numPosicion} ya está siendo procesada.");
            }
        }


        protected override void OnStart(string[] args)
        {

        }

        protected override void OnStop()
        {
        }

        private void ProcesoConteo_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {

        }
    }
}
