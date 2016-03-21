// Copyright ?2004, 2011, Oracle and/or its affiliates. All rights reserved.
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

using System;
using System.Collections;
using System.IO;
using System.Text;
using MySql.Data.Common;
using System.Data;

using System.Collections.Generic;

namespace MySql.Data.MySqlClient
{
  internal abstract class Statement
  {
    protected MySqlCommand command;
    protected string commandText;
    private List<MySqlPacket> buffers;

    private Statement(MySqlCommand cmd)
    {
      command = cmd;
      buffers = new List<MySqlPacket>();
    }

    public Statement(MySqlCommand cmd, string text)
      : this(cmd)
    {
      commandText = text;
    }

    #region Properties

    public virtual string ResolvedCommandText
    {
      get { return commandText; }
    }

    protected Driver Driver
    {
      get { return command.Connection.driver; }
    }

    protected MySqlConnection Connection
    {
      get { return command.Connection; }
    }

    protected MySqlParameterCollection Parameters
    {
      get { return command.Parameters; }
    }

    #endregion

    public virtual void Close(MySqlDataReader reader)
    {
    }

    public virtual void Resolve(bool preparing)
    {
    }

    public virtual void Execute()
    {
      // we keep a reference to this until we are done
      BindParameters();
      ExecuteNext();
    }

    public virtual bool ExecuteNext()
    {
      if (buffers.Count == 0)
        return false;

      MySqlPacket packet = (MySqlPacket)buffers[0];
      //MemoryStream ms = stream.InternalBuffer;
      Driver.SendQuery(packet);
      buffers.RemoveAt(0);
      return true;
    }

    protected virtual void BindParameters()
    {
      MySqlParameterCollection parameters = command.Parameters;
      int index = 0;

      while (true)
      {
        InternalBindParameters(ResolvedCommandText, parameters, null);

        // if we are not batching, then we are done.  This is only really relevant the
        // first time through
        if (command.Batch == null) return;
        while (index < command.Batch.Count)
        {
          MySqlCommand batchedCmd = command.Batch[index++];
          MySqlPacket packet = (MySqlPacket)buffers[buffers.Count - 1];

          // now we make a guess if this statement will fit in our current stream
          long estimatedCmdSize = batchedCmd.EstimatedSize();
          if (((packet.Length - 4) + estimatedCmdSize) > Connection.driver.MaxPacketSize)
          {
            // it won't, so we setup to start a new run from here
            parameters = batchedCmd.Parameters;
            break;
          }

          // looks like we might have room for it so we remember the current end of the stream
          buffers.RemoveAt(buffers.Count - 1);
          //long originalLength = packet.Length - 4;

          // and attempt to stream the next command
          string text = ResolvedCommandText;
          if (text.StartsWith("(", StringComparison.Ordinal))
            packet.WriteStringNoNull(", ");
          else
            packet.WriteStringNoNull("; ");
          InternalBindParameters(text, batchedCmd.Parameters, packet);
          if ((packet.Length - 4) > Connection.driver.MaxPacketSize)
          {
            //TODO
            //stream.InternalBuffer.SetLength(originalLength);
            parameters = batchedCmd.Parameters;
            break;
          }
        }
        if (index == command.Batch.Count)
          return;
      }
    }

    private void InternalBindParameters(string sql, MySqlParameterCollection parameters,
        MySqlPacket packet)
    {
      bool sqlServerMode = command.Connection.Settings.SqlServerMode;

      if (packet == null)
      {
        packet = new MySqlPacket(Driver.Encoding);
        packet.Version = Driver.Version;
        packet.WriteByte(0);
      }

      MySqlTokenizer tokenizer = new MySqlTokenizer(sql);
      tokenizer.ReturnComments = true;
      tokenizer.SqlServerMode = sqlServerMode;

      int pos = 0;
      string token = tokenizer.NextToken();
      int parameterCount = 0;
      while (token != null)
      {
        // serialize everything that came before the token (i.e. whitespace)
        packet.WriteStringNoNull(sql.Substring(pos, tokenizer.StartIndex - pos));
        pos = tokenizer.StopIndex;
        if (MySqlTokenizer.IsParameter(token))
        {
          if ((!parameters.containsUnnamedParameters && token.Length == 1 && parameterCount > 0) || parameters.containsUnnamedParameters && token.Length > 1)
            throw new MySqlException("Resources.MixedParameterNamingNotAllowed");


		  parameters.containsUnnamedParameters = token.Length == 1;
          if (SerializeParameter(parameters, packet, token, parameterCount))
            token = null;
          parameterCount++;
        }
        if (token != null)
        {
          if (sqlServerMode && tokenizer.Quoted && token.StartsWith("[", StringComparison.Ordinal))
            token = String.Format("`{0}`", token.Substring(1, token.Length - 2));
          packet.WriteStringNoNull(token);
        }
        token = tokenizer.NextToken();
      }
      buffers.Add(packet);
    }

    protected virtual bool ShouldIgnoreMissingParameter(string parameterName)
    {
      if (Connection.Settings.AllowUserVariables)
        return true;
      if (parameterName.StartsWith("@" + StoredProcedure.ParameterPrefix, StringComparison.OrdinalIgnoreCase))
        return true;
      if (parameterName.Length > 1 &&
          (parameterName[1] == '`' || parameterName[1] == '\''))
        return true;
      return false;
    }

    /// <summary>
    /// Serializes the given parameter to the given memory stream
    /// </summary>
    /// <remarks>
    /// <para>This method is called by PrepareSqlBuffers to convert the given
    /// parameter to bytes and write those bytes to the given memory stream.
    /// </para>
    /// </remarks>
    /// <returns>True if the parameter was successfully serialized, false otherwise.</returns>
    private bool SerializeParameter(MySqlParameterCollection parameters,
                                    MySqlPacket packet, string parmName, int parameterIndex)
    {
      MySqlParameter parameter = null;

      if (!parameters.containsUnnamedParameters)
        parameter = parameters.GetParameterFlexible(parmName, false);
      else
      {
        if (parameterIndex <= parameters.Count)
          parameter = parameters[parameterIndex];
        else
          throw new MySqlException("Resources.ParameterIndexNotFound");

	  }

      if (parameter == null)
      {
        // if we are allowing user variables and the parameter name starts with @
        // then we can't throw an exception
        if (parmName.StartsWith("@", StringComparison.Ordinal) && ShouldIgnoreMissingParameter(parmName))
          return false;
        throw new MySqlException(
            String.Format("Resources.ParameterMustBeDefined", parmName));

	  }
      parameter.Serialize(packet, false, Connection.Settings);
      return true;
    }
  }
}
