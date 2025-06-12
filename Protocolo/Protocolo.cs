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

using System.Linq;

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

}
