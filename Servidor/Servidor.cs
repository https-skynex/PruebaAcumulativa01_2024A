/* ************************************************************************
                                Practica 07
Integrantes: Kevin Perez  
Fecha de realización: 11/06/2025  
Fecha de entrega: 18/06/2025  

RESULTADOS:  
Con credenciales válidas (root/admin20), el servidor rechaza al cliente el 
50% de las veces con NOK ACCESO_NEGADO, causando confusión. Tras validar dos 
placas (ABC1234 y XYZ5678) desde una misma IP, el comando CONTADOR devuelve 
OK 2, demostrando que el registro funciona, pero con riesgo de corrupción 
por hilos simultáneos.

CONCLUSIONES:  
1. El servidor maneja correctamente múltiples conexiones simultáneas mediante hilos,
garantizando escalabilidad. No obstante, presenta dos fallas críticas: la aleatoriedad 
en la concesión de acceso (50% de rechazo incluso con credenciales válidas) contradice 
la lógica de autenticación esperada, 
2. El uso de un Dictionary no sincronizado para el conteo de solicitudes genera 
condiciones de carrera en entornos multihilo, lo que puede corromper datos 
o causar excepciones no controladas.

RECOMENDACIONES:  
1. Sustituir el Dictionary por un ConcurrentDictionary para garantizar acceso
seguro en hilos concurrentes sin condiciones de carrera. 
2. Eliminar la lógica aleatoria en el comando INGRESO: el acceso debe concederse 
siempre con credenciales válidas para mantener consistencia.

************************************************************************ */

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using Protocolo;

namespace Servidor
{
    class Servidor
    {
        private static TcpListener _escuchador;
        private static Dictionary<string, int> listadoClientes
            = new Dictionary<string, int>();

        static void Main(string[] args)
        {
            try
            {
                _escuchador = new TcpListener(IPAddress.Any, 8080);
                _escuchador.Start();
                Console.WriteLine("Servidor iniciado en puerto 8080...");

                while (true)
                {
                    TcpClient cliente = _escuchador.AcceptTcpClient();
                    Console.WriteLine($"Cliente conectado: {cliente.Client.RemoteEndPoint}");
                    Thread hiloCliente = new Thread(ManipuladorCliente);
                    hiloCliente.Start(cliente);
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"Error de socket: {ex.Message}");
            }
            finally
            {
                _escuchador?.Stop();
            }
        }

        private static void ManipuladorCliente(object obj)
        {
            TcpClient cliente = (TcpClient)obj;
            NetworkStream flujo = null;

            try
            {
                flujo = cliente.GetStream();
                byte[] bufferRx = new byte[1024];
                int bytesRx;
                while ((bytesRx = flujo.Read(bufferRx, 0, bufferRx.Length)) > 0)
                {
                    // Usar Protocolo para deserializar y resolver
                    Pedido pedido =  Protocolo.Protocolo.DeserializarPedido(bufferRx, bytesRx);
                    Respuesta respuesta = Protocolo.Protocolo.ResolverPedido(
                        pedido,
                        cliente.Client.RemoteEndPoint.ToString()
                    );

                    // Enviar respuesta serializada
                    byte[] bufferTx = Protocolo.Protocolo.SerializarRespuesta(respuesta);
                    flujo.Write(bufferTx, 0, bufferTx.Length);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en cliente: {ex.Message}");
            }
            finally
            {
                flujo?.Close();
                cliente?.Close();
            }
        }

        private static Respuesta ResolverPedido(Pedido pedido, string direccionCliente)
        {
            Respuesta respuesta = new Respuesta
            { Estado = "NOK", Mensaje = "Comando no reconocido" };

            switch (pedido.Comando)
            {
                case "INGRESO":
                    if (pedido.Parametros.Length == 2 &&
                        pedido.Parametros[0] == "root" &&
                        pedido.Parametros[1] == "admin20")
                    {
                        respuesta = new Random().Next(2) == 0
                            ? new Respuesta 
                            { Estado = "OK", 
                                Mensaje = "ACCESO_CONCEDIDO" }
                            : new Respuesta 
                            { Estado = "NOK", 
                                Mensaje = "ACCESO_NEGADO" };
                    }
                    else
                    {
                        respuesta.Mensaje = "ACCESO_NEGADO";
                    }
                    break;

                case "CALCULO":
                    if (pedido.Parametros.Length == 3)
                    {
                        string modelo = pedido.Parametros[0];
                        string marca = pedido.Parametros[1];
                        string placa = pedido.Parametros[2];
                        if (ValidarPlaca(placa))
                        {
                            byte indicadorDia = ObtenerIndicadorDia(placa);
                            respuesta = new Respuesta
                            { Estado = "OK", 
                                Mensaje = $"{placa} {indicadorDia}" };
                            ContadorCliente(direccionCliente);
                        }
                        else
                        {
                            respuesta.Mensaje = "Placa no válida";
                        }
                    }
                    break;

                case "CONTADOR":
                    if (listadoClientes.ContainsKey(direccionCliente))
                    {
                        respuesta = new Respuesta
                        { Estado = "OK",
                            Mensaje = listadoClientes[direccionCliente].ToString() };
                    }
                    else
                    {
                        respuesta.Mensaje = "No hay solicitudes previas";
                    }
                    break;
            }

            return respuesta;
        }

        private static bool ValidarPlaca(string placa)
        {
            return Regex.IsMatch(placa, @"^[A-Z]{3}[0-9]{4}$");
        }

        private static byte ObtenerIndicadorDia(string placa)
        {
            int ultimoDigito = int.Parse(placa.Substring(6, 1));
            switch (ultimoDigito)
            {
                case 1: 
                case 2: 
                    return 0b00100000; // Lunes
                case 3: 
                case 4: 
                    return 0b00010000; // Martes
                case 5: 
                case 6: 
                    return 0b00001000; // Miércoles
                case 7: 
                case 8: 
                    return 0b00000100; // Jueves
                case 9: 
                case 0: 
                    return 0b00000010; // Viernes
                default: 
                    return 0;
            }
        }

        private static void ContadorCliente(string direccionCliente)
        {
            if (listadoClientes.ContainsKey(direccionCliente))
            {
                listadoClientes[direccionCliente]++;
            }
            else
            {
                listadoClientes[direccionCliente] = 1;
            }
        }

    }
}
