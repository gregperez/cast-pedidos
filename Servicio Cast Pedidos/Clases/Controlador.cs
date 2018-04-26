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
            string strPathLog = @"C:\Users\Exxispydesa2\Desktop\Pedidos.txt";
            TextWriter tw = new StreamWriter(strPathLog, true);

            string nro_comprobante = "";

            try
            {
                if (dbOracleCab.EjecutaSQL(ConsultasOracle.GetPedidosCab()))
                {
                    while (dbOracleCab.oDataReader.Read())
                    {
                        nro_comprobante = dbOracleCab.oDataReader["nro_comprobante"].ToString();
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
                CrearRegistroLog(ex.HResult.ToString(), ex.Message.ToString(), nro_comprobante);
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
            string strPathLog = @"C:\Users\Exxispydesa2\Desktop\Log.txt";
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
            SAPbobsCOM.Recordset oRecordset = null;

            int Respuesta = 0;
            string MsgErrSBO = "";
            string identi = "";
            string nro_comprobante = "";
            string error_comprobante = "";
            bool esPedido = true;
            bool creditoOK = true;
            bool stockOK = true;
            bool precioOK = true;
            string almacen = "";
            int listaPrecio = 0;

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

                        error_comprobante = dbOracleCab.oDataReader["tip_comprobante"].ToString() + "-" + dbOracleCab.oDataReader["ser_comprobante"].ToString() + "-" + dbOracleCab.oDataReader["nro_comprobante"].ToString();
                        nro_comprobante = dbOracleCab.oDataReader["nro_comprobante"].ToString();
                        dbOracleDet = new DBOracle(ServerOracle, UserOracle, PassOracle);
                        dbOracleDet.EjecutaSQL(ConsultasOracle.GetPedidosDet(nro_comprobante));

                        oRecordset = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);

                        oRecordset.DoQuery(ConsultasSap.GetParametroValor("ListaPrecio"));
                        if (oRecordset.RecordCount > 0)
                        {
                            listaPrecio = Convert.ToInt32(oRecordset.Fields.Item("ValParam").Value.ToString());
                        }

                        if (!ValidarCreditoDisponible(ref almacen))
                        {
                            esPedido = false;
                            creditoOK = false;
                        }
                        if (!ValidarStockyPrecio(ref stockOK, ref precioOK, almacen, listaPrecio))
                        {
                            esPedido = false;
                        }

                        CrearPedido(oDoc, nro_comprobante, Respuesta, MsgErrSBO, 
                            identi, esPedido, creditoOK, stockOK, precioOK, almacen,
                            listaPrecio);
                        esPedido = true;
                        creditoOK = true;
                        stockOK = true;
                        precioOK = true;

                        nro_comprobante = "";
                    }
                }
            }
            catch (Exception ex)
            {
                CrearRegistroLog(ex.HResult.ToString(), ex.Message.ToString(), nro_comprobante);
                WriteErrorLog("ProcesarPedidos:" + error_comprobante + " Mensaje:" + ex.Message.ToString());
            }
            finally
            {
                dbSap.DesconectarSAP(oCompany);
            }
        }

        public bool ValidarCreditoDisponible(ref string almacen)
        {
            bool esValido = false;
            SAPbobsCOM.Recordset oRecordset = null;
            string cardCode = string.Empty;
            oRecordset = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);

            DBOracle consultas = new DBOracle(ServerOracle, UserOracle, PassOracle);

            string empresaSAP = "";
            consultas.EjecutaSQL(ConsultasOracle.EmpresaEquivalencia(dbOracleCab.oDataReader["cod_empresa"].ToString()));
            if (consultas.oDataReader.Read())
            {
                empresaSAP = consultas.oDataReader["codEmpresaSAP"].ToString();
            }

            consultas.EjecutaSQL(ConsultasOracle.EmpleadoEquivalencia(dbOracleCab.oDataReader["cod_cliente"].ToString(), empresaSAP));
            if (consultas.oDataReader.Read())
            {
                cardCode = consultas.oDataReader["codCliente"].ToString();
            }
            consultas.EjecutaSQL(ConsultasOracle.SucursalEquivalencia(dbOracleCab.oDataReader["cod_empresa"].ToString(), dbOracleCab.oDataReader["cod_sucursal"].ToString()));
            if (consultas.oDataReader.Read())
            {
                almacen = consultas.oDataReader["codSucSAP"].ToString();
            }

            oRecordset.DoQuery(ConsultasSap.GetLineaCreditoUDO(cardCode, empresaSAP));
            if (oRecordset.RecordCount > 0)
            {
                if (Convert.ToDouble(oRecordset.Fields.Item("U_Saldo_disp").Value) > Convert.ToDouble(dbOracleCab.oDataReader["monto_total"].ToString()))
                    esValido = true;
            }

            return esValido;
        }

        public bool ValidarStockyPrecio(ref bool stockOK, ref bool precioOK, string almacen,
            int listaPrecio)
        {
            bool esValido = true;
            SAPbobsCOM.Recordset oRecordset = null;
            oRecordset = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
            DBOracle detalleCopy = new DBOracle(ServerOracle, UserOracle, PassOracle);
            detalleCopy = dbOracleDet;

            while (detalleCopy.oDataReader.Read())
            {
                string cod_articulo = "";
                double cantidad = 0;
                double precio = 0;
                cod_articulo = detalleCopy.oDataReader["cod_articulo"].ToString();
                cantidad = Convert.ToDouble(detalleCopy.oDataReader["cantidad"].ToString());
                precio = Convert.ToDouble(detalleCopy.oDataReader["precio_unitario"].ToString());
                oRecordset.DoQuery(ConsultasSap.GetItemStock(cod_articulo, almacen));
                if (oRecordset.RecordCount > 0)
                {
                    if (cantidad > Convert.ToDouble(oRecordset.Fields.Item("Stock").Value.ToString()))
                    {
                        stockOK = false;
                        esValido = false;
                    }
                }
                    
                oRecordset.DoQuery(ConsultasSap.GetPrecioLista(cod_articulo, listaPrecio));
                if (oRecordset.RecordCount > 0)
                {
                    if (precio < Convert.ToDouble(oRecordset.Fields.Item("Price").Value.ToString()))
                    {
                        precioOK = false;
                        esValido = false;
                    }
                }
            }

            return esValido;
        }

        public void CrearPedido(SAPbobsCOM.Documents oDoc, string nro_comprobante, 
            int Respuesta, string MsgErrSBO, string identi, bool esPedido, 
            bool creditoOK, bool stockOK, bool precioOK, string almacen,
            int listaPrecio)
        {
            DBOracle consultas = new DBOracle(ServerOracle, UserOracle, PassOracle);
            DBOracle dbOracleUpdate = new DBOracle(ServerOracle, UserOracle, PassOracle);

            SAPbobsCOM.Recordset oRecordset = null;
            oRecordset = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
            string empresa = string.Empty;
            string empresaSAp = "";
            string nroPedido = "";
            int filas = 0;

            if (esPedido)
                oDoc = (SAPbobsCOM.Documents)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oOrders);
            else
                oDoc = (SAPbobsCOM.Documents)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oQuotations);

            //oRecordset.DoQuery(ConsultasSap.GetCardCode(dbOracleCab.oDataReader["ruc"].ToString()));
            //if (oRecordset.RecordCount > 0)
            //{
            //    oDoc.CardCode = oRecordset.Fields.Item("CardCode").Value.ToString();
            //}
            empresa = dbOracleCab.oDataReader["cod_empresa"].ToString();

            consultas.EjecutaSQL(ConsultasOracle.EmpresaEquivalencia(empresa));
            if (consultas.oDataReader.Read())
            {
                oDoc.BPL_IDAssignedToInvoice = Convert.ToInt32(consultas.oDataReader["codEmpresaSAP"].ToString());
                empresaSAp = consultas.oDataReader["codEmpresaSAP"].ToString();
            }

            consultas.EjecutaSQL(ConsultasOracle.EmpleadoEquivalencia(dbOracleCab.oDataReader["cod_cliente"].ToString(), empresaSAp));
            if (consultas.oDataReader.Read())
            {
                oDoc.CardCode = consultas.oDataReader["codCliente"].ToString();
            }
            
            oDoc.DocDate = Convert.ToDateTime(dbOracleCab.oDataReader["fec_comprobante"].ToString());
            oDoc.DocDueDate = Convert.ToDateTime(dbOracleCab.oDataReader["fec_comprobante"].ToString());

            //oDoc.SalesPersonCode = Convert.ToInt32(dbOracleCab.oDataReader["cod_vendedor"].ToString());

            consultas.EjecutaSQL(ConsultasOracle.CondicionVentaEquivalencia(empresa, dbOracleCab.oDataReader["cod_condicion_venta"].ToString()));
            if (consultas.oDataReader.Read())
            {
                oDoc.PaymentGroupCode = Convert.ToInt32(consultas.oDataReader["codCondicionSAP"].ToString());
            }

            consultas.EjecutaSQL(ConsultasOracle.MonedaEquivalencia(dbOracleCab.oDataReader["cod_moneda"].ToString()));
            if (consultas.oDataReader.Read())
            {
                oDoc.DocCurrency = consultas.oDataReader["codMonedaSAP"].ToString();
            }

            if (!dbOracleCab.oDataReader["tip_cambio"].ToString().Equals(""))
            {
                oDoc.DocRate = Convert.ToDouble(dbOracleCab.oDataReader["tip_cambio"].ToString());
            }
            //oDoc.DocumentStatus = SAPbobsCOM.BoStatus.bost_Open;// dbOracleCab.oDataReader["estado"].ToString();
            oDoc.Comments = dbOracleCab.oDataReader["comentario"].ToString();
            oDoc.FederalTaxID = dbOracleCab.oDataReader["ruc"].ToString();
            oDoc.Address = dbOracleCab.oDataReader["dir_cliente"].ToString();
            //oDoc.DocTotal = Convert.ToDouble(dbOracleCab.oDataReader["monto_total"].ToString());
            nroPedido = dbOracleCab.oDataReader["tip_comprobante"].ToString() + "-" + dbOracleCab.oDataReader["ser_comprobante"].ToString() + "-" + dbOracleCab.oDataReader["nro_comprobante"].ToString();
            oDoc.UserFields.Fields.Item("U_Tipo").Value = dbOracleCab.oDataReader["tip_comprobante"].ToString();
            oDoc.UserFields.Fields.Item("U_Serie").Value = dbOracleCab.oDataReader["ser_comprobante"].ToString();
            oDoc.UserFields.Fields.Item("U_Numero").Value = dbOracleCab.oDataReader["nro_comprobante"].ToString();
            oDoc.UserFields.Fields.Item("U_cod_provincia").Value = dbOracleCab.oDataReader["cod_provincia"].ToString();
            oDoc.UserFields.Fields.Item("U_cod_ciudad").Value = dbOracleCab.oDataReader["cod_ciudad"].ToString();
            oDoc.UserFields.Fields.Item("U_enviar_ypane").Value = dbOracleCab.oDataReader["enviar_ypane"].ToString();
            oDoc.UserFields.Fields.Item("U_wms_preparado").Value = dbOracleCab.oDataReader["wms_preparado"].ToString();
            oDoc.UserFields.Fields.Item("U_wms_id_transaccion").Value = dbOracleCab.oDataReader["wms_id_transaccion"].ToString();
            oDoc.UserFields.Fields.Item("U_control").Value = dbOracleCab.oDataReader["solo_credito"].ToString();
            string origen = "";
            if (dbOracleCab.oDataReader["origen"].ToString().Equals(""))
            {
                origen = "CAST";
            }
            else
            {
                origen = "INVENTIVA";
            }
            oDoc.UserFields.Fields.Item("U_DocOrigen").Value = origen;
            
            if (!creditoOK)
            {
                oDoc.UserFields.Fields.Item("U_LimiCrediVal").Value = "S";
            }
            if (!stockOK)
            {
                oDoc.UserFields.Fields.Item("U_StockVal").Value = "S";
            }
            if (!precioOK)
            {
                oDoc.UserFields.Fields.Item("U_PrecioVal").Value = "S";
            }
            oDoc.DocType = SAPbobsCOM.BoDocumentTypes.dDocument_Items;

            dbOracleDet = new DBOracle(ServerOracle, UserOracle, PassOracle);
            int i = 0;
            if (dbOracleDet.EjecutaSQL(ConsultasOracle.GetPedidosDet(nro_comprobante)))
            {
                while (dbOracleDet.oDataReader.Read())
                {
                    oDoc.Lines.ItemCode = dbOracleDet.oDataReader["cod_articulo"].ToString();
                    oDoc.Lines.Quantity = Convert.ToDouble(dbOracleDet.oDataReader["cantidad"].ToString());
                    oDoc.Lines.UnitPrice = Convert.ToDouble(dbOracleDet.oDataReader["precio_unitario"].ToString());
                    oDoc.Lines.TaxCode = "IVA_10";

                    if (!stockOK && !precioOK)
                    {
                        double cantidad = 0;
                        double precio = 0;
                        oRecordset.DoQuery(ConsultasSap.GetItemStock(oDoc.Lines.ItemCode, almacen));
                        if (oRecordset.RecordCount > 0)
                        {
                            if (oDoc.Lines.Quantity > Convert.ToDouble(oRecordset.Fields.Item("Stock").Value.ToString()))
                            {
                                cantidad = Convert.ToDouble(oRecordset.Fields.Item("Stock").Value.ToString());
                            }
                        }
                        oRecordset.DoQuery(ConsultasSap.GetPrecioLista(oDoc.Lines.ItemCode, listaPrecio));
                        if (oRecordset.RecordCount > 0)
                        {
                            if (oDoc.Lines.UnitPrice < Convert.ToDouble(oRecordset.Fields.Item("Price").Value.ToString()))
                            {
                                precio = Convert.ToDouble(oRecordset.Fields.Item("Price").Value.ToString());
                            }
                        }
                        oDoc.Lines.UserFields.Fields.Item("U_MotivoOferta").Value =
                            String.Format("Cantidad solicitada: {0}, disponible: {1}. Precio venta: {2}, lista: {3}.",
                            oDoc.Lines.Quantity, cantidad, oDoc.Lines.UnitPrice, precio);
                    }
                    else
                    {
                        if (!stockOK)
                        {
                            oRecordset.DoQuery(ConsultasSap.GetItemStock(oDoc.Lines.ItemCode, almacen));
                            if (oRecordset.RecordCount > 0)
                            {
                                if (oDoc.Lines.Quantity > Convert.ToDouble(oRecordset.Fields.Item("Stock").Value.ToString()))
                                {
                                    oDoc.Lines.UserFields.Fields.Item("U_MotivoOferta").Value =
                                        String.Format("Cantidad solicitada ({0}) supera el stock disponible ({1}).",
                                        oDoc.Lines.Quantity, Convert.ToDouble(oRecordset.Fields.Item("Stock").Value.ToString()));
                                }
                            }
                        }
                        if (!precioOK)
                        {
                            oRecordset.DoQuery(ConsultasSap.GetPrecioLista(oDoc.Lines.ItemCode, listaPrecio));
                            if (oRecordset.RecordCount > 0)
                            {
                                if (oDoc.Lines.UnitPrice < Convert.ToDouble(oRecordset.Fields.Item("Price").Value.ToString()))
                                {
                                    oDoc.Lines.UserFields.Fields.Item("U_MotivoOferta").Value =
                                        String.Format("Precio de venta ({0}) es menor al de la lista de precio predeterminada ({1}).",
                                        oDoc.Lines.UnitPrice, Convert.ToDouble(oRecordset.Fields.Item("Price").Value.ToString()));
                                }
                            }
                        }
                    }

                    oDoc.Lines.SetCurrentLine(i);
                    oDoc.Lines.Add();
                    i++;
                }
            }

            Respuesta = oDoc.Add();
            if (Respuesta != 0)
            {
                oCompany.GetLastError(out Respuesta, out MsgErrSBO);
                
                CrearRegistroLog(Respuesta.ToString(), MsgErrSBO, nro_comprobante);
                filas = 0;
                dbOracleUpdate.EjecutaSQL(ConsultasOracle.UpdatePedidoCab(nro_comprobante), ref filas);
                filas = 0;
                dbOracleUpdate.EjecutaSQL(ConsultasOracle.UpdatePedidoDet(nro_comprobante), ref filas);
                WriteErrorLog("CrearPedido:"+nroPedido+" Error: " + Respuesta +" " + MsgErrSBO);
            }
            else
            {
                identi = oCompany.GetNewObjectKey();
                dbOracleUpdate.EjecutaSQL(ConsultasOracle.UpdatePedidoCab(nro_comprobante), ref filas);
                filas = 0;
                dbOracleUpdate.EjecutaSQL(ConsultasOracle.UpdatePedidoDet(nro_comprobante), ref filas);

            }
        }
        
        public static void WriteErrorLog(string strErrorText)
        {
            try
            {
                string strFileName = "errorLog.txt";
                string strPath = System.Windows.Forms.Application.StartupPath;
                System.IO.File.AppendAllText(strPath + "\\" + strFileName, strErrorText + " - " + DateTime.Now.ToString() + "\r\n");
            }
            catch (Exception ex)
            {
                WriteErrorLog("Error in WriteErrorLog: " + ex.Message);
            }
        }
        
        private void CrearRegistroLog(string codError, string descError, string nroPed)
        {
            SAPbobsCOM.CompanyService oCompanyService = null;
            SAPbobsCOM.GeneralData oGeneralData = null;
            SAPbobsCOM.GeneralService oGeneralService = null;

            oCompanyService = oCompany.GetCompanyService();
            oGeneralService = oCompanyService.GetGeneralService("EXXLOGCASTPEDID");
            oGeneralData = ((SAPbobsCOM.GeneralData)(oGeneralService.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralData)));

            oGeneralData.SetProperty("U_EXX_CodError", codError);
            oGeneralData.SetProperty("U_EXX_DescError", descError);
            oGeneralData.SetProperty("U_EXX_NroPed", nroPed);
            oGeneralData.SetProperty("U_EXX_Fecha", String.Format("{0:G}", DateTime.Now.ToString()));

            SAPbobsCOM.GeneralDataParams oGeneralParams = null;
            oGeneralParams = oGeneralService.Add(oGeneralData);
        }
        
        #endregion


    }
}
