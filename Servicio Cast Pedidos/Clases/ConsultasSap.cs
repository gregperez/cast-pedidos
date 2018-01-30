using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Servicio_Cast_Pedidos.Clases
{
    class ConsultasSap
    {
        #region Atributos

        private static StringBuilder m_sSQL = new StringBuilder(); //Variable para la construccion de strings

        #endregion

        #region Metodos

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string GetLineaCredito(string valor)
        {
            m_sSQL.Length = 0;

            m_sSQL.Append(" SELECT IFNULL(T0.\"CreditLine\", 0) AS \"CreditLine\" FROM OCRD T0 ");
            m_sSQL.AppendFormat(" WHERE T0.\"CardCode\" = '{0}' ", valor);

            return m_sSQL.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string GetItemStock(string valor)
        {
            m_sSQL.Length = 0;

            m_sSQL.Append(" SELECT ");
            m_sSQL.Append(" SUM(T0.\"OnHand\" - (T0.\"IsCommited\" - IFNULL( (SELECT ");
            m_sSQL.Append(" SUM(T5.\"Quantity\") ");
            m_sSQL.Append(" FROM RDR1 T5, ORDR T6 ");
            m_sSQL.Append(" WHERE T5.\"ItemCode\" = T0.\"ItemCode\" ");
            m_sSQL.Append(" AND T5.\"WhsCode\" = T0.\"WhsCode\" ");
            m_sSQL.Append(" AND T6.\"CANCELED\" = 'N' ");
            m_sSQL.Append(" AND T6.\"PoPrss\" = 'Y' ");
            m_sSQL.Append(" AND T6.\"DocStatus\" = 'O' ");
            m_sSQL.Append(" AND T5.\"DocEntry\" = T6.\"DocEntry\"), ");
            m_sSQL.Append(" 0))) AS \"Stock\", ");
            m_sSQL.Append(" T0.\"ItemCode\" ");
            m_sSQL.Append(" FROM OITW T0 ");
            m_sSQL.AppendFormat(" WHERE T0.\"ItemCode\" = '{0}' ", valor);
            m_sSQL.AppendFormat(" AND T0.\"WhsCode\" = '{0}' ", "300-28");
            m_sSQL.Append(" GROUP BY T0.\"ItemCode\" ");

            return m_sSQL.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string GetPrecioLista(string valor)
        {
            m_sSQL.Length = 0;

            m_sSQL.Append(" SELECT IFNULL(T0.\"Price\", 0) AS \"Price\" FROM ITM1 T0 ");
            m_sSQL.Append(" INNER JOIN OPLN T1 ON T0.\"PriceList\" = T1.\"ListNum\" ");
            m_sSQL.AppendFormat(" WHERE T0.\"ItemCode\" = '{0}' ", valor);
            m_sSQL.AppendFormat(" AND T1.\"ListNum\" = '{0}' ", "1");

            return m_sSQL.ToString();
        }

        #endregion
    }
}
