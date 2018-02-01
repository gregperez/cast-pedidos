﻿using System;
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
        public static string GetLineaCredito(string CardCode)
        {
            m_sSQL.Length = 0;

            m_sSQL.Append(" SELECT T0.\"CreditLine\" FROM CAST_PEDIDO_LIMITE_CREDITO T0 ");
            m_sSQL.AppendFormat(" WHERE T0.\"CardCode\" = '{0}' ", CardCode);

            return m_sSQL.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string GetItemStock(string ItemCode, string WhsCode)
        {
            m_sSQL.Length = 0;

            m_sSQL.Append(" SELECT T0.\"Stock\" FROM CAST_PEDIDOS_STOCK_DISPONIBLE T0 ");
            m_sSQL.AppendFormat(" WHERE T0.\"ItemCode\" = '{0}' ", ItemCode);
            m_sSQL.AppendFormat(" AND T0.\"WhsCode\" = '{0}' ", WhsCode);

            return m_sSQL.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string GetPrecioLista(string ItemCode, int ListNum)
        {
            m_sSQL.Length = 0;

            m_sSQL.Append(" SELECT T0.\"Price\" FROM CAST_PEDIDOS_PRECIO_LISTA T0 ");
            m_sSQL.AppendFormat(" WHERE T0.\"ItemCode\" = '{0}' ", ItemCode);
            m_sSQL.AppendFormat(" AND T1.\"ListNum\" = '{0}' ", ListNum);

            return m_sSQL.ToString();
        }

        #endregion
    }
}
