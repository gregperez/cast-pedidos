using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Servicio_Cast_Pedidos.Clases
{
    class Controlador
    {
        #region Definiciones

        private string ServerOracle = string.Empty;
        private string UserOracle = string.Empty;
        private string PassOracle = string.Empty;
        private DBOracle dbOracleCab = null;
        private DBOracle dbOracleDet = null;
        private DBSap dbSap;
        private SAPbobsCOM.Company oCompany;

        /// <summary>
        /// 
        /// </summary>
        public Controlador()
        {
            this.ServerOracle = ConfigurationManager.AppSettings["ServerOracle"];
            this.UserOracle = ConfigurationManager.AppSettings["OracleUser"];
            this.PassOracle = ConfigurationManager.AppSettings["OraclePwd"];
        }

        #endregion

        #region Métodos

        public void ConectarOracle()
        {
            dbOracleCab = new DBOracle(ServerOracle, UserOracle, PassOracle);

            string fecha = DateTime.Now.ToString();
            string strPathLog = @"C:\Users\gperez\Desktop\Pedidos.txt";
            TextWriter tw = new StreamWriter(strPathLog, true);

            try
            {
                if (dbOracleCab.EjecutaSQL(ConsultasOracle.GetPedidosCab()))
                {
                    while (dbOracleCab.oDataReader.Read())
                    {
                        string nro_comprobante = dbOracleCab.oDataReader["nro_comprobante"].ToString();
                        tw.WriteLine(String.Format("cod_empresa: {0}", dbOracleCab.oDataReader["cod_empresa"].ToString()));
                        tw.WriteLine(String.Format("nro_comprobante: {0}", dbOracleCab.oDataReader["nro_comprobante"].ToString()));
                        tw.WriteLine(String.Format("fec_comprobante: {0}", dbOracleCab.oDataReader["fec_comprobante"].ToString()));
                        tw.WriteLine(String.Format("cod_cliente: {0}", dbOracleCab.oDataReader["cod_cliente"].ToString()));
                        tw.WriteLine(String.Format("monto_total: {0}", dbOracleCab.oDataReader["monto_total"].ToString()));
                        
                        dbOracleDet = new DBOracle(ServerOracle, UserOracle, PassOracle);
                        if (dbOracleDet.EjecutaSQL(ConsultasOracle.GetPedidosDet(nro_comprobante)))
                        {
                            while (dbOracleDet.oDataReader.Read())
                            {
                                tw.WriteLine(String.Format("nro_item: {0}", dbOracleDet.oDataReader["nro_item"].ToString()));
                                tw.WriteLine(String.Format("cod_articulo: {0}", dbOracleDet.oDataReader["cod_articulo"].ToString()));
                                tw.WriteLine(String.Format("cantidad: {0}", dbOracleDet.oDataReader["cantidad"].ToString()));
                                tw.WriteLine(String.Format("precio_unitario: {0}", dbOracleDet.oDataReader["precio_unitario"].ToString()));
                                tw.WriteLine(String.Format("monto_total: {0}", dbOracleDet.oDataReader["monto_total"].ToString()));
                                tw.WriteLine(String.Format("total_iva: {0}", dbOracleDet.oDataReader["total_iva"].ToString()));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Gestion de errores.
            }
            finally
            {
                tw.Close();
                dbOracleCab.Dispose();
                dbOracleDet.Dispose();
            }
        }

        /// <summary>
        /// Método para comprobar conexión con SAP.
        /// </summary>
        /// <returns>Retorna string con el nombre de la BD a la cual nos conectamos. Código y 
        /// mensaje de error en caso contrario</returns>
        public void ConectarSAP()
        {
            dbSap = new DBSap();
            string fecha = DateTime.Now.ToString();
            string strPathLog = @"C:\Users\gperez\Desktop\Log.txt";
            TextWriter tw = new StreamWriter(strPathLog, true);
            if (dbSap.Conectar())
            {
                oCompany = dbSap.oCompany;
                tw.WriteLine(String.Format("A las {0} nos conectamos a {1} ", fecha, oCompany.CompanyName));
            }
            else
                tw.WriteLine(String.Format("Sin conexión a las {0}. Ocurrio el siguiente error: {1} - {2}", fecha, dbSap.iError, dbSap.sError));
            tw.Close();
        }

        public void ProcesarPedidos()
        {
            dbOracleCab = new DBOracle(ServerOracle, UserOracle, PassOracle);
            dbSap = new DBSap();
            
            SAPbobsCOM.Documents oDoc = null;

            int Respuesta = 0;
            string MsgErrSBO = "";
            string identi = "";
            string nro_comprobante = "";
            bool esPedido = true;
            bool creditoOK = true;
            bool stockOK = true;
            bool precioOK = true;
            string motivoOferta = "";

            try
            {
                if (dbOracleCab.EjecutaSQL(ConsultasOracle.GetPedidosCab()))
                {
                    while (dbOracleCab.oDataReader.Read())
                    {
                        if (dbSap.oCompany == null)
                        {
                            dbSap.Conectar();
                            oCompany = dbSap.oCompany;
                        }

                        nro_comprobante = dbOracleCab.oDataReader["nro_comprobante"].ToString();
                        dbOracleDet = new DBOracle(ServerOracle, UserOracle, PassOracle);
                        dbOracleDet.EjecutaSQL(ConsultasOracle.GetPedidosDet(nro_comprobante));

                        if (!ValidarCreditoDisponible())
                        {
                            creditoOK = false;
                            esPedido = false;
                            motivoOferta = String.Format("Oferta de venta creada por rechazo del Pedido {0}, " +
                                "por limite de crédito excedido.", nro_comprobante);
                        }
                        if (!ValidarStockDisponible(ref stockOK, ref precioOK))
                        {
                            esPedido = false;
                            if (!stockOK)
                                motivoOferta = String.Format("Oferta de venta creada por rechazo del Pedido {0}, " +
                                    "por falta de disponibilidad de stock.", nro_comprobante);
                            if (!precioOK)
                                motivoOferta = String.Format("Oferta de venta creada por rechazo del Pedido {0}, " +
                                    "por diferencia de precio.", nro_comprobante);
                        }

                        CrearDocumento(oDoc, nro_comprobante, Respuesta, MsgErrSBO, 
                            identi, esPedido, creditoOK, motivoOferta);
                        creditoOK = true;
                        esPedido = true;
                        stockOK = true;
                        precioOK = true;

                        nro_comprobante = "";
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.EventLog.WriteEntry("Application", String.Format("En el método {0}. Ocurrió el siguiente error: {1} - {2} ",
                    System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
            }
            finally
            {
                dbOracleCab.Dispose();
                dbOracleDet.Dispose();
            }
        }

        public bool ValidarCreditoDisponible()
        {
            bool esValido = true;
            SAPbobsCOM.Recordset oRecordset = null;
            oRecordset = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
            oRecordset.DoQuery(ConsultasSap.GetLineaCredito("C0000007764"));
            //oRecordset.DoQuery(ConsultasSap.GetLineaCredito(dbOracleCab.oDataReader["cod_cliente"].ToString()));
            if (oRecordset.RecordCount > 0)
            {
                if (Convert.ToDouble(dbOracleCab.oDataReader["monto_total"].ToString()) > Convert.ToDouble(oRecordset.Fields.Item("CreditLine").Value))
                    esValido = false;
            }

            return esValido;
        }

        public bool ValidarStockDisponible(ref bool stockOK, ref bool precioOK)
        {
            bool esValido = true;
            SAPbobsCOM.Recordset oRecordset = null;
            oRecordset = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
            DBOracle detalleCopy = new DBOracle(ServerOracle, UserOracle, PassOracle);
            detalleCopy = dbOracleDet;

            while (detalleCopy.oDataReader.Read() && stockOK && precioOK)
            {
                string cod_articulo = "";
                double cantidad = 0;
                double precio = 0;
                cod_articulo = detalleCopy.oDataReader["cod_articulo"].ToString();
                cantidad = Convert.ToDouble(detalleCopy.oDataReader["cantidad"].ToString());
                precio = Convert.ToDouble(detalleCopy.oDataReader["precio_unitario"].ToString());
                oRecordset.DoQuery(ConsultasSap.GetItemStock(cod_articulo));
                if (oRecordset.RecordCount > 0)
                {
                    if (cantidad > Convert.ToDouble(oRecordset.Fields.Item("Stock").Value.ToString()))
                    {
                        stockOK = false;
                        esValido = false;
                        break;
                    }
                }
                    
                oRecordset.DoQuery(ConsultasSap.GetPrecioLista(cod_articulo));
                if (oRecordset.RecordCount > 0)
                {
                    if (precio > Convert.ToDouble(oRecordset.Fields.Item("Price").Value.ToString()))
                    {
                        precioOK = false;
                        esValido = false;
                        break;
                    }
                }
            }

            return esValido;
        }

        public void CrearDocumento(SAPbobsCOM.Documents oDoc, string nro_comprobante, 
            int Respuesta, string MsgErrSBO, string identi, bool esPedido, 
            bool creditoOK, string motivoOferta)
        {
            if (esPedido)
                oDoc = (SAPbobsCOM.Documents)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oOrders);
            else
                oDoc = (SAPbobsCOM.Documents)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oQuotations);

            oDoc.CardCode = "C0000007764"; // dbOracleCab.oDataReader["cod_cliente"].ToString();
            oDoc.DocDate = Convert.ToDateTime(dbOracleCab.oDataReader["fec_comprobante"].ToString());
            oDoc.DocDueDate = Convert.ToDateTime(dbOracleCab.oDataReader["fec_comprobante"].ToString());
            oDoc.SalesPersonCode = 1; // Convert.ToInt32(dbOracleCab.oDataReader["cod_vendedor"].ToString());
                                      //oDoc.PaymentMethod = dbOracleCab.oDataReader["cod_condicion_venta"].ToString();
            oDoc.DocCurrency = "GS"; // dbOracleCab.oDataReader["cod_moneda"].ToString();
            oDoc.DocRate = Convert.ToDouble(dbOracleCab.oDataReader["tip_cambio"].ToString());
            //oDoc.DocumentStatus = SAPbobsCOM.BoStatus.bost_Open;// dbOracleCab.oDataReader["cod_vendedor"].ToString();
            oDoc.Comments = dbOracleCab.oDataReader["comentario"].ToString();
            //oDoc.DocTotal = Convert.ToDouble(dbOracleCab.oDataReader["monto_total"].ToString());
            oDoc.BPL_IDAssignedToInvoice = 3; //Convert.ToInt32(dbOracleCab.oDataReader["cod_empresa"].ToString());

            oDoc.UserFields.Fields.Item("U_Tipo").Value = dbOracleCab.oDataReader["tip_comprobante"].ToString();
            oDoc.UserFields.Fields.Item("U_Serie").Value = dbOracleCab.oDataReader["ser_comprobante"].ToString();
            oDoc.UserFields.Fields.Item("U_Numero").Value = dbOracleCab.oDataReader["nro_comprobante"].ToString();
            /*oDoc.UserFields.Fields.Item("U_cod_provincia").Value = dbOracleCab.oDataReader["cod_provincia"].ToString();
            oDoc.UserFields.Fields.Item("U_cod_ciudad").Value = dbOracleCab.oDataReader["cod_ciudad"].ToString();
            oDoc.UserFields.Fields.Item("U_enviar_ypane").Value = dbOracleCab.oDataReader["enviar_ypane"].ToString();
            oDoc.UserFields.Fields.Item("U_wms_preparado").Value = dbOracleCab.oDataReader["wms_preparado"].ToString();
            oDoc.UserFields.Fields.Item("U_wms_id_transaccion").Value = dbOracleCab.oDataReader["wms_id_transaccion"].ToString();
            if (!creditoOK)
                oDoc.UserFields.Fields.Item("U_control").Value = dbOracleCab.oDataReader["control"].ToString();
            oDoc.UserFields.Fields.Item("U_procesado").Value = dbOracleCab.oDataReader["procesado"].ToString();*/
            if (!esPedido)
                oDoc.UserFields.Fields.Item("U_MotivoOferta").Value = motivoOferta;
            oDoc.DocType = SAPbobsCOM.BoDocumentTypes.dDocument_Items;

            dbOracleDet = new DBOracle(ServerOracle, UserOracle, PassOracle);
            int i = 0;
            if (dbOracleDet.EjecutaSQL(ConsultasOracle.GetPedidosDet(nro_comprobante)))
            {
                while (dbOracleDet.oDataReader.Read())
                {
                    oDoc.Lines.ItemCode = dbOracleDet.oDataReader["cod_articulo"].ToString();
                    oDoc.Lines.Quantity = Convert.ToDouble(dbOracleDet.oDataReader["cantidad"].ToString());
                    oDoc.Lines.Price = Convert.ToDouble(dbOracleDet.oDataReader["precio_unitario"].ToString());
                    oDoc.Lines.TaxCode = "IVA_10";

                    oDoc.Lines.SetCurrentLine(i);
                    oDoc.Lines.Add();
                    i++;
                }
            }

            Respuesta = oDoc.Add();
            if (Respuesta != 0)
            {
                oCompany.GetLastError(out Respuesta, out MsgErrSBO);
                //Application.SBO_Application.MessageBox(Program.AddOnName + ": Error al generar el asiento:" + MsgErrSBO);
                System.Diagnostics.EventLog.WriteEntry("Application", String.Format("En el método {0}. Ocurrió el siguiente error: {1} - {2} ",
                    System.Reflection.MethodBase.GetCurrentMethod().Name, Respuesta, MsgErrSBO));
            }
            else
            {

                identi = oCompany.GetNewObjectKey();
                DBOracle dbOracleUpdate = new DBOracle(ServerOracle, UserOracle, PassOracle);
                int filas = 0;
                dbOracleUpdate.EjecutaSQL(ConsultasOracle.UpdatePedido(nro_comprobante), ref filas);
                //Application.SBO_Application.MessageBox("La Matricula " + identi + " fue creada exitosamente ");
                System.Diagnostics.EventLog.WriteEntry("Application", String.Format("Cantidad filas actualizadas: {0}, del documento {1} ",
                    filas, nro_comprobante));

            }
        }

        #endregion
    }
}
