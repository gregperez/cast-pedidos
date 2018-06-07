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
        public static string GetLineaCredito(string CardCode)
        {
            m_sSQL.Length = 0;

            m_sSQL.Append(" SELECT T0.\"CreditLine\" FROM CAST_PEDIDOS_LIMITE_CREDITO T0 ");
            m_sSQL.AppendFormat(" WHERE T0.\"CardCode\" = '{0}' ", CardCode);

            return m_sSQL.ToString();
        }

        public static string GetLineaCreditoUDO(string CardCode,string codEmpresa)
        {
            m_sSQL.Length = 0;

            m_sSQL.Append(" SELECT T0.\"U_Saldo_disp\" FROM \"@EXX_DET_LINCRED\" T0 ");
            m_sSQL.AppendFormat(" WHERE T0.\"Code\" = '{0}' ", CardCode);
            m_sSQL.AppendFormat(" AND T0.\"U_Cod_Empresa\" = '{0}' ", codEmpresa);
            m_sSQL.AppendFormat(" AND T0.\"U_FechaHasta\" > '{0}' ", DateTime.Now.ToString("yyyyMMdd"));

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
            m_sSQL.AppendFormat(" AND T0.\"ListNum\" = '{0}' ", ListNum);

            return m_sSQL.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string GetParametroValor(string CodParam)
        {
            m_sSQL.Length = 0;

            m_sSQL.Append(" SELECT \"U_EXX_ValParam\" \"ValParam\" FROM \"@EXX_CONFCASTPED\" ");
            m_sSQL.AppendFormat(" WHERE \"U_EXX_CodParam\" = '{0}' ", CodParam);

            return m_sSQL.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string GetCardCode(string RUC)
        {
            m_sSQL.Length = 0;

            m_sSQL.Append(" SELECT T0.\"CardCode\" FROM OCRD T0 ");
            m_sSQL.AppendFormat(" WHERE T0.\"LicTradNum\" = '{0}' ", RUC);
            m_sSQL.Append(" AND T0.\"CardType\" = 'C' ");

            return m_sSQL.ToString();
        }
        
        #endregion
    }
}
