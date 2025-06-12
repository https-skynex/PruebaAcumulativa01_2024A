/* ************************************************************************
                                Practica 07
Integrantes: Kevin Perez  
Fecha de realización: 11/06/2025  
Fecha de entrega: 18/06/2025  

RESULTADOS:  
Al iniciar el formulario, la conexión se cierra automáticamente debido al finally 
en Load, mostrando el mensaje: "No hay conexión" al intentar operaciones. Si el 
servidor retorna OK ABC1234 16 (para Martes), el cliente intenta convertir "16" 
a binario (0b00010000) y activa chkMartes, pero el código actual usa un switch 
con literales binarios no equivalentes a decimales, dejando todos los checkboxes 
desactivados.

CONCLUSIONES:  
1.La interfaz gráfica del cliente ofrece una experiencia intuitiva al guiar al usuario 
mediante paneles condicionales (login → validación de placas). Sin embargo, el 
bloque finally en el evento Load cierra inmediatamente la conexión TCP, impidiendo 
cualquier operación posterior. 
2. Además, la interpretación de la respuesta del comando CALCULO es errónea: 
confunde valores binarios con su representación decimal (16), 
activando incorrectamente los checkboxes de días.

RECOMENDACIONES:  
1. Seria esencial liminar el bloque finally en FrmValidador_Load para mantener la 
conexión activa durante la sesión.
2. Corregir la interpretación del byte en btnConsultar_Click: usar valores decimales 
en el switch (ej: 32 para Lunes, 16 para Martes) en lugar de representaciones binarias.

************************************************************************ */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Cliente
{
    static class Program
    {
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FrmValidador());
        }
    }
}
