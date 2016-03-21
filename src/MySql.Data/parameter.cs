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

using System;
using System.Data;
using System.Data.Common;
using MySql.Data.Types;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Collections;
using System.ComponentModel;

namespace MySql.Data.MySqlClient
{
	/// <summary>
	/// Represents a parameter to a <see cref="MySqlCommand"/>, and optionally, its mapping to <see cref="DataSet"/> columns. This class cannot be inherited.
	/// </summary>
	public sealed class MySqlParameter : DbParameter
	{
		private const int UNSIGNED_MASK = 0x8000;
		private object paramValue;
		private ParameterDirection direction = ParameterDirection.Input;
		private bool isNullable;
		private string paramName;
		private string sourceColumn;
		private int size;
		private MySqlDbType mySqlDbType;
		private DbType dbType;
		private bool inferType;
		private bool sourceColumnNullMapping;
		private MySqlParameterCollection collection;
		private IMySqlValue valueObject;
		private Encoding encoding;
		private IList possibleValues;

		/// <summary>
		/// Initializes a new instance of the MySqlParameter class.
		/// </summary>
		public MySqlParameter()
		{
			inferType = true;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MySqlParameter"/> class with the parameter name and a value of the new MySqlParameter.
		/// </summary>
		/// <param name="parameterName">The name of the parameter to map. </param>
		/// <param name="value">An <see cref="Object"/> that is the value of the <see cref="MySqlParameter"/>. </param>
		public MySqlParameter(string parameterName, object value)
			: this()
		{
			ParameterName = parameterName;
			Value = value;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MySqlParameter"/> class with the parameter name and the data type.
		/// </summary>
		/// <param name="parameterName">The name of the parameter to map. </param>
		/// <param name="dbType">One of the <see cref="MySqlDbType"/> values. </param>
		public MySqlParameter(string parameterName, MySqlDbType dbType)
			: this(parameterName, null)
		{
			MySqlDbType = dbType;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MySqlParameter"/> class with the parameter name, the <see cref="MySqlDbType"/>, and the size.
		/// </summary>
		/// <param name="parameterName">The name of the parameter to map. </param>
		/// <param name="dbType">One of the <see cref="MySqlDbType"/> values. </param>
		/// <param name="size">The length of the parameter. </param>
		public MySqlParameter(string parameterName, MySqlDbType dbType, int size)
			: this(parameterName, dbType)
		{
			this.size = size;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MySqlParameter"/> class with the parameter name, the <see cref="MySqlDbType"/>, the size, and the source column name.
		/// </summary>
		/// <param name="parameterName">The name of the parameter to map. </param>
		/// <param name="dbType">One of the <see cref="MySqlDbType"/> values. </param>
		/// <param name="size">The length of the parameter. </param>
		/// <param name="sourceColumn">The name of the source column. </param>
		public MySqlParameter(string parameterName, MySqlDbType dbType, int size, string sourceColumn)
			:
				this(parameterName, dbType)
		{
			this.size = size;
			direction = ParameterDirection.Input;
			this.sourceColumn = sourceColumn;
		}

		internal MySqlParameter(string name, MySqlDbType type, ParameterDirection dir, string col, object val)
			: this(name, type)
		{
			direction = dir;
			sourceColumn = col;
			Value = val;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MySqlParameter"/> class with the parameter name, the type of the parameter, the size of the parameter, a <see cref="ParameterDirection"/>, the precision of the parameter, the scale of the parameter, the source column, a <see cref="DataRowVersion"/> to use, and the value of the parameter.
		/// </summary>
		/// <param name="parameterName">The name of the parameter to map. </param>
		/// <param name="dbType">One of the <see cref="MySqlDbType"/> values. </param>
		/// <param name="size">The length of the parameter. </param>
		/// <param name="direction">One of the <see cref="ParameterDirection"/> values. </param>
		/// <param name="isNullable">true if the value of the field can be null, otherwise false. </param>
		/// <param name="precision">The total number of digits to the left and right of the decimal point to which <see cref="MySqlParameter.Value"/> is resolved.</param>
		/// <param name="scale">The total number of decimal places to which <see cref="MySqlParameter.Value"/> is resolved. </param>
		/// <param name="sourceColumn">The name of the source column. </param>
		/// <param name="sourceVersion">One of the <see cref="DataRowVersion"/> values. </param>
		/// <param name="value">An <see cref="Object"/> that is the value of the <see cref="MySqlParameter"/>. </param>
		/// <exception cref="ArgumentException"/>
		public MySqlParameter(string parameterName, MySqlDbType dbType, int size, ParameterDirection direction,
							  bool isNullable, byte precision, byte scale, string sourceColumn, object value)
			: this(parameterName, dbType, size, sourceColumn)
		{
			this.direction = direction;
			Value = value;
		}

		internal MySqlParameterCollection Collection
		{
			get { return collection; }
			set { collection = value; }
		}

		internal bool TypeHasBeenSet
		{
			get { return inferType == false; }
		}

		internal Encoding Encoding
		{
			get { return encoding; }
			set { encoding = value; }
		}

		internal string BaseName
		{
			get
			{
				if (ParameterName.StartsWith("@") || ParameterName.StartsWith("?"))
					return ParameterName.Substring(1);
				return ParameterName;
			}
		}

		/// <summary>
		/// Gets or sets the <see cref="DbType"/> of the parameter.
		/// </summary>
		public override DbType DbType
		{
			get { return dbType; }
			set
			{
				SetDbType(value);
				inferType = false;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether the parameter is input-only, output-only, bidirectional, or a stored procedure return value parameter.
		/// As of MySql version 4.1 and earlier, input-only is the only valid choice.
		/// </summary>
		public override ParameterDirection Direction
		{
			get { return direction; }
			set { direction = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether the parameter accepts null values.
		/// </summary>
		public override Boolean IsNullable
		{
			get { return isNullable; }
			set { isNullable = value; }
		}

		/// <summary>
		/// Gets or sets the MySqlDbType of the parameter.
		/// </summary>        
		public MySqlDbType MySqlDbType
		{
			get { return mySqlDbType; }
			set
			{
				SetMySqlDbType(value);
				inferType = false;
			}
		}

		/// <summary>
		/// Gets or sets the name of the MySqlParameter.
		/// </summary>
		public override String ParameterName
		{
			get { return paramName; }
			set
			{
				if (collection != null)
					collection.ParameterNameChanged(this, paramName, value);
				paramName = value;
			}
		}

		/// <summary>
		/// Gets or sets the maximum size, in bytes, of the data within the column.
		/// </summary>
		public override int Size
		{
			get { return size; }
			set { size = value; }
		}

		/// <summary>
		/// Gets or sets the name of the source column that is mapped to the <see cref="DataSet"/> and used for loading or returning the <see cref="Value"/>.
		/// </summary>
		public override String SourceColumn
		{
			get { return sourceColumn; }
			set { sourceColumn = value; }
		}

		/// <summary>
		/// Gets or sets the value of the parameter.
		/// </summary>
		[TypeConverter(typeof(StringConverter))]
		public override object Value
		{
			get { return paramValue; }
			set
			{
				paramValue = value;
				byte[] valueAsByte = value as byte[];
				string valueAsString = value as string;

				if (valueAsByte != null)
					size = valueAsByte.Length;
				else if (valueAsString != null)
					size = valueAsString.Length;
				if (inferType)
					SetTypeFromValue();
			}
		}

		private IMySqlValue ValueObject
		{
			get { return valueObject; }
		}

		/// <summary>
		/// Returns the possible values for this parameter if this parameter is of type
		/// SET or ENUM.  Returns null otherwise.
		/// </summary>
		public IList PossibleValues
		{
			get { return possibleValues; }
			internal set { possibleValues = value; }
		}

		/// <summary>
		/// Overridden. Gets a string containing the <see cref="ParameterName"/>.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return paramName;
		}

		internal int GetPSType()
		{
			switch (mySqlDbType)
			{
				case MySqlDbType.Bit:
					return (int)MySqlDbType.Int64 | UNSIGNED_MASK;

				case MySqlDbType.UByte:
					return (int)MySqlDbType.Byte | UNSIGNED_MASK;

				case MySqlDbType.UInt64:
					return (int)MySqlDbType.Int64 | UNSIGNED_MASK;

				case MySqlDbType.UInt32:
					return (int)MySqlDbType.Int32 | UNSIGNED_MASK;

				case MySqlDbType.UInt24:
					return (int)MySqlDbType.Int32 | UNSIGNED_MASK;

				case MySqlDbType.UInt16:
					return (int)MySqlDbType.Int16 | UNSIGNED_MASK;

				default:
					return (int)mySqlDbType;
			}
		}

		internal void Serialize(MySqlPacket packet, bool binary, MySqlConnectionStringBuilder settings)
		{
			if (!binary && (paramValue == null || paramValue == DBNull.Value))
				packet.WriteStringNoNull("NULL");
			else
			{
				if (ValueObject.MySqlDbType == MySqlDbType.Guid)
				{
					MySqlGuid g = (MySqlGuid)ValueObject;
					g.OldGuids = settings.OldGuids;
					valueObject = g;
				}
				ValueObject.WriteValue(packet, binary, paramValue, size);
			}
		}

		private void SetMySqlDbType(MySqlDbType mysql_dbtype)
		{
			mySqlDbType = mysql_dbtype;
			valueObject = MySqlField.GetIMySqlValue(mySqlDbType);

			switch (mySqlDbType)
			{
				case MySqlDbType.Decimal:
					dbType = DbType.Decimal;
					break;

				case MySqlDbType.Byte:
					dbType = DbType.SByte;
					break;

				case MySqlDbType.UByte:
					dbType = DbType.Byte;
					break;

				case MySqlDbType.Int16:
					dbType = DbType.Int16;
					break;

				case MySqlDbType.UInt16:
					dbType = DbType.UInt16;
					break;

				case MySqlDbType.Int24:
				case MySqlDbType.Int32:
					dbType = DbType.Int32;
					break;

				case MySqlDbType.UInt24:
				case MySqlDbType.UInt32:
					dbType = DbType.UInt32;
					break;

				case MySqlDbType.Int64:
					dbType = DbType.Int64;
					break;

				case MySqlDbType.UInt64:
					dbType = DbType.UInt64;
					break;

				case MySqlDbType.Bit:
					dbType = DbType.UInt64;
					break;

				case MySqlDbType.Float:
					dbType = DbType.Single;
					break;

				case MySqlDbType.Double:
					dbType = DbType.Double;
					break;

				case MySqlDbType.Timestamp:
				case MySqlDbType.DateTime:
					dbType = DbType.DateTime;
					break;

				case MySqlDbType.Date:
				case MySqlDbType.Newdate:
				case MySqlDbType.Year:
					dbType = DbType.Date;
					break;

				case MySqlDbType.Time:
					dbType = DbType.Time;
					break;

				case MySqlDbType.Enum:
				case MySqlDbType.Set:
				case MySqlDbType.VarChar:
					dbType = DbType.String;
					break;

				case MySqlDbType.TinyBlob:
				case MySqlDbType.MediumBlob:
				case MySqlDbType.LongBlob:
				case MySqlDbType.Blob:
					dbType = DbType.Object;
					break;

				case MySqlDbType.String:
					dbType = DbType.StringFixedLength;
					break;

				case MySqlDbType.Guid:
					dbType = DbType.Guid;
					break;
			}
		}

		private void SetDbType(DbType db_type)
		{
			dbType = db_type;
			switch (dbType)
			{
				case DbType.Guid:
					mySqlDbType = MySqlDbType.Guid;
					break;

				case DbType.AnsiString:
				case DbType.String:
					mySqlDbType = MySqlDbType.VarChar;
					break;

				case DbType.AnsiStringFixedLength:
				case DbType.StringFixedLength:
					mySqlDbType = MySqlDbType.String;
					break;

				case DbType.Boolean:
				case DbType.Byte:
					mySqlDbType = MySqlDbType.UByte;
					break;

				case DbType.SByte:
					mySqlDbType = MySqlDbType.Byte;
					break;

				case DbType.Date:
					mySqlDbType = MySqlDbType.Date;
					break;

				case DbType.DateTime:
					mySqlDbType = MySqlDbType.DateTime;
					break;

				case DbType.Time:
					mySqlDbType = MySqlDbType.Time;
					break;

				case DbType.Single:
					mySqlDbType = MySqlDbType.Float;
					break;

				case DbType.Double:
					mySqlDbType = MySqlDbType.Double;
					break;

				case DbType.Int16:
					mySqlDbType = MySqlDbType.Int16;
					break;

				case DbType.UInt16:
					mySqlDbType = MySqlDbType.UInt16;
					break;

				case DbType.Int32:
					mySqlDbType = MySqlDbType.Int32;
					break;

				case DbType.UInt32:
					mySqlDbType = MySqlDbType.UInt32;
					break;

				case DbType.Int64:
					mySqlDbType = MySqlDbType.Int64;
					break;

				case DbType.UInt64:
					mySqlDbType = MySqlDbType.UInt64;
					break;

				case DbType.Decimal:
				case DbType.Currency:
					mySqlDbType = MySqlDbType.Decimal;
					break;

				case DbType.Object:
				case DbType.VarNumeric:
				case DbType.Binary:
				default:
					mySqlDbType = MySqlDbType.Blob;
					break;
			}
			valueObject = MySqlField.GetIMySqlValue(mySqlDbType);
		}

		private void SetTypeFromValue()
		{
			if (paramValue == null || paramValue == DBNull.Value) return;

			if (paramValue is Guid)
				DbType = DbType.Guid;
			else if (paramValue is TimeSpan)
				DbType = DbType.Time;
			else if (paramValue is bool)
				DbType = DbType.Byte;
			else
			{
				Type tc = paramValue.GetType();
				DbType = (tc == SByte.MinValue.GetType()) ? DbType.SByte :
					(tc == Byte.MinValue.GetType()) ? DbType.Byte :
					(tc == Int16.MinValue.GetType()) ? DbType.Int16 :
					(tc == UInt16.MinValue.GetType()) ? DbType.UInt16 :
					(tc == Int32.MinValue.GetType()) ? DbType.Int32 :
					(tc == UInt32.MinValue.GetType()) ? DbType.UInt32 :
					(tc == Int64.MinValue.GetType()) ? DbType.Int64 :
					(tc == UInt64.MinValue.GetType()) ? DbType.UInt64 :
					(tc == DateTime.MinValue.GetType()) ? DbType.DateTime :
					(tc == String.Empty.GetType()) ? DbType.String :
					(tc == Single.MinValue.GetType()) ? DbType.Single :
					(tc == Double.MinValue.GetType()) ? DbType.Double :
					(tc == Decimal.MinValue.GetType()) ? DbType.Decimal : DbType.Object;
			}
		}

		public MySqlParameter Clone()
		{
			MySqlParameter clone = new MySqlParameter(paramName, mySqlDbType, direction,
				sourceColumn, paramValue);
			// if we have not had our type set yet then our clone should not either
			clone.inferType = inferType;
			return clone;
		}

		/// <summary>
		/// Resets the <b>DbType</b> property to its original settings.
		/// </summary>
		public override void ResetDbType()
		{
			inferType = true;
		}

		/// <summary>
		/// Sets or gets a value which indicates whether the source column is nullable.
		/// This allows <see cref="DbCommandBuilder"/> to correctly generate Update statements
		/// for nullable columns.
		/// </summary>
		public override bool SourceColumnNullMapping
		{
			get { return sourceColumnNullMapping; }
			set { sourceColumnNullMapping = value; }
		}

		// this method is pretty dumb but we want it to be fast.  it doesn't return size based
		// on value and type but just on the value.
		internal long EstimatedSize()
		{
			if (Value == null || Value == DBNull.Value)
				return 4; // size of NULL
			if (Value is byte[])
				return (Value as byte[]).Length;
			if (Value is string)
				return (Value as string).Length * 4; // account for UTF-8 (yeah I know)
			if (Value is decimal || Value is float)
				return 64;
			return 32;
		}
	}


	internal class MySqlParameterConverter : TypeConverter
	{
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			// Always call the base to see if it can perform the conversion.
			return base.CanConvertTo(context, destinationType);
		}

		public override object ConvertTo(ITypeDescriptorContext context,
										 CultureInfo culture, object value, Type destinationType)
		{
			// Always call base, even if you can't convert.
			return base.ConvertTo(context, culture, value, destinationType);
		}
	}
}