// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published
// by the Free Software Foundation; version 3 of the License.
//
// This program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License
// for more details.
//
// You should have received a copy of the GNU Lesser General Public License along
// with this program; if not, write to the Free Software Foundation, Inc.,
// 51 Franklin St, Fifth Floor, Boston, MA 02110-1301  USA

using MySql.Data.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace MySql.Data.MySqlClient
{
	/// <summary>
	/// Summary description for PreparedStatement.
	/// </summary>
	internal class PreparableStatement : Statement
	{
		private int executionCount;
		private int statementId;
		private BitArray nullMap;
		private List<MySqlParameter> parametersToSend = new List<MySqlParameter>();
		private MySqlPacket packet;
		private int dataPosition;
		private int nullMapPosition;

		public PreparableStatement(MySqlCommand command, string text)
			: base(command, text)
		{
		}

		#region Properties

		public int ExecutionCount
		{
			get { return executionCount; }
			set { executionCount = value; }
		}

		public bool IsPrepared
		{
			get { return statementId > 0; }
		}

		public int StatementId
		{
			get { return statementId; }
		}

		#endregion Properties

		public virtual void Prepare()
		{
			// strip out names from parameter markers
			string text;
			List<string> parameter_names = PrepareCommandText(out text);

			// ask our connection to send the prepare command
			MySqlField[] paramList = null;
			statementId = Driver.PrepareStatement(text, ref paramList);

			// now we need to assign our field names since we stripped them out
			// for the prepare
			for (int i = 0; i < parameter_names.Count; i++)
			{
				//paramList[i].ColumnName = (string) parameter_names[i];
				string parameterName = (string)parameter_names[i];
				MySqlParameter p = Parameters.GetParameterFlexible(parameterName, false);
				if (p == null)
					throw new InvalidOperationException(
						String.Format("ResourceStrings.ParameterNotFoundDuringPrepare", parameterName));
				p.Encoding = paramList[i].Encoding;
				parametersToSend.Add(p);
			}

			// now prepare our null map
			int numNullBytes = 0;
			if (paramList != null && paramList.Length > 0)
			{
				nullMap = new BitArray(paramList.Length);
				numNullBytes = (nullMap.Length + 7) / 8;
			}

			packet = new MySqlPacket(Driver.Encoding);

			// write out some values that do not change run to run
			packet.WriteByte(0);
			packet.WriteInteger(statementId, 4);
			packet.WriteByte((byte)0); // flags; always 0 for 4.1
			packet.WriteInteger(1, 4); // interation count; 1 for 4.1
			nullMapPosition = packet.Position;
			packet.Position += numNullBytes;  // leave room for our null map
			packet.WriteByte(1); // rebound flag
								 // write out the parameter types
			foreach (MySqlParameter p in parametersToSend)
				packet.WriteInteger(p.GetPSType(), 2);
			dataPosition = packet.Position;
		}

		public override void Execute()
		{
			// if we are not prepared, then call down to our base
			if (!IsPrepared)
			{
				base.Execute();
				return;
			}

			//TODO: support long data here
			// create our null bitmap

			// we check this because Mono doesn't ignore the case where nullMapBytes
			// is zero length.
			//            if (nullMapBytes.Length > 0)
			//          {
			//            byte[] bits = packet.Buffer;
			//          nullMap.CopyTo(bits,
			//        nullMap.CopyTo(nullMapBytes, 0);

			// start constructing our packet
			//            if (Parameters.Count > 0)
			//              nullMap.CopyTo(packet.Buffer, nullMapPosition);
			//if (parameters != null && parameters.Count > 0)
			//else
			//	packet.WriteByte( 0 );
			//TODO:  only send rebound if parms change

			// now write out all non-null values
			packet.Position = dataPosition;
			for (int i = 0; i < parametersToSend.Count; i++)
			{
				MySqlParameter p = parametersToSend[i];
				nullMap[i] = (p.Value == DBNull.Value || p.Value == null) ||
					p.Direction == ParameterDirection.Output;
				if (nullMap[i]) continue;
				packet.Encoding = p.Encoding;
				p.Serialize(packet, true, Connection.Settings);
			}
			if (nullMap != null)
				ToByteArray(nullMap, packet.Buffer, nullMapPosition);

			executionCount++;

			Driver.ExecuteStatement(packet);
		}

		private void ToByteArray(BitArray bits, byte[] outp, int pos)
		{
			int numBytes = bits.Length / 8;
			if (bits.Length % 8 != 0) numBytes++;

			int byteIndex = pos, bitIndex = 0;

			for (int i = 0; i < bits.Length; i++)
			{
				if (bits[i])
					outp[byteIndex] |= (byte)(1 << (7 - bitIndex));

				bitIndex++;
				if (bitIndex == 8)
				{
					bitIndex = 0;
					byteIndex++;
				}
			}
		}

		public override bool ExecuteNext()
		{
			if (!IsPrepared)
				return base.ExecuteNext();
			return false;
		}

		/// <summary>
		/// Prepares CommandText for use with the Prepare method
		/// </summary>
		/// <returns>Command text stripped of all paramter names</returns>
		/// <remarks>
		/// Takes the output of TokenizeSql and creates a single string of SQL
		/// that only contains '?' markers for each parameter.  It also creates
		/// the parameterMap array list that includes all the paramter names in the
		/// order they appeared in the SQL
		/// </remarks>
		private List<string> PrepareCommandText(out string stripped_sql)
		{
			StringBuilder newSQL = new StringBuilder();
			List<string> parameterMap = new List<string>();

			int startPos = 0;
			string sql = ResolvedCommandText;
			MySqlTokenizer tokenizer = new MySqlTokenizer(sql);
			string parameter = tokenizer.NextParameter();
			while (parameter != null)
			{
				parameter = tokenizer.NextParameter();
			}
			newSQL.Append(sql.Substring(startPos));
			stripped_sql = newSQL.ToString();
			return parameterMap;
		}

		public virtual void CloseStatement()
		{
			if (!IsPrepared) return;

			Driver.CloseStatement(statementId);
			statementId = 0;
		}
	}
}