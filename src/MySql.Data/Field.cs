// Copyright ?2004, 2015, Oracle and/or its affiliates. All rights reserved.
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

using System.Text;
using MySql.Data.Common;
using MySql.Data.Types;
using System.Globalization;
using System.Text.RegularExpressions;
using System;
using System.Collections;
using System.Collections.Generic;

namespace MySql.Data.MySqlClient
{
	internal enum ColumnFlags : int
	{
		NOT_NULL = 1,
		PRIMARY_KEY = 2,
		UNIQUE_KEY = 4,
		MULTIPLE_KEY = 8,
		BLOB = 16,
		UNSIGNED = 32,
		ZERO_FILL = 64,
		BINARY = 128,
		ENUM = 256,
		AUTO_INCREMENT = 512,
		TIMESTAMP = 1024,
		SET = 2048,
		NUMBER = 32768
	};

	/// <summary>
	/// Summary description for Field.
	/// </summary>
	internal class MySqlField
	{
		#region Fields

		// public fields
		public string CatalogName;
		public int ColumnLength;
		public string ColumnName;
		public string OriginalColumnName;
		public string TableName;
		public string RealTableName;
		public string DatabaseName;
		public Encoding Encoding;
		public int maxLength;

		// protected fields
		protected ColumnFlags colFlags;
		protected int charSetIndex;
		protected byte precision;
		protected byte scale;
		protected MySqlDbType mySqlDbType;
		protected DBVersion connVersion;
		protected Driver driver;
		protected bool binaryOk;
		protected List<Type> typeConversions = new List<Type>();

		#endregion

		public MySqlField(Driver driver)
		{
			this.driver = driver;
			connVersion = driver.Version;
			maxLength = 1;
			binaryOk = true;
		}

		#region Properties

		public int CharacterSetIndex
		{
			get { return charSetIndex; }
			set { charSetIndex = value; SetFieldEncoding(); }
		}

		public MySqlDbType Type
		{
			get { return mySqlDbType; }
		}

		public byte Precision
		{
			get { return precision; }
			set { precision = value; }
		}

		public byte Scale
		{
			get { return scale; }
			set { scale = value; }
		}

		public int MaxLength
		{
			get { return maxLength; }
			set { maxLength = value; }
		}

		public ColumnFlags Flags
		{
			get { return colFlags; }
		}

		public bool IsAutoIncrement
		{
			get { return (colFlags & ColumnFlags.AUTO_INCREMENT) > 0; }
		}

		public bool IsNumeric
		{
			get { return (colFlags & ColumnFlags.NUMBER) > 0; }
		}

		public bool AllowsNull
		{
			get { return (colFlags & ColumnFlags.NOT_NULL) == 0; }
		}

		public bool IsUnique
		{
			get { return (colFlags & ColumnFlags.UNIQUE_KEY) > 0; }
		}

		public bool IsPrimaryKey
		{
			get { return (colFlags & ColumnFlags.PRIMARY_KEY) > 0; }
		}

		public bool IsBlob
		{
			get
			{
				return (mySqlDbType >= MySqlDbType.TinyBlob &&
				mySqlDbType <= MySqlDbType.Blob) ||
				(mySqlDbType >= MySqlDbType.TinyText &&
				mySqlDbType <= MySqlDbType.Text) ||
				(colFlags & ColumnFlags.BLOB) > 0;
			}
		}

		public bool IsBinary
		{
			get
			{
				return binaryOk && (CharacterSetIndex == 63);
			}
		}

		public bool IsUnsigned
		{
			get { return (colFlags & ColumnFlags.UNSIGNED) > 0; }
		}

		public bool IsTextField
		{
			get
			{
				return Type == MySqlDbType.VarString || Type == MySqlDbType.VarChar ||
							Type == MySqlDbType.String || (IsBlob && !IsBinary);
			}
		}

		public int CharacterLength
		{
			get { return ColumnLength / MaxLength; }
		}

		public List<Type> TypeConversions
		{
			get { return typeConversions; }
		}

		#endregion

		public void SetTypeAndFlags(MySqlDbType type, ColumnFlags flags)
		{
			colFlags = flags;
			mySqlDbType = type;

			if (String.IsNullOrEmpty(TableName) && String.IsNullOrEmpty(RealTableName) &&
			  IsBinary && driver.Settings.FunctionsReturnString)
			{
				CharacterSetIndex = driver.ConnectionCharSetIndex;
			}

			// if our type is an unsigned number, then we need
			// to bump it up into our unsigned types
			// we're trusting that the server is not going to set the UNSIGNED
			// flag unless we are a number
			if (IsUnsigned)
			{
				switch (type)
				{
					case MySqlDbType.Byte:
						mySqlDbType = MySqlDbType.UByte;
						return;
					case MySqlDbType.Int16:
						mySqlDbType = MySqlDbType.UInt16;
						return;
					case MySqlDbType.Int24:
						mySqlDbType = MySqlDbType.UInt24;
						return;
					case MySqlDbType.Int32:
						mySqlDbType = MySqlDbType.UInt32;
						return;
					case MySqlDbType.Int64:
						mySqlDbType = MySqlDbType.UInt64;
						return;
				}
			}

			if (IsBlob)
			{
				// handle blob to UTF8 conversion if requested.  This is only activated
				// on binary blobs
				if (IsBinary && driver.Settings.TreatBlobsAsUTF8)
				{
					bool convertBlob = false;
					Regex includeRegex = driver.Settings.GetBlobAsUTF8IncludeRegex();
					Regex excludeRegex = driver.Settings.GetBlobAsUTF8ExcludeRegex();
					if (includeRegex != null && includeRegex.IsMatch(ColumnName))
						convertBlob = true;
					else if (includeRegex == null && excludeRegex != null &&
					  !excludeRegex.IsMatch(ColumnName))
						convertBlob = true;

					if (convertBlob)
					{
						binaryOk = false;
						Encoding = System.Text.Encoding.GetEncoding("UTF-8");
						charSetIndex = -1;  // lets driver know we are in charge of encoding
						maxLength = 4;
					}
				}

				if (!IsBinary)
				{
					if (type == MySqlDbType.TinyBlob)
						mySqlDbType = MySqlDbType.TinyText;
					else if (type == MySqlDbType.MediumBlob)
						mySqlDbType = MySqlDbType.MediumText;
					else if (type == MySqlDbType.Blob)
						mySqlDbType = MySqlDbType.Text;
					else if (type == MySqlDbType.LongBlob)
						mySqlDbType = MySqlDbType.LongText;
				}
			}

			// now determine if we really should be binary
			if (driver.Settings.RespectBinaryFlags)
				CheckForExceptions();

			if (Type == MySqlDbType.String && CharacterLength == 36 && !driver.Settings.OldGuids)
				mySqlDbType = MySqlDbType.Guid;

			if (!IsBinary) return;

			if (driver.Settings.RespectBinaryFlags)
			{
				if (type == MySqlDbType.String)
					mySqlDbType = MySqlDbType.Binary;
				else if (type == MySqlDbType.VarChar ||
					 type == MySqlDbType.VarString)
					mySqlDbType = MySqlDbType.VarBinary;
			}

			if (CharacterSetIndex == 63)
				CharacterSetIndex = driver.ConnectionCharSetIndex;

			if (Type == MySqlDbType.Binary && ColumnLength == 16 && driver.Settings.OldGuids)
				mySqlDbType = MySqlDbType.Guid;
		}

		public void AddTypeConversion(Type t)
		{
			if (TypeConversions.Contains(t)) return;
			TypeConversions.Add(t);
		}

		private void CheckForExceptions()
		{
			string colName = String.Empty;
			if (OriginalColumnName != null)
				colName = OriginalColumnName.ToUpper();
			if (colName.StartsWith("CHAR(", StringComparison.Ordinal))
				binaryOk = false;
		}

		public IMySqlValue GetValueObject()
		{
			IMySqlValue v = GetIMySqlValue(Type);
			if (v is MySqlByte && ColumnLength == 1 && driver.Settings.TreatTinyAsBoolean)
			{
				MySqlByte b = (MySqlByte)v;
				b.TreatAsBoolean = true;
				v = b;
			}
			else if (v is MySqlGuid)
			{
				MySqlGuid g = (MySqlGuid)v;
				g.OldGuids = driver.Settings.OldGuids;
				v = g;
			}
			return v;
		}

		public static IMySqlValue GetIMySqlValue(MySqlDbType type)
		{
			switch (type)
			{
				case MySqlDbType.Byte:
					return new MySqlByte();
				case MySqlDbType.UByte:
					return new MySqlUByte();
				case MySqlDbType.Int16:
					return new MySqlInt16();
				case MySqlDbType.UInt16:
					return new MySqlUInt16();
				case MySqlDbType.Int24:
				case MySqlDbType.Int32:
				case MySqlDbType.Year:
					return new MySqlInt32(type, true);
				case MySqlDbType.UInt24:
				case MySqlDbType.UInt32:
					return new MySqlUInt32(type, true);
				case MySqlDbType.Bit:
					return new MySqlBit();
				case MySqlDbType.Int64:
					return new MySqlInt64();
				case MySqlDbType.UInt64:
					return new MySqlUInt64();
				case MySqlDbType.Time:
					return new MySqlTimeSpan();
				case MySqlDbType.Date:
				case MySqlDbType.DateTime:
				case MySqlDbType.Newdate:
				case MySqlDbType.Timestamp:
					return new MySqlDateTime(type, true);
				case MySqlDbType.Decimal:
				case MySqlDbType.NewDecimal:
					return new MySqlDecimal();
				case MySqlDbType.Float:
					return new MySqlSingle();
				case MySqlDbType.Double:
					return new MySqlDouble();
				case MySqlDbType.Set:
				case MySqlDbType.Enum:
				case MySqlDbType.String:
				case MySqlDbType.VarString:
				case MySqlDbType.VarChar:
				case MySqlDbType.Text:
				case MySqlDbType.TinyText:
				case MySqlDbType.MediumText:
				case MySqlDbType.LongText:
				case MySqlDbType.JSON:
				case (MySqlDbType)Field_Type.NULL:
					return new MySqlString(type, true);
#if !CF
				case MySqlDbType.Geometry:
					return new MySqlGeometry(type, true);
#endif
				case MySqlDbType.Blob:
				case MySqlDbType.MediumBlob:
				case MySqlDbType.LongBlob:
				case MySqlDbType.TinyBlob:
				case MySqlDbType.Binary:
				case MySqlDbType.VarBinary:
					return new MySqlBinary(type, true);
				case MySqlDbType.Guid:
					return new MySqlGuid();
				default:
					throw new MySqlException("Unknown data type");
			}
		}

		private void SetFieldEncoding()
		{
			Dictionary<int, string> charSets = driver.CharacterSets;
			DBVersion version = driver.Version;

			if (charSets == null || charSets.Count == 0 || CharacterSetIndex == -1) return;
			if (charSets[CharacterSetIndex] == null) return;

			CharacterSet cs = CharSetMap.GetCharacterSet(version, (string)charSets[CharacterSetIndex]);
			// starting with 6.0.4 utf8 has a maxlen of 4 instead of 3.  The old
			// 3 byte utf8 is utf8mb3
			if (cs.name.ToLower() == "utf-8" &&
			  version.Major >= 6)
				MaxLength = 4;
			else
				MaxLength = cs.byteCount;
			Encoding = CharSetMap.GetEncoding(version, (string)charSets[CharacterSetIndex]);
		}
	}
}
