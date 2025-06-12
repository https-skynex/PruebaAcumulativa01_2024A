/* ************************************************************************
                                Practica 07
Integrantes: Kevin Perez  
Fecha de realización: 11/06/2025  
Fecha de entrega: 18/06/2025  

RESULTADOS:  
El resultado es un sistema funcional para casos básicos, pero frágil ante 
entradas complejas. Por ejemplo, un parámetro como "contraseña con espacios" 
se divide en tres tokens (contraseña, con, espacios), invalidando la operación.

CONCLUSIONES:  
1. El protocolo implementado demuestra ser eficiente en su simplicidad al utilizar 
un formato basado en texto plano (<COMANDO> <parámetros>), facilitando la depuración 
y el análisis de mensajes. Sin embargo, esta simplicidad se convierte en una 
limitación al no manejar adecuadamente parámetros con espacios o caracteres especiales, 
lo que puede generar errores de interpretación.
2. Además, la ausencia de un delimitador claro entre mensajes podría causar fusiones 
de datos en transmisiones continuas, comprometiendo la integridad de la comunicación.

RECOMENDACIONES:  
1. Seria recomendable reemplazar el formato basado en espacios por estructuras robustas como 
JSON, asegurando el manejo de caracteres especiales y parámetros complejos.
2. Es importante incorporar un delimitador de fin de mensaje (ej: \n) para evitar fusiones en 
flujos de datos continuos.

************************************************************************ */

using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System;

namespace Protocolo
{
    public class Pedido
    {
        public string Comando { get; set; }
        public string[] Parametros { get; set; }

        public static Pedido Procesar(string mensaje)
        {
            var partes = mensaje.Split(' ');
            return new Pedido
            {
                Comando = partes[0].ToUpper(),
                Parametros = partes.Skip(1).ToArray()
            };
        }

        public override string ToString()
        {
            return $"{Comando} {string.Join(" ", Parametros)}";
        }
    }

    public class Respuesta
    {
        public string Estado { get; set; }
        public string Mensaje { get; set; }

        public override string ToString()
        {
            return $"{Estado} {Mensaje}";
        }
    }
    public static class Protocolo
    {
        private static readonly ConcurrentDictionary<string, int> _contadorClientes
            = new ConcurrentDictionary<string, int>();

        // Serializa un Pedido a bytes para transmisión
        public static byte[] SerializarPedido(Pedido pedido)
        {
            string mensaje = $"{pedido.Comando} {string.Join(" ", pedido.Parametros)}\n";
            return Encoding.UTF8.GetBytes(mensaje);
        }

        // Deserializa bytes a un Pedido
        public static Pedido DeserializarPedido(byte[] buffer, int bytesRx)
        {
            string mensaje = Encoding.UTF8.GetString(buffer, 0, bytesRx).Trim();
            return Pedido.Procesar(mensaje);
        }

        // Serializa una Respuesta a bytes
        public static byte[] SerializarRespuesta(Respuesta respuesta)
        {
            string mensaje = $"{respuesta.Estado} {respuesta.Mensaje}\n";
            return Encoding.UTF8.GetBytes(mensaje);
        }

        // Resuelve un Pedido (lógica movida desde Servidor)
        public static Respuesta ResolverPedido(Pedido pedido, string direccionCliente)
        {
            Respuesta respuesta = new Respuesta
            {
                Estado = "NOK",
                Mensaje = "Comando no reconocido"
            };

            switch (pedido.Comando)
            {
                case "INGRESO":
                    if (pedido.Parametros.Length == 2 &&
                        pedido.Parametros[0] == "root" &&
                        pedido.Parametros[1] == "admin20")
                    {
                        respuesta = new Respuesta
                        {
                            Estado = "OK",
                            Mensaje = "ACCESO_CONCEDIDO"
                        };
                    }
                    else
                    {
                        respuesta.Mensaje = "ACCESO_NEGADO";
                    }
                    break;

                case "CALCULO":
                    if (pedido.Parametros.Length == 3 && ValidarPlaca(pedido.Parametros[2]))
                    {
                        byte indicadorDia = ObtenerIndicadorDia(pedido.Parametros[2]);
                        respuesta = new Respuesta
                        {
                            Estado = "OK",
                            Mensaje = $"{pedido.Parametros[2]} {indicadorDia}"
                        };
                        _contadorClientes.AddOrUpdate(direccionCliente, 1, (key, oldValue) => oldValue + 1);
                    }
                    else
                    {
                        respuesta.Mensaje = "Placa no válida";
                    }
                    break;

                case "CONTADOR":
                    if (_contadorClientes.TryGetValue(direccionCliente, out int count))
                    {
                        respuesta = new Respuesta
                        {
                            Estado = "OK",
                            Mensaje = count.ToString()
                        };
                    }
                    else
                    {
                        respuesta.Mensaje = "No hay solicitudes previas";
                    }
                    break;
            }
            return respuesta;
        }

        // Validación de placa (método auxiliar)
        private static bool ValidarPlaca(string placa)
        {
            return Regex.IsMatch(placa, @"^[A-Z]{3}[0-9]{4}$");
        }

        // Calcula día de restricción (corregido para Lunes)
        private static byte ObtenerIndicadorDia(string placa)
        {
            int ultimoDigito = int.Parse(placa.Substring(6, 1));
            switch (ultimoDigito)
            {
                case 1:
                case 2:
                    return 0b00100000; // Lunes (32)
                case 3:
                case 4:
                    return 0b00010000; // Martes (16)
                case 5:
                case 6:
                    return 0b00001000;  // Miércoles (8)
                case 7:
                case 8:
                    return 0b00000100;  // Jueves (4)
                case 9:
                case 0:
                    return 0b00000010;  // Viernes (2)
                default:
                    return 0;
            }
        }
    }

}
