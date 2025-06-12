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
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using Protocolo;

namespace Cliente
{
    public partial class FrmValidador : Form
    {
        private TcpClient remoto;
        private NetworkStream flujo;

        public FrmValidador()
        {
            InitializeComponent();
        }

        private void FrmValidador_Load(object sender, EventArgs e)
        {
            try
            {
                remoto = new TcpClient("127.0.0.1", 8080);
                flujo = remoto.GetStream();
            }
            catch (SocketException ex)
            {
                MessageBox.Show("No se puedo establecer conexión " + ex.Message,
                    "ERROR");
            }
        }

        private void btnIniciar_Click(object sender, EventArgs e)
        {
            var pedido = new Pedido
            {
                Comando = "INGRESO",
                Parametros = new[] { txtUsuario.Text, txtPassword.Text }
            };

            Respuesta respuesta = HazOperacion(pedido);

            if (respuesta?.Estado == "OK" && respuesta.Mensaje == "ACCESO_CONCEDIDO")
            {
                panPlaca.Enabled = true;
                panLogin.Enabled = false;
            }
            else
            {
                MessageBox.Show("Acceso denegado", "ERROR");
            }
        }

        private Respuesta HazOperacion(Pedido pedido)
        {
            try
            {
                // Serializar y enviar
                byte[] bufferTx = Protocolo.Protocolo.SerializarPedido(pedido);
                flujo.Write(bufferTx, 0, bufferTx.Length);

                // Recibir y deserializar
                byte[] bufferRx = new byte[1024];
                int bytesRx = flujo.Read(bufferRx, 0, bufferRx.Length);
                string mensaje = Encoding.UTF8.GetString(bufferRx, 0, bytesRx).Trim();
                var partes = mensaje.Split(' ');

                return new Respuesta
                {
                    Estado = partes[0],
                    Mensaje = string.Join(" ", partes.Skip(1).ToArray())
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "ERROR");
                return null;
            }
        }

        private void btnConsultar_Click(object sender, EventArgs e)
        {
            string modelo = txtModelo.Text;
            string marca = txtMarca.Text;
            string placa = txtPlaca.Text;

            Pedido pedido = new Pedido
            {
                Comando = "CALCULO",
                Parametros = new[] { modelo, marca, placa }
            };

            Respuesta respuesta = HazOperacion(pedido);
            if (respuesta == null)
            {
                MessageBox.Show("Hubo un error", "ERROR");
                return;
            }

            if (respuesta.Estado == "NOK")
            {
                MessageBox.Show("Error en la solicitud.", "ERROR");
                chkLunes.Checked = false;
                chkMartes.Checked = false;
                chkMiercoles.Checked = false;
                chkJueves.Checked = false;
                chkViernes.Checked = false;
            }
            else
            {
                var partes = respuesta.Mensaje.Split(' ');
                MessageBox.Show("Se recibió: " + respuesta.Mensaje,
                    "INFORMACIÓN");
                byte resultado = Byte.Parse(partes[1]);
                switch (resultado)
                {
                    case 32: // Lunes (0b00100000)
                        chkLunes.Checked = true;
                        chkMartes.Checked = false;
                        chkMiercoles.Checked = false;
                        chkJueves.Checked = false;
                        chkViernes.Checked = false;
                        break;
                    case 16: // Martes (0b00010000)
                        chkMartes.Checked = true;
                        chkLunes.Checked = false;
                        chkMiercoles.Checked = false;
                        chkJueves.Checked = false;
                        chkViernes.Checked = false;
                        break;
                    case 8:  // Miércoles (0b00001000)
                        chkMiercoles.Checked = true;
                        chkLunes.Checked = false;
                        chkMartes.Checked = false;
                        chkJueves.Checked = false;
                        chkViernes.Checked = false;
                        break;
                    case 4:  // Jueves (0b00000100)
                        chkJueves.Checked = true;
                        chkLunes.Checked = false;
                        chkMartes.Checked = false;
                        chkMiercoles.Checked = false;
                        chkViernes.Checked = false;
                        break;
                    case 2:  // Viernes (0b00000010)
                        chkViernes.Checked = true;
                        chkLunes.Checked = false;
                        chkMartes.Checked = false;
                        chkMiercoles.Checked = false;
                        chkJueves.Checked = false;
                        break;
                    default:
                        chkLunes.Checked = false;
                        chkMartes.Checked = false;
                        chkMiercoles.Checked = false;
                        chkJueves.Checked = false;
                        chkViernes.Checked = false;
                        break;
                }
            }
        }

        private void btnNumConsultas_Click(object sender, EventArgs e)
        {
            Pedido pedido = new Pedido
            {
                Comando = "CONTADOR",
                Parametros = new string[0] // No necesita parámetros
            };

            Respuesta respuesta = HazOperacion(pedido);
            if (respuesta == null)
            {
                MessageBox.Show("Hubo un error", "ERROR");
                return;
            }

            if (respuesta.Estado == "NOK")
            {
                MessageBox.Show("Error en la solicitud.", "ERROR");
            }
            else
            {
                MessageBox.Show("El número de pedidos recibidos en este cliente es " + respuesta.Mensaje,
                    "INFORMACIÓN");
            }
        }

        private void FrmValidador_FormClosing(object sender, FormClosingEventArgs e)
        {
            flujo?.Close();
            remoto?.Close();
        }
    }
}