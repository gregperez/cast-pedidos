using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Servicio_Cast_Pedidos.Clases
{
    /// <summary>
    /// Clase de Conexión y empleo de la DB Sap.
    /// SAPBusinessOneSDK
    /// </summary>
    public class DBSap
    {
        #region Atributos

        private int intError;
        private string strError;
        public SAPbobsCOM.Company oCompany = null;

        #endregion

        #region Propiedades

        /// <summary>
        /// Propiedad que devuelve valor entero del error en caso que haya 
        /// fallado la conexión.
        /// </summary>
        public int iError
        {
            get { return intError; }
        }

        /// <summary>
        /// Propiedad que devuelve valor string del error en caso que haya 
        /// fallado la conexión.
        /// </summary>
        public string sError
        {
            get { return strError; }
        }

        #endregion

        #region Métodos

        /// <summary>
        /// Constructor de la clase.
        /// </summary>
        public DBSap()
        { }

        /// <summary>
        /// Función que estable conexión con la BD de SAP. 
        /// </summary>
        /// <returns>Retorna un objeto de tipo SAPbobsCOM.Company si la conexión 
        /// fue realizada de manera exitosa en caso contrario retornara null.</returns>
        public bool Conectar()
        {
            SAPbobsCOM.BoDataServerTypes BDTipo;
            string BDtype = string.Empty;
            bool ok = false;

            try
            {
                oCompany = new SAPbobsCOM.Company();
                BDTipo = SAPbobsCOM.BoDataServerTypes.dst_MSSQL2005;
                BDtype = ConfigurationManager.AppSettings["DataServerType"];
                switch (BDtype)
                {
                    case ("SQL_2005"):
                        BDTipo = SAPbobsCOM.BoDataServerTypes.dst_MSSQL2005;
                        break;
                    case ("SQL_2008"):
                        BDTipo = SAPbobsCOM.BoDataServerTypes.dst_MSSQL2008;
                        break;
                    case ("SQL_2012"):
                        BDTipo = SAPbobsCOM.BoDataServerTypes.dst_MSSQL2012;
                        break;
                    case ("SQL_2014"):
                        BDTipo = SAPbobsCOM.BoDataServerTypes.dst_MSSQL2014;
                        break;
                    case ("HANA"):
                        BDTipo = SAPbobsCOM.BoDataServerTypes.dst_HANADB;
                        break;
                }
                oCompany.Server = ConfigurationManager.AppSettings["ServerSAP"];
                oCompany.CompanyDB = ConfigurationManager.AppSettings["BD"];
                oCompany.DbUserName = ConfigurationManager.AppSettings["HanaUser"];
                oCompany.DbPassword = ConfigurationManager.AppSettings["HanaPwd"];
                if (!ConfigurationManager.AppSettings["LicenseServer"].Equals(""))
                {
                    oCompany.LicenseServer = ConfigurationManager.AppSettings["LicenseServer"];
                }
                oCompany.UserName = ConfigurationManager.AppSettings["SapUser"];
                oCompany.Password = ConfigurationManager.AppSettings["SapPwd"];
                oCompany.DbServerType = BDTipo;
                oCompany.language = SAPbobsCOM.BoSuppLangs.ln_Spanish_La;

                if (oCompany != null)
                {
                    oCompany.UseTrusted = false;
                    int Con = oCompany.Connect();
                    if (Con == 0)
                        ok = true;
                    else
                        oCompany.GetLastError(out intError, out strError);
                }
            }
            catch (Exception ex)
            {
                ok = false;
                //System.Diagnostics.EventLog.WriteEntry("Application", "Ocurrió el siguiente error: " + ex.Message);
            }
            return ok;
        }

        public void LiberarObjeto(Object oObject)
        {
            try
            {
                if (oObject != null)
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(oObject);

                oObject = null;
                GC.Collect();
            }
            catch (Exception)
            {
                oObject = null;
                GC.Collect();
            }
        }

        #endregion
    }
}
