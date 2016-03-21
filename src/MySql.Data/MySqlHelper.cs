// Copyright ?2004, 2014, Oracle and/or its affiliates. All rights reserved.
//
// MySQL Connector/NET is licensed under the terms of the GPLv2
// <http://www.gnu.org/licenses/old-licenses/gpl-2.0.html>, like most 
// MySQL Connectors. There are special exceptions to the terms and 
// conditions of the GPLv2 as it is applied to this software, see the 
// FLOSS License Exception
// <http://www.mysql.com/about/legal/licensing/foss-exception.html>.
//
// This program is free software; you can redistribute it and/or modify 
// it under the terms of the GNU General Public License as published 
// by the Free Software Foundation; version 2 of the License.
//
// This program is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
//
// You should have received a copy of the GNU General Public License along 
// with this program; if not, write to the Free Software Foundation, Inc., 
// 51 Franklin St, Fifth Floor, Boston, MA 02110-1301  USA

using System.Data;
using MySql.Data.MySqlClient;
using System.Text;
#if NET_40_OR_GREATER
using System.Threading.Tasks;
using System.Threading;
using System;
#endif

namespace MySql.Data.MySqlClient
{
  /// <summary>
  /// Helper class that makes it easier to work with the provider.
  /// </summary>
  public sealed class MySqlHelper
  {
    enum CharClass : byte
    {
      None,
      Quote,
      Backslash
    }

    private static string stringOfBackslashChars = "\u005c\u00a5\u0160\u20a9\u2216\ufe68\uff3c";
    private static string stringOfQuoteChars =
        "\u0022\u0027\u0060\u00b4\u02b9\u02ba\u02bb\u02bc\u02c8\u02ca\u02cb\u02d9\u0300\u0301\u2018\u2019\u201a\u2032\u2035\u275b\u275c\uff07";

    private static CharClass[] charClassArray = makeCharClassArray();

    // this class provides only static methods
    private MySqlHelper()
    {
    }

    #region ExecuteNonQuery

    /// <summary>
    /// Executes a single command against a MySQL database.  The <see cref="MySqlConnection"/> is assumed to be
    /// open when the method is called and remains open after the method completes.
    /// </summary>
    /// <param name="connection"><see cref="MySqlConnection"/> object to use</param>
    /// <param name="commandText">SQL command to be executed</param>
    /// <param name="commandParameters">Array of <see cref="MySqlParameter"/> objects to use with the command.</param>
    /// <returns></returns>
    public static int ExecuteNonQuery(MySqlConnection connection, string commandText, params MySqlParameter[] commandParameters)
    {
      //create a command and prepare it for execution
      MySqlCommand cmd = new MySqlCommand();
      cmd.Connection = connection;
      cmd.CommandText = commandText;
      cmd.CommandType = CommandType.Text;

      if (commandParameters != null)
        foreach (MySqlParameter p in commandParameters)
          cmd.Parameters.Add(p);

      int result = cmd.ExecuteNonQuery();
      cmd.Parameters.Clear();

      return result;
    }

    /// <summary>
    /// Executes a single command against a MySQL database.  A new <see cref="MySqlConnection"/> is created
    /// using the <see cref="MySqlConnection.ConnectionString"/> given.
    /// </summary>
    /// <param name="connectionString"><see cref="MySqlConnection.ConnectionString"/> to use</param>
    /// <param name="commandText">SQL command to be executed</param>
    /// <param name="parms">Array of <see cref="MySqlParameter"/> objects to use with the command.</param>
    /// <returns></returns>
    public static int ExecuteNonQuery(string connectionString, string commandText, params MySqlParameter[] parms)
    {
      //create & open a SqlConnection, and dispose of it after we are done.
      using (MySqlConnection cn = new MySqlConnection(connectionString))
      {
        cn.Open();

        //call the overload that takes a connection in place of the connection string
        return ExecuteNonQuery(cn, commandText, parms);
      }
    }
    #endregion

 

    #region ExecuteDataReader

    /// <summary>
    /// Executes a single command against a MySQL database, possibly inside an existing transaction.
    /// </summary>
    /// <param name="connection"><see cref="MySqlConnection"/> object to use for the command</param>
    /// <param name="transaction"><see cref="MySqlTransaction"/> object to use for the command</param>
    /// <param name="commandText">Command text to use</param>
    /// <param name="commandParameters">Array of <see cref="MySqlParameter"/> objects to use with the command</param>
    /// <param name="ExternalConn">True if the connection should be preserved, false if not</param>
    /// <returns><see cref="MySqlDataReader"/> object ready to read the results of the command</returns>
    private static MySqlDataReader ExecuteReader(MySqlConnection connection, MySqlTransaction transaction, string commandText, MySqlParameter[] commandParameters, bool ExternalConn)
    {
      //create a command and prepare it for execution
      MySqlCommand cmd = new MySqlCommand();
      cmd.Connection = connection;
      cmd.Transaction = transaction;
      cmd.CommandText = commandText;
      cmd.CommandType = CommandType.Text;

      if (commandParameters != null)
        foreach (MySqlParameter p in commandParameters)
          cmd.Parameters.Add(p);

      //create a reader
      MySqlDataReader dr;

      // call ExecuteReader with the appropriate CommandBehavior
      if (ExternalConn)
      {
        dr = cmd.ExecuteReader();
      }
      else
      {
        dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
      }

      // detach the SqlParameters from the command object, so they can be used again.
      cmd.Parameters.Clear();

      return dr;
    }

    /// <summary>
    /// Executes a single command against a MySQL database.
    /// </summary>
    /// <param name="connectionString">Settings to use for this command</param>
    /// <param name="commandText">Command text to use</param>
    /// <returns><see cref="MySqlDataReader"/> object ready to read the results of the command</returns>
    public static MySqlDataReader ExecuteReader(string connectionString, string commandText)
    {
      //pass through the call providing null for the set of SqlParameters
      return ExecuteReader(connectionString, commandText, (MySqlParameter[])null);
    }

    /// <summary>
    /// Executes a single command against a MySQL database.
    /// </summary>
    /// <param name="connection"><see cref="MySqlConnection"/> object to use for the command</param>
    /// <param name="commandText">Command text to use</param>
    /// <returns><see cref="MySqlDataReader"/> object ready to read the results of the command</returns>
    public static MySqlDataReader ExecuteReader(MySqlConnection connection, string commandText)
    {
      //pass through the call providing null for the set of SqlParameters
      return ExecuteReader(connection, null, commandText, (MySqlParameter[])null, true);
    }

    /// <summary>
    /// Executes a single command against a MySQL database.
    /// </summary>
    /// <param name="connectionString">Settings to use for this command</param>
    /// <param name="commandText">Command text to use</param>
    /// <param name="commandParameters">Array of <see cref="MySqlParameter"/> objects to use with the command</param>
    /// <returns><see cref="MySqlDataReader"/> object ready to read the results of the command</returns>
    public static MySqlDataReader ExecuteReader(string connectionString, string commandText, params MySqlParameter[] commandParameters)
    {
      //create & open a SqlConnection
      MySqlConnection cn = new MySqlConnection(connectionString);
      cn.Open();

      //call the private overload that takes an internally owned connection in place of the connection string
      return ExecuteReader(cn, null, commandText, commandParameters, false);
    }

    /// <summary>
    /// Executes a single command against a MySQL database.
    /// </summary>
    /// <param name="connection">Connection to use for the command</param>
    /// <param name="commandText">Command text to use</param>
    /// <param name="commandParameters">Array of <see cref="MySqlParameter"/> objects to use with the command</param>
    /// <returns><see cref="MySqlDataReader"/> object ready to read the results of the command</returns>
    public static MySqlDataReader ExecuteReader(MySqlConnection connection, string commandText, params MySqlParameter[] commandParameters)
    {
      //call the private overload that takes an internally owned connection in place of the connection string
      return ExecuteReader(connection, null, commandText, commandParameters, true);
    }


    #endregion

    #region ExecuteScalar

    /// <summary>
    /// Execute a single command against a MySQL database.
    /// </summary>
    /// <param name="connectionString">Settings to use for the update</param>
    /// <param name="commandText">Command text to use for the update</param>
    /// <returns>The first column of the first row in the result set, or a null reference if the result set is empty.</returns>
    public static object ExecuteScalar(string connectionString, string commandText)
    {
      //pass through the call providing null for the set of MySqlParameters
      return ExecuteScalar(connectionString, commandText, (MySqlParameter[])null);
    }

    /// <summary>
    /// Execute a single command against a MySQL database.
    /// </summary>
    /// <param name="connectionString">Settings to use for the command</param>
    /// <param name="commandText">Command text to use for the command</param>
    /// <param name="commandParameters">Parameters to use for the command</param>
    /// <returns>The first column of the first row in the result set, or a null reference if the result set is empty.</returns>
    public static object ExecuteScalar(string connectionString, string commandText, params MySqlParameter[] commandParameters)
    {
      //create & open a SqlConnection, and dispose of it after we are done.
      using (MySqlConnection cn = new MySqlConnection(connectionString))
      {
        cn.Open();

        //call the overload that takes a connection in place of the connection string
        return ExecuteScalar(cn, commandText, commandParameters);
      }
    }

    /// <summary>
    /// Execute a single command against a MySQL database.
    /// </summary>
    /// <param name="connection"><see cref="MySqlConnection"/> object to use</param>
    /// <param name="commandText">Command text to use for the command</param>
    /// <returns>The first column of the first row in the result set, or a null reference if the result set is empty.</returns>
    public static object ExecuteScalar(MySqlConnection connection, string commandText)
    {
      //pass through the call providing null for the set of MySqlParameters
      return ExecuteScalar(connection, commandText, (MySqlParameter[])null);
    }

    /// <summary>
    /// Execute a single command against a MySQL database.
    /// </summary>
    /// <param name="connection"><see cref="MySqlConnection"/> object to use</param>
    /// <param name="commandText">Command text to use for the command</param>
    /// <param name="commandParameters">Parameters to use for the command</param>
    /// <returns>The first column of the first row in the result set, or a null reference if the result set is empty.</returns>
    public static object ExecuteScalar(MySqlConnection connection, string commandText, params MySqlParameter[] commandParameters)
    {
      //create a command and prepare it for execution
      MySqlCommand cmd = new MySqlCommand();
      cmd.Connection = connection;
      cmd.CommandText = commandText;
      cmd.CommandType = CommandType.Text;

      if (commandParameters != null)
        foreach (MySqlParameter p in commandParameters)
          cmd.Parameters.Add(p);

      //execute the command & return the results
      object retval = cmd.ExecuteScalar();

      // detach the SqlParameters from the command object, so they can be used again.
      cmd.Parameters.Clear();
      return retval;

    }

    #endregion

    #region Utility methods
    private static CharClass[] makeCharClassArray()
    {

      CharClass[] a = new CharClass[65536];
      foreach (char c in stringOfBackslashChars)
      {
        a[c] = CharClass.Backslash;
      }
      foreach (char c in stringOfQuoteChars)
      {
        a[c] = CharClass.Quote;
      }
      return a;
    }

    private static bool needsQuoting(string s)
    {
      foreach (char c in s)
      {
        if (charClassArray[c] != CharClass.None)
        {
          return true;
        }
      }
      return false;
    }

    /// <summary>
    /// Escapes the string.
    /// </summary>
    /// <param name="value">The string to escape</param>
    /// <returns>The string with all quotes escaped.</returns>
    public static string EscapeString(string value)
    {
      if (!needsQuoting(value))
        return value;

      StringBuilder sb = new StringBuilder();

      foreach (char c in value)
      {
        CharClass charClass = charClassArray[c];
        if (charClass != CharClass.None)
        {
          sb.Append("\\");
        }
        sb.Append(c);
      }
      return sb.ToString();
    }

    public static string DoubleQuoteString(string value)
    {
      if (!needsQuoting(value))
        return value;

      StringBuilder sb = new StringBuilder();
      foreach (char c in value)
      {
        CharClass charClass = charClassArray[c];
        if (charClass == CharClass.Quote)
          sb.Append(c);
        else if (charClass == CharClass.Backslash)
          sb.Append("\\");
        sb.Append(c);
      }
      return sb.ToString();
    }

    #endregion

#if NET_40_OR_GREATER
    #region Async

    #region DataRow
    /// <summary>
    /// Async version of ExecuteDataRow
    /// </summary>
    /// <param name="connectionString">Settings to be used for the connection</param>
    /// <param name="commandText">Command to execute</param>
    /// <param name="parms">Parameters to use for the command</param>
    /// <returns>DataRow containing the first row of the resultset</returns>
    public static Task<DataRow> ExecuteDataRowAsync(string connectionString, string commandText, params MySqlParameter[] parms)
    {
      return ExecuteDataRowAsync(connectionString, commandText, CancellationToken.None, parms);
    }

    public static Task<DataRow> ExecuteDataRowAsync(string connectionString, string commandText, CancellationToken cancellationToken, params MySqlParameter[] parms)
    {
      var result = new TaskCompletionSource<DataRow>();
      if (cancellationToken == CancellationToken.None || !cancellationToken.IsCancellationRequested)
      {
        try
        {
          var row = ExecuteDataRow(connectionString, commandText, parms);
          result.SetResult(row);
        }
        catch (Exception ex)
        {
          result.SetException(ex);
        }
      }
      else
      {
        result.SetCanceled();
      }
      return result.Task;
    }

    #endregion

    #region NonQuery
    /// <summary>
    /// Async version of ExecuteNonQuery
    /// </summary>
    /// <param name="connection"><see cref="MySqlConnection"/> object to use</param>
    /// <param name="commandText">SQL command to be executed</param>
    /// <param name="commandParameters">Array of <see cref="MySqlParameter"/> objects to use with the command.</param>
    /// <returns>Rows affected</returns>
    public static Task<int> ExecuteNonQueryAsync(MySqlConnection connection, string commandText, params MySqlParameter[] commandParameters)
    {
      return ExecuteNonQueryAsync(connection, commandText, CancellationToken.None, commandParameters);
    }

    public static Task<int> ExecuteNonQueryAsync(MySqlConnection connection, string commandText, CancellationToken cancellationToken, params MySqlParameter[] commandParameters)
    {
      var result = new TaskCompletionSource<int>();
      if (cancellationToken == CancellationToken.None || !cancellationToken.IsCancellationRequested)
      {
        try
        {
          var queryResult = ExecuteNonQuery(connection, commandText, commandParameters);
          result.SetResult(queryResult);
        }
        catch (Exception ex)
        {
          result.SetException(ex);
        }
      }
      else
      {
        result.SetCanceled();
      }
      return result.Task;
    }

    /// <summary>
    /// Async version of ExecuteNonQuery
    /// </summary>
    /// <param name="connectionString"><see cref="MySqlConnection.ConnectionString"/> to use</param>
    /// <param name="commandText">SQL command to be executed</param>
    /// <param name="commandParameters">Array of <see cref="MySqlParameter"/> objects to use with the command.</param>
    /// <returns>Rows affected</returns>
    public static Task<int> ExecuteNonQueryAsync(string connectionString, string commandText, params MySqlParameter[] commandParameters)
    {
      return ExecuteNonQueryAsync(connectionString, commandText, CancellationToken.None, commandParameters);
    }

    public static Task<int> ExecuteNonQueryAsync(string connectionString, string commandText, CancellationToken cancellationToken, params MySqlParameter[] commandParameters)
    {
      var result = new TaskCompletionSource<int>();
      if (cancellationToken == CancellationToken.None || !cancellationToken.IsCancellationRequested)
      {
        try
        {
          var queryResult = ExecuteNonQuery(connectionString, commandText, commandParameters);
          result.SetResult(queryResult);
        }
        catch (Exception ex)
        {
          result.SetException(ex);
        }
      }
      else
      {
        result.SetCanceled();
      }
      return result.Task;
    }

    #endregion

    #region DataSet
    /// <summary>
    /// Async version of ExecuteDataset
    /// </summary>
    /// <param name="connectionString">Settings to be used for the connection</param>
    /// <param name="commandText">Command to execute</param>
    /// <returns><see cref="DataSet"/> containing the resultset</returns>
    public static Task<DataSet> ExecuteDatasetAsync(string connectionString, string commandText)
    {
      return ExecuteDatasetAsync(connectionString, commandText, CancellationToken.None, (MySqlParameter[])null);
    }

    public static Task<DataSet> ExecuteDatasetAsync(string connectionString, string commandText, CancellationToken cancellationToken)
    {
      return ExecuteDatasetAsync(connectionString, commandText, cancellationToken, (MySqlParameter[])null);
    }

    /// <summary>
    /// Async version of ExecuteDataset
    /// </summary>
    /// <param name="connectionString">Settings to be used for the connection</param>
    /// <param name="commandText">Command to execute</param>
    /// <param name="commandParameters">Parameters to use for the command</param>
    /// <returns><see cref="DataSet"/> containing the resultset</returns>
    public static Task<DataSet> ExecuteDatasetAsync(string connectionString, string commandText, params MySqlParameter[] commandParameters)
    {
      return ExecuteDatasetAsync(connectionString, commandText, CancellationToken.None, commandParameters);
    }

    public static Task<DataSet> ExecuteDatasetAsync(string connectionString, string commandText, CancellationToken cancellationToken, params MySqlParameter[] commandParameters)
    {
      var result = new TaskCompletionSource<DataSet>();
      if (cancellationToken == CancellationToken.None || !cancellationToken.IsCancellationRequested)
      {
        try
        {
          var dataset = ExecuteDataset(connectionString, commandText, commandParameters);
          result.SetResult(dataset);
        }
        catch (Exception ex)
        {
          result.SetException(ex);
        }
      }
      else
      {
        result.SetCanceled();
      }
      return result.Task;
    }

    /// <summary>
    /// Async version of ExecuteDataset
    /// </summary>
    /// <param name="connection"><see cref="MySqlConnection"/> object to use</param>
    /// <param name="commandText">Command to execute</param>
    /// <returns><see cref="DataSet"/> containing the resultset</returns>
    public static Task<DataSet> ExecuteDatasetAsync(MySqlConnection connection, string commandText)
    {
      return ExecuteDatasetAsync(connection, commandText, CancellationToken.None, (MySqlParameter[])null);
    }

    public static Task<DataSet> ExecuteDatasetAsync(MySqlConnection connection, string commandText, CancellationToken cancellationToken)
    {
      return ExecuteDatasetAsync(connection, commandText, cancellationToken, (MySqlParameter[])null);
    }

    /// <summary>
    /// Async version of ExecuteDataset
    /// </summary>
    /// <param name="connection"><see cref="MySqlConnection"/> object to use</param>
    /// <param name="commandText">Command to execute</param>
    /// <param name="commandParameters">Parameters to use for the command</param>
    /// <returns><see cref="DataSet"/> containing the resultset</returns>
    public static Task<DataSet> ExecuteDatasetAsync(MySqlConnection connection, string commandText, params MySqlParameter[] commandParameters)
    {
      return ExecuteDatasetAsync(connection, commandText, CancellationToken.None, commandParameters);
    }

    public static Task<DataSet> ExecuteDatasetAsync(MySqlConnection connection, string commandText, CancellationToken cancellationToken, params MySqlParameter[] commandParameters)
    {
      var result = new TaskCompletionSource<DataSet>();
      if (cancellationToken == CancellationToken.None || !cancellationToken.IsCancellationRequested)
      {
        try
        {
          var dataset = ExecuteDataset(connection, commandText, commandParameters);
          result.SetResult(dataset);
        }
        catch (Exception ex)
        {
          result.SetException(ex);
        }
      }
      else
      {
        result.SetCanceled();
      }
      return result.Task;
    }

    /// <summary>
    /// Async version of UpdateDataset
    /// </summary>
    /// <param name="connectionString">Settings to use for the update</param>
    /// <param name="commandText">Command text to use for the update</param>
    /// <param name="ds"><see cref="DataSet"/> containing the new data to use in the update</param>
    /// <param name="tablename">Tablename in the dataset to update</param>
    public static Task UpdateDataSetAsync(string connectionString, string commandText, DataSet ds, string tablename)
    {
      return UpdateDataSetAsync(connectionString, commandText, ds, tablename, CancellationToken.None);
    }

    public static Task UpdateDataSetAsync(string connectionString, string commandText, DataSet ds, string tablename, CancellationToken cancellationToken)
    {
      var result = new TaskCompletionSource<bool>();
      if (cancellationToken == CancellationToken.None || !cancellationToken.IsCancellationRequested)
      {
        try
        {
          UpdateDataSet(connectionString, commandText, ds, tablename);
          result.SetResult(true);
        }
        catch (Exception ex)
        {
          result.SetException(ex);
        }
      }
      else
      {
        result.SetCanceled();
      }
      return result.Task;
    }

    #endregion

    #region DataReader
    /// <summary>
    /// Async version of ExecuteReader
    /// </summary>
    /// <param name="connection"><see cref="MySqlConnection"/> object to use for the command</param>
    /// <param name="transaction"><see cref="MySqlTransaction"/> object to use for the command</param>
    /// <param name="commandText">Command text to use</param>
    /// <param name="commandParameters">Array of <see cref="MySqlParameter"/> objects to use with the command</param>
    /// <param name="ExternalConn">True if the connection should be preserved, false if not</param>
    /// <returns><see cref="MySqlDataReader"/> object ready to read the results of the command</returns>
    private static Task<MySqlDataReader> ExecuteReaderAsync(MySqlConnection connection, MySqlTransaction transaction, string commandText, MySqlParameter[] commandParameters, bool ExternalConn)
    {
      return ExecuteReaderAsync(connection, transaction, commandText, commandParameters, ExternalConn, CancellationToken.None);
    }

    private static Task<MySqlDataReader> ExecuteReaderAsync(MySqlConnection connection, MySqlTransaction transaction, string commandText, MySqlParameter[] commandParameters, bool ExternalConn, CancellationToken cancellationToken)
    {
      var result = new TaskCompletionSource<MySqlDataReader>();
      if (cancellationToken == CancellationToken.None || !cancellationToken.IsCancellationRequested)
      {
        try
        {
          var reader = ExecuteReader(connection, transaction, commandText, commandParameters, ExternalConn);
          result.SetResult(reader);
        }
        catch (Exception ex)
        {
          result.SetException(ex);
        }
      }
      else
      {
        result.SetCanceled();
      }
      return result.Task;
    }

    /// <summary>
    /// Async version of ExecuteReader
    /// </summary>
    /// <param name="connectionString">Settings to use for this command</param>
    /// <param name="commandText">Command text to use</param>
    /// <returns><see cref="MySqlDataReader"/> object ready to read the results of the command</returns>
    public static Task<MySqlDataReader> ExecuteReaderAsync(string connectionString, string commandText)
    {
      return ExecuteReaderAsync(connectionString, commandText, CancellationToken.None, (MySqlParameter[])null);
    }

    public static Task<MySqlDataReader> ExecuteReaderAsync(string connectionString, string commandText, CancellationToken cancellationToken)
    {
      return ExecuteReaderAsync(connectionString, commandText, cancellationToken, (MySqlParameter[])null);
    }

    /// <summary>
    /// Async version of ExecuteReader
    /// </summary>
    /// <param name="connection"><see cref="MySqlConnection"/> object to use for the command</param>
    /// <param name="commandText">Command text to use</param>
    /// <returns><see cref="MySqlDataReader"/> object ready to read the results of the command</returns>
    public static Task<MySqlDataReader> ExecuteReaderAsync(MySqlConnection connection, string commandText)
    {
      return ExecuteReaderAsync(connection, null, commandText, (MySqlParameter[])null, true, CancellationToken.None);
    }

    public static Task<MySqlDataReader> ExecuteReaderAsync(MySqlConnection connection, string commandText, CancellationToken cancellationToken)
    {
      return ExecuteReaderAsync(connection, null, commandText, (MySqlParameter[])null, true, cancellationToken);
    }

    /// <summary>
    /// Async version of ExecuteReader
    /// </summary>
    /// <param name="connectionString">Settings to use for this command</param>
    /// <param name="commandText">Command text to use</param>
    /// <param name="commandParameters">Array of <see cref="MySqlParameter"/> objects to use with the command</param>
    /// <returns><see cref="MySqlDataReader"/> object ready to read the results of the command</returns>
    public static Task<MySqlDataReader> ExecuteReaderAsync(string connectionString, string commandText, params MySqlParameter[] commandParameters)
    {
      return ExecuteReaderAsync(connectionString, commandText, CancellationToken.None, commandParameters);
    }

    public static Task<MySqlDataReader> ExecuteReaderAsync(string connectionString, string commandText, CancellationToken cancellationToken, params MySqlParameter[] commandParameters)
    {
      var result = new TaskCompletionSource<MySqlDataReader>();
      if (cancellationToken == CancellationToken.None || !cancellationToken.IsCancellationRequested)
      {
        try
        {
          var reader = ExecuteReader(connectionString, commandText, commandParameters);
          result.SetResult(reader);
        }
        catch (Exception ex)
        {
          result.SetException(ex);
        }
      }
      else
      {
        result.SetCanceled();
      }
      return result.Task;
    }

    /// <summary>
    /// Async version of ExecuteReader
    /// </summary>
    /// <param name="connection">Connection to use for the command</param>
    /// <param name="commandText">Command text to use</param>
    /// <param name="commandParameters">Array of <see cref="MySqlParameter"/> objects to use with the command</param>
    /// <returns><see cref="MySqlDataReader"/> object ready to read the results of the command</returns>
    public static Task<MySqlDataReader> ExecuteReaderAsync(MySqlConnection connection, string commandText, params MySqlParameter[] commandParameters)
    {
      return ExecuteReaderAsync(connection, null, commandText, commandParameters, true, CancellationToken.None);
    }

    public static Task<MySqlDataReader> ExecuteReaderAsync(MySqlConnection connection, string commandText, CancellationToken cancellationToken, params MySqlParameter[] commandParameters)
    {
      return ExecuteReaderAsync(connection, null, commandText, commandParameters, true, cancellationToken);
    }

    #endregion

    #region Scalar
    /// <summary>
    /// Async version of ExecuteScalar
    /// </summary>
    /// <param name="connectionString">Settings to use for the update</param>
    /// <param name="commandText">Command text to use for the update</param>
    /// <returns>The first column of the first row in the result set, or a null reference if the result set is empty.</returns>
    public static Task<object> ExecuteScalarAsync(string connectionString, string commandText)
    {
      return ExecuteScalarAsync(connectionString, commandText, CancellationToken.None, (MySqlParameter[])null);
    }

    public static Task<object> ExecuteScalarAsync(string connectionString, string commandText, CancellationToken cancellationToken)
    {
      return ExecuteScalarAsync(connectionString, commandText, cancellationToken, (MySqlParameter[])null);
    }

    /// <summary>
    /// Async version of ExecuteScalar
    /// </summary>
    /// <param name="connectionString">Settings to use for the command</param>
    /// <param name="commandText">Command text to use for the command</param>
    /// <param name="commandParameters">Parameters to use for the command</param>
    /// <returns>The first column of the first row in the result set, or a null reference if the result set is empty.</returns>
    public static Task<object> ExecuteScalarAsync(string connectionString, string commandText, params MySqlParameter[] commandParameters)
    {
      return ExecuteScalarAsync(connectionString, commandText, CancellationToken.None, commandParameters);
    }

    public static Task<object> ExecuteScalarAsync(string connectionString, string commandText, CancellationToken cancellationToken, params MySqlParameter[] commandParameters)
    {
      var result = new TaskCompletionSource<object>();
      if (cancellationToken == CancellationToken.None || !cancellationToken.IsCancellationRequested)
      {
        try
        {
          var scalarResult = ExecuteScalar(connectionString, commandText, commandParameters);
          result.SetResult(scalarResult);
        }
        catch (Exception ex)
        {
          result.SetException(ex);
        }
      }
      else
      {
        result.SetCanceled();
      }
      return result.Task;
    }

    /// <summary>
    /// Async version of ExecuteScalar
    /// </summary>
    /// <param name="connection"><see cref="MySqlConnection"/> object to use</param>
    /// <param name="commandText">Command text to use for the command</param>
    /// <returns>The first column of the first row in the result set, or a null reference if the result set is empty.</returns>
    public static Task<object> ExecuteScalarAsync(MySqlConnection connection, string commandText)
    {
      return ExecuteScalarAsync(connection, commandText, CancellationToken.None, (MySqlParameter[])null);
    }

    public static Task<object> ExecuteScalarAsync(MySqlConnection connection, string commandText, CancellationToken cancellationToken)
    {
      return ExecuteScalarAsync(connection, commandText, cancellationToken, (MySqlParameter[])null);
    }

    /// <summary>
    /// Async version of ExecuteScalar
    /// </summary>
    /// <param name="connection"><see cref="MySqlConnection"/> object to use</param>
    /// <param name="commandText">Command text to use for the command</param>
    /// <param name="commandParameters">Parameters to use for the command</param>
    /// <returns>The first column of the first row in the result set, or a null reference if the result set is empty.</returns>
    public static Task<object> ExecuteScalarAsync(MySqlConnection connection, string commandText, params MySqlParameter[] commandParameters)
    {
      return ExecuteScalarAsync(connection, commandText, CancellationToken.None, commandParameters);
    }

    public static Task<object> ExecuteScalarAsync(MySqlConnection connection, string commandText, CancellationToken cancellationToken, params MySqlParameter[] commandParameters)
    {
      var result = new TaskCompletionSource<object>();
      if (cancellationToken == CancellationToken.None || !cancellationToken.IsCancellationRequested)
      {
        try
        {
          var scalarResult = ExecuteScalar(connection, commandText, commandParameters);
          result.SetResult(scalarResult);
        }
        catch (Exception ex)
        {
          result.SetException(ex);
        }
      }
      else
      {
        result.SetCanceled();
      }
      return result.Task;
    }
    #endregion
    #endregion
#endif
  }
}
