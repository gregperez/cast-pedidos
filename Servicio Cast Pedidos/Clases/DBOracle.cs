using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using Oracle.ManagedDataAccess.Client;

namespace Servicio_Cast_Pedidos.Clases
{
    /// <summary>
    /// Clase de Conexión y empleo de la DB Oracle.
    /// ODP.NET Oracle managed provider
    /// </summary>
    public class DBOracle : IDisposable
    {
        #region Atributos

        private OracleConnection oConnection;
        private OracleTransaction oTransaction;
        public OracleDataReader oDataReader;

        // Indica el numero de intentos de conectar a la BD sin exito.
        public byte intentos = 0;

        private struct stConnDB
        {
            public string CadenaConexion;
            public string ErrorDesc;
            public int ErrorNum;
        }

        private stConnDB info;

        #endregion


        #region "Propiedades"

        /// <summary>
        /// Devuelve la descripcion de error de la clase.
        /// </summary>
        public string ErrDesc
        {
            get { return this.info.ErrorDesc; }
        }

        /// <summary>
        /// Devuelve el numero de error de la clase.
        /// </summary>
        public string ErrNum
        {
            get { return info.ErrorNum.ToString(); }
        }

        #endregion


        #region Métodos

        /// <summary>
        /// Constructor.
        /// </summary>
        public DBOracle(string Servidor, string Usuario, string Password)
        {
            // Creamos la cadena de conexión de la base de datos.
            info.CadenaConexion = string.Format("Data Source={0};User Id={1};Password={2};", Servidor, Usuario, Password);

            // Instanciamos objeto conecction.
            oConnection = new OracleConnection();

        }

        /// <summary>
        /// Implement IDisposable.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose de la clase.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Liberamos objetos manejados.
            }

            try
            {
                // Liberamos los obtetos no manejados.
                if (oDataReader != null)
                {
                    oDataReader.Close();
                    oDataReader.Dispose();
                }

                // Cerramos la conexión a DB.
                if (!Desconectar())
                {
                    // Grabamos Log de Error...
                }

            }
            catch (Exception ex)
            {
                // Asignamos error.
                AsignarError(ref ex);
            }

        }

        /// <summary>
        /// Destructor.
        /// </summary>
        ~DBOracle()
        {
            Dispose(false);
        }

        /// <summary>
        /// Se conecta a una base de datos de Oracle.
        /// </summary>
        /// <returns>True si se conecta bien.</returns>
        private bool Conectar()
        {

            bool ok = false;

            try
            {
                if (oConnection != null)
                {
                    // Fijamos la cadena de conexión de la base de datos.
                    oConnection.ConnectionString = info.CadenaConexion;
                    oConnection.Open();
                    ok = true;
                }
            }
            catch (Exception ex)
            {
                // Desconectamos y liberamos memoria.
                Desconectar();
                // Asignamos error.
                AsignarError(ref ex);
                // Asignamos error de función
                ok = false;
            }

            return ok;

        }

        /// <summary>
        /// Cierra la conexión de BBDD.
        /// </summary>
        public bool Desconectar()
        {
            try
            {
                // Cerramos la conexion
                if (oConnection != null)
                {
                    if (oConnection.State != ConnectionState.Closed)
                    {
                        oConnection.Close();
                    }
                }
                // Liberamos su memoria.
                oConnection.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                AsignarError(ref ex);
                return false;
            }
        }

        /// <summary>
        /// Ejecuta un procedimiento almacenado de Oracle.
        /// </summary>
        /// <param name="oraCommand">Objeto Command con los datos del procedimiento.</param>
        /// <param name="SpName">Nombre del procedimiento almacenado.</param>
        /// <returns>True si el procedimiento se ejecuto bien.</returns>
        public bool EjecutaSP(ref OracleCommand OraCommand, string SpName)
        {

            bool ok = true;

            try
            {
                // Si no esta conectado, se conecta.
                if (!IsConected())
                {
                    ok = Conectar();
                }

                if (ok)
                {
                    OraCommand.Connection = oConnection;
                    OraCommand.CommandText = SpName;
                    OraCommand.CommandType = CommandType.StoredProcedure;
                    OraCommand.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                AsignarError(ref ex);
                ok = false;
            }

            return ok;

        }

        /// <summary>
        /// Ejecuta una sql que rellenar un DataReader (sentencia select).
        /// </summary>
        /// <param name="SqlQuery">sentencia sql a ejecutar</param>
        /// <returns></returns> 
        public bool EjecutaSQL(string SqlQuery)
        {

            bool ok = true;

            OracleCommand ora_Command = new OracleCommand();

            try
            {

                // Si no esta conectado, se conecta.
                if (!IsConected())
                {
                    ok = Conectar();
                }

                if (ok)
                {
                    // Cerramos cursores abiertos, para evitar el error ORA-1000
                    if ((oDataReader != null))
                    {
                        oDataReader.Close();
                        oDataReader.Dispose();
                    }

                    ora_Command.Connection = oConnection;
                    ora_Command.CommandType = CommandType.Text;
                    ora_Command.CommandText = SqlQuery;

                    // Ejecutamos sql.
                    oDataReader = ora_Command.ExecuteReader();
                }

            }
            catch (Exception ex)
            {
                AsignarError(ref ex);
                ok = false;
            }
            finally
            {
                if (ora_Command != null)
                {
                    ora_Command.Dispose();
                }
            }

            return ok;

        }

        /// <summary>
        /// Ejecuta una sql que no devuelve datos (update, delete, insert).
        /// </summary>
        /// <param name="SqlQuery">sentencia sql a ejecutar</param>
        /// <param name="FilasAfectadas">Fila afectadas por la sentencia SQL</param>
        /// <returns></returns>
        public bool EjecutaSQL(string SqlQuery, ref int FilasAfectadas)
        {

            bool ok = true;
            OracleCommand ora_Command = new OracleCommand();

            try
            {

                // Si no esta conectado, se conecta.
                if (!IsConected())
                {
                    ok = Conectar();
                }

                if (ok)
                {
                    oTransaction = oConnection.BeginTransaction();
                    ora_Command = oConnection.CreateCommand();
                    ora_Command.CommandType = CommandType.Text;
                    ora_Command.CommandText = SqlQuery;
                    FilasAfectadas = ora_Command.ExecuteNonQuery();
                    oTransaction.Commit();
                }

            }
            catch (Exception ex)
            {
                // Hacemos rollback.
                oTransaction.Rollback();
                AsignarError(ref ex);
                ok = false;
            }
            finally
            {
                // Recolectamos objetos para liberar su memoria.
                if (ora_Command != null)
                {
                    ora_Command.Dispose();
                }
            }

            return ok;

        }


        /// <summary>
        /// Captura Excepciones
        /// </summary>
        /// <param name="ex">Excepcion producida.</param>
        private void AsignarError(ref Exception ex)
        {
            // Si es una excepcion de Oracle.
            if (ex is OracleException)
            {
                info.ErrorNum = ((OracleException)ex).Number;
                info.ErrorDesc = ex.Message;
            }
            else
            {
                info.ErrorNum = 0;
                info.ErrorDesc = ex.Message;
            }
            // Grabamos Log de Error...
        }



        /// <summary>
        /// Devuelve el estado de la base de datos
        /// </summary>
        /// <returns>True si esta conectada.</returns>
        public bool IsConected()
        {

            bool ok = false;

            try
            {
                // Si el objeto conexion ha sido instanciado
                if (oConnection != null)
                {
                    // Segun el estado de la Base de Datos.
                    switch (oConnection.State)
                    {
                        case ConnectionState.Closed:
                        case ConnectionState.Broken:
                        case ConnectionState.Connecting:
                            ok = false;
                            break;
                        case ConnectionState.Open:
                        case ConnectionState.Fetching:
                        case ConnectionState.Executing:
                            ok = true;
                            break;
                    }
                }
                else
                {
                    ok = false;
                }

            }
            catch (Exception ex)
            {
                AsignarError(ref ex);
                ok = false;
            }

            return ok;

        }

        #endregion

    }
}
