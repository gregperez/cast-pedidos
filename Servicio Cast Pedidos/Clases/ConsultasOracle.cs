using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Servicio_Cast_Pedidos.Clases
{
    class ConsultasOracle
    {
        #region Atributos

        private static StringBuilder m_sSQL = new StringBuilder(); //Variable para la construccion de strings

        #endregion

        #region Metodos

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string GetPedidosCab()
        {
            m_sSQL.Length = 0;

            m_sSQL.Append(" SELECT * FROM gen_pedidos_cab ");
            m_sSQL.Append(" WHERE procesado = 'N' ");
            //m_sSQL.Append(" AND nro_comprobante = '207' ");

            return m_sSQL.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string GetPedidosDet(string valor)
        {
            m_sSQL.Length = 0;

            m_sSQL.Append(" SELECT * FROM gen_pedidos_det "); ;
            m_sSQL.AppendFormat(" WHERE nro_comprobante = '{0}' ", valor.ToString());
            m_sSQL.Append(" AND procesado = 'N' ");

            return m_sSQL.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string UpdatePedidoCab(string valor)
        {
            m_sSQL.Length = 0;

            m_sSQL.Append(" UPDATE gen_pedidos_cab SET procesado = 'S' ");
            m_sSQL.AppendFormat(" WHERE nro_comprobante = '{0}'", valor.ToString());

            return m_sSQL.ToString();
        }

        public static string UpdatePedidoDet(string valor)
        {
            m_sSQL.Length = 0;

            m_sSQL.Append(" UPDATE gen_pedidos_det SET procesado = 'S' ");
            m_sSQL.AppendFormat(" WHERE nro_comprobante = '{0}'", valor.ToString());

            return m_sSQL.ToString();
        }

        public static string EmpresaEquivalencia(string codEmpresa)
        {
            m_sSQL.Length = 0;

            m_sSQL.Append("SELECT e.COD_EMPRESA_SAP as \"codEmpresaSAP\" " +
                        "FROM EMPRESA e " +
                        "WHERE  e.COD_EMPRESA = '" + codEmpresa + "'");
            return m_sSQL.ToString();
        }

        public static string CondicionVentaEquivalencia(string codEmpresa, string codCondicionV)
        {
            m_sSQL.Length = 0;

            m_sSQL.Append("SELECT c.COD_CONDICION_SAP as \"codCondicionSAP\" " +
                        "FROM CONDICIONES_PAGOS c " +
                        "WHERE  c.COD_EMPRESA = '" + codEmpresa + "' AND c.COD_CONDICION='" + codCondicionV + "'");
            return m_sSQL.ToString();
        }

        public static string MonedaEquivalencia(string codMoneda)
        {
            m_sSQL.Length = 0;

            m_sSQL.Append("SELECT m.COD_MONEDA_SAP as \"codMonedaSAP\" " +
                        "FROM MONEDAS m " +
                        "WHERE  m.COD_MONEDA = '" + codMoneda + "'");
            return m_sSQL.ToString();
        }

        public static string SucursalEquivalencia(string codEmpresa, string codsucursal)
        {
            m_sSQL.Length = 0;

            m_sSQL.Append("SELECT s.COD_SUC_SAP as \"codSucSAP\" " +
                        "FROM SUCURSAL s " +
                        "WHERE  s.COD_EMPRESA = '" + codEmpresa + "' AND s.COD_SUCURSAL='" + codsucursal + "'");
            return m_sSQL.ToString();
        }

        public static string EmpleadoEquivalencia(string codPersona)
        {
            m_sSQL.Length = 0;

            m_sSQL.Append("SELECT DISTINCT COD_CLIENTE as \"codCliente\", NOMBRE_CLIENTE as \"nomCliente\" " +
                        "FROM CLIENTES c " +
                        "WHERE  c.COD_PERSONA_INV = '" + codPersona + "' AND c.ESTADO='S'");
            return m_sSQL.ToString();
        }

        #endregion
    }
}
