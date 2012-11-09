using System.Collections;
using System.Data;

namespace SnipMan.Core
{
    /// <summary>
    /// Generic Data Provider
    /// </summary>
    /// <remarks>Can be used to create Oracle, Access, SQL Server, MySQL Interfaces.</remarks>
    public interface IDataProvider
    {
        /// <summary>
        /// Set connection string to database
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        void SetConnection(string connectionString);

        /// <summary>
        /// Create database parameter
        /// </summary>
        /// <param name="obj">Object to Convert</param>
        /// <param name="conn">Connection Object</param>
        /// <returns></returns>
        /// <remarks></remarks>
        object CreateParameter(object obj, object conn);

        object CreateParameterBind(string bindname, object obj, object conn);

        /// <summary>
        /// Execute a Prepared Statement
        /// </summary>
        /// <param name="sql">SQL Command</param>
        /// <param name="parameters">Parameters</param>
        /// <returns></returns>
        /// <remarks></remarks>
        DataTable PreparedQuery(string sql, params object[] parameters);

        /// <summary>
        /// Execute a Prepared Statement (Non-Returning)
        /// </summary>
        /// <param name="sql">SQL Command</param>
        /// <param name="parameters">Parameters</param>
        /// <returns></returns>
        /// <remarks></remarks>
        void PreparedNonQuery(string sql, params object[] parameters);

        /// <summary>
        /// Execute a Prepared Statement with Parameter Binding
        /// </summary>
        /// <param name="sql">SQL Command</param>
        /// <param name="parameters">Parameters as HashTable Key = Bind To, Value = Object</param>
        /// <returns></returns>
        /// <remarks></remarks>
        DataTable PreparedQueryBind(string sql, Hashtable parameters);

        /// <summary>
        /// Execute a Prepared Statement with BEGIN and END, returns only one value.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        object PreparedStatement(string sql, string retVal, object retType, params object[] parameters);

        /// <summary>
        /// Execute Scalar Query
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        object PreparedScalar(string sql, params object[] parameters);

        /// <summary>
        /// Execute a Stored Function
        /// </summary>
        /// <param name="funcName">Function Name</param>
        /// <param name="retType">Return Type</param>
        /// <param name="oracleParameters">Parameters</param>
        /// <returns></returns>
        /// <remarks></remarks>
        object StoredFunction(string funcName, object retType, params object[] oracleParameters);

        /// <summary>
        /// Execute a Stored Procedure
        /// </summary>
        /// <param name="procedureName">Procedure Location</param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        DataTable StoredProcedure(string procedureName, params object[] parameters);

        /// <summary>
        /// Execute a Query that Modifies Data (Insert, Update)
        /// </summary>
        /// <param name="sql">SQL Command</param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        int UpdateQuery(string sql, params object[] parameters);

        /// <summary>
        /// Gets the Connection String from the Specified INI
        /// </summary>
        /// <param name="path">Path to INI</param>
        /// <param name="filename">INI Filename</param>
        /// <returns></returns>
        /// <remarks></remarks>
        string GetConnString(string path, string filename);

        bool ShrinkDatabase();
    }
}
