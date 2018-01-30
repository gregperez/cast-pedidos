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
            m_sSQL.Append(" WHERE procesado = '1' ");
            //m_sSQL.Append(" AND nro_comprobante = '206' ");

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
        public static string UpdatePedido(string valor)
        {
            m_sSQL.Length = 0;

            m_sSQL.Append(" UPDATE gen_pedidos_cab SET procesado = 0 ");
            m_sSQL.AppendFormat(" WHERE nro_comprobante = '{0}'", valor.ToString());

            return m_sSQL.ToString();
        }

        #endregion
    }
}
