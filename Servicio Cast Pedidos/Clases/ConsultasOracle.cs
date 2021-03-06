﻿using System;
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
            //m_sSQL.Append(" WHERE nro_comprobante = '16' ");

            return m_sSQL.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string GetPedidosDet(string valor)
        {
            m_sSQL.Length = 0;

            m_sSQL.Append(" SELECT d1.nro_item \"nro_item\", d1.cod_articulo \"cod_articulo\", ");
            m_sSQL.Append(" nvl(d1.cantidad, 0) \"cantidad\", nvl(d1.precio_unitario, 0) \"precio_unitario\", ");
            m_sSQL.Append(" nvl(d1.monto_total, 0) \"monto_total\", nvl(d1.total_iva, 0) \"total_iva\" ");
            m_sSQL.Append(" FROM gen_pedidos_det d1 ");
            m_sSQL.AppendFormat(" WHERE d1.nro_comprobante = '{0}' ", valor.ToString());
            //m_sSQL.Append(" AND d1.procesado = 'N' ");

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

        public static string EmpleadoEquivalencia(string codPersona,string codEmpresa)
        {
            m_sSQL.Length = 0;

            m_sSQL.Append("SELECT DISTINCT COD_CLIENTE as \"codCliente\", NOMBRE_CLIENTE as \"nomCliente\" " +
                        "FROM CLIENTES c " +
                        "WHERE  c.COD_PERSONA_INV = '" + codPersona + "'AND COD_EMPRESA='"+ codEmpresa  + "' AND c.ESTADO='S'");
            return m_sSQL.ToString();
        }

        public static string ValidarPedidoCompleto()
        {
            m_sSQL.Length = 0;

            m_sSQL.Append("SELECT DISTINCT(T0.nro_comprobante),  T0.cod_empresa, T0.cod_sucursal, T0.tip_comprobante, " +
                "T0.ser_comprobante, T0.nro_comprobante, T0.fec_comprobante, T0.cod_cliente, T0.cod_vendedor, " +
                "T0.cod_condicion_venta, T0.cod_lista_precio, T0.cod_moneda, T0.tip_cambio, T0.estado, T0.cod_usuario, " +
                "T0.comentario, T0.ruc, T0.dir_cliente, T0.tel_cliente, T0.cod_provincia, T0.cod_ciudad, T0.enviar_ypane, " +
                "T0.wms_preparado, T0.wms_id_transaccion, T0.solo_credito, T0.monto_total, T0.procesado, T0.nro_pedido, " +
                "T0.origen, T0.docentry, T0.typeobject, T0.tip_pedido, T0.preparo, T0.wms_preparo, T0.fec_hora_ingreso " +
                "FROM gen_pedidos_cab T0 ");
            m_sSQL.Append("JOIN gen_pedidos_det T1 ON T0.nro_comprobante = T1.nro_comprobante ");
            m_sSQL.Append(" WHERE T0.procesado = 'N' ");

            return m_sSQL.ToString();
        }

        #endregion
    }
}
