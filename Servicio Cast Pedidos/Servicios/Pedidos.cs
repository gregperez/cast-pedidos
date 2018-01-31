using Servicio_Cast_Pedidos.Clases;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Timers;

namespace Servicio_Cast_Pedidos.Servicios
{
    partial class Pedidos : ServiceBase
    {
        #region Atributos

        private Timer tmService = null;

        /// <summary>
        /// Constructor de la clase.
        /// </summary>
        public Pedidos()
        {
            InitializeComponent();
        }

        #endregion

        #region Eventos Principales del Servicio

        /// <summary>
        /// Método encargado de iniciar el servicio. 
        /// </summary>
        /// <param name="args">Arreglo de parametros iniciales del servicio</param>
        protected override void OnStart(string[] args)
        {
            tmService = new Timer();
            tmService.Interval = GetNextIntervalo();
            tmService.Elapsed += new ElapsedEventHandler(TimerElepased);
            tmService.Enabled = true;
            tmService.Start();
        }

        /// <summary>
        /// Método encargado de detener el servicio.
        /// </summary>
        protected override void OnStop()
        {
            tmService.Stop();
        }

        #endregion

        #region Métodos

        /// <summary>
        /// Método temporizador para mantener el servicio corriendo y en ejecución.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimerElepased(object sender, ElapsedEventArgs e)
        {
            tmService.Enabled = false;
            Controlador oController = new Controlador();
            //oController.ConectarOracle();
            //oController.ConectarSAP();
            oController.ProcesarPedidos();
            tmService.Enabled = true;
        }

        /// <summary>
        /// Método para establecer el intervalo de tiempo en que se ejecutara el servicio.
        /// </summary>
        /// <returns>Retorna la cantidad de tiempo</returns>
        private double GetNextIntervalo()
        {
            double miliseconds;
            double seconds;
            double minutes;
            double interval;
            miliseconds = Convert.ToDouble(ConfigurationManager.AppSettings["miliseconds"]);
            seconds = Convert.ToDouble(ConfigurationManager.AppSettings["seconds"]);
            minutes = Convert.ToDouble(ConfigurationManager.AppSettings["minutes"]);
            interval = Convert.ToDouble(ConfigurationManager.AppSettings["interval"]);

            return miliseconds * seconds * minutes * interval;
        }

        #endregion
    }
}
