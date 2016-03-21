// Copyright (c) 2004-2008 MySQL AB, 2008-2009 Sun Microsystems, Inc., 2009, 2014 Oracle and/or its affiliates. All rights reserved.
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
using System.Data;
using System.IO;
using MySql.Data.MySqlClient;
using System.Globalization;


namespace MySql.Data.Types
{

  /// <summary>
  /// 
  /// </summary>
  public partial struct MySqlDateTime : IMySqlValue, IComparable
  {
    private bool isNull;
    private MySqlDbType type;
    private int year, month, day, hour, minute, second;
    private int millisecond, microsecond;
    public int TimezoneOffset;

    /// <summary>
    /// Constructs a new <b>MySqlDateTime</b> object by setting the individual time properties to
    /// the given values.
    /// </summary>
    /// <param name="year">The year to use.</param>
    /// <param name="month">The month to use.</param>
    /// <param name="day">The day to use.</param>
    /// <param name="hour">The hour to use.</param>
    /// <param name="minute">The minute to use.</param>
    /// <param name="second">The second to use.</param>
    /// <param name="microsecond">The microsecond to use.</param>
    public MySqlDateTime(int year, int month, int day, int hour, int minute, int second, int microsecond)
      : this(MySqlDbType.DateTime, year, month, day, hour, minute, second, microsecond)
    {
    }

    /// <summary>
    /// Constructs a new <b>MySqlDateTime</b> object by using values from the given <see cref="DateTime"/> object.
    /// </summary>
    /// <param name="dt">The <see cref="DateTime"/> object to copy.</param>
    public MySqlDateTime(DateTime dt)
      : this(MySqlDbType.DateTime, dt)
    {
    }

    /// <summary>
    /// Constructs a new <b>MySqlDateTime</b> object by copying the current value of the given object.
    /// </summary>
    /// <param name="mdt">The <b>MySqlDateTime</b> object to copy.</param>
    public MySqlDateTime(MySqlDateTime mdt)
    {
      year = mdt.Year;
      month = mdt.Month;
      day = mdt.Day;
      hour = mdt.Hour;
      minute = mdt.Minute;
      second = mdt.Second;
      microsecond = 0;
      millisecond = 0;
      type = MySqlDbType.DateTime;
      isNull = false;
      TimezoneOffset = 0;
    }

    /// <summary>
    /// Enables the contruction of a <b>MySqlDateTime</b> object by parsing a string.
    /// </summary>
    public MySqlDateTime(string dateTime)
      : this(MySqlDateTime.Parse(dateTime))
    {
    }

    internal MySqlDateTime(MySqlDbType type, int year, int month, int day, int hour, int minute,
      int second, int microsecond)
    {
      this.isNull = false;
      this.type = type;
      this.year = year;
      this.month = month;
      this.day = day;
      this.hour = hour;
      this.minute = minute;
      this.second = second;
      this.microsecond = microsecond;
      this.millisecond = this.microsecond / 1000;
      this.TimezoneOffset = 0;
    }

    internal MySqlDateTime(MySqlDbType type, bool isNull)
      : this(type, 0, 0, 0, 0, 0, 0, 0)
    {
      this.isNull = isNull;
    }

    internal MySqlDateTime(MySqlDbType type, DateTime val)
      : this(type, 0, 0, 0, 0, 0, 0, 0)
    {
      this.isNull = false;
      year = val.Year;
      month = val.Month;
      day = val.Day;
      hour = val.Hour;
      minute = val.Minute;
      second = val.Second;
      Microsecond = (int)(val.Ticks % 10000000) / 10;
    }

    #region Properties

    /// <summary>
    /// Indicates if this object contains a value that can be represented as a DateTime
    /// </summary>
    public bool IsValidDateTime
    {
      get
      {
        return year != 0 && month != 0 && day != 0;
      }
    }

    /// <summary>Returns the year portion of this datetime</summary>
    public int Year
    {
      get { return year; }
      set { year = value; }
    }

    /// <summary>Returns the month portion of this datetime</summary>
    public int Month
    {
      get { return month; }
      set { month = value; }
    }

    /// <summary>Returns the day portion of this datetime</summary>
    public int Day
    {
      get { return day; }
      set { day = value; }
    }

    /// <summary>Returns the hour portion of this datetime</summary>
    public int Hour
    {
      get { return hour; }
      set { hour = value; }
    }

    /// <summary>Returns the minute portion of this datetime</summary>
    public int Minute
    {
      get { return minute; }
      set { minute = value; }
    }

    /// <summary>Returns the second portion of this datetime</summary>
    public int Second
    {
      get { return second; }
      set { second = value; }
    }

    /// <summary>
    /// Returns the milliseconds portion of this datetime 
    /// expressed as a value between 0 and 999
    /// </summary>
    public int Millisecond {
      get { return millisecond; }
      set
      {
        if (value < 0 || value > 999)
          throw new ArgumentOutOfRangeException("Millisecond", "InvalidMillisecondValue");
        millisecond = value;
        microsecond = value * 1000;
      }
    }

    /// <summary>
    /// Returns the microseconds portion of this datetime (6 digit precision)
    /// </summary>
    public int Microsecond
    {
      get { return microsecond; }
      set
      {
        if (value < 0 || value > 999999)
          throw new ArgumentOutOfRangeException("Microsecond", "Resources.InvalidMicrosecondValue");

		microsecond = value;
        millisecond = value / 1000;
      }
    }

    #endregion

    #region IMySqlValue Members

    /// <summary>
    /// Returns true if this datetime object has a null value
    /// </summary>
    public bool IsNull
    {
      get { return isNull; }
    }

    MySqlDbType IMySqlValue.MySqlDbType
    {
      get { return type; }
    }

    object IMySqlValue.Value
    {
      get { return GetDateTime(); }
    }

    /// <summary>
    /// Retrieves the value of this <see cref="MySqlDateTime"/> as a DateTime object.
    /// </summary>
    public DateTime Value
    {
      get { return GetDateTime(); }
    }

    Type IMySqlValue.SystemType
    {
      get { return typeof(DateTime); }
    }

    string IMySqlValue.MySqlTypeName
    {
      get
      {
        switch (type)
        {
          case MySqlDbType.Date: return "DATE";
          case MySqlDbType.Newdate: return "NEWDATE";
          case MySqlDbType.Timestamp: return "TIMESTAMP";
        }
        return "DATETIME";
      }
    }


    private void SerializeText(MySqlPacket packet, MySqlDateTime value)
    {
      string val = String.Empty;

      val = String.Format("{0:0000}-{1:00}-{2:00}",
                value.Year, value.Month, value.Day);
      if (type != MySqlDbType.Date)
      {
        val = value.Microsecond > 0 ? String.Format("{0} {1:00}:{2:00}:{3:00}.{4:000000}", val,
          value.Hour, value.Minute, value.Second, value.Microsecond) : String.Format("{0} {1:00}:{2:00}:{3:00} ", val,
          value.Hour, value.Minute, value.Second);
      }

      packet.WriteStringNoNull("'" + val + "'");
    }

    void IMySqlValue.WriteValue(MySqlPacket packet, bool binary, object value, int length)
    {
      MySqlDateTime dtValue;

      string valueAsString = value as string;

      if (value is DateTime)
        dtValue = new MySqlDateTime(type, (DateTime)value);
      else if (valueAsString != null)
        dtValue = MySqlDateTime.Parse(valueAsString);
      else if (value is MySqlDateTime)
        dtValue = (MySqlDateTime)value;
      else
        throw new MySqlException("Unable to serialize date/time value.");

      if (!binary)
      {
        SerializeText(packet, dtValue);
        return;
      }

      if (dtValue.Microsecond > 0)
        packet.WriteByte(11);
      else
        packet.WriteByte(7);

      packet.WriteInteger(dtValue.Year, 2);
      packet.WriteByte((byte)dtValue.Month);
      packet.WriteByte((byte)dtValue.Day);
      if (type == MySqlDbType.Date)
      {
        packet.WriteByte(0);
        packet.WriteByte(0);
        packet.WriteByte(0);
      }
      else
      {
        packet.WriteByte((byte)dtValue.Hour);
        packet.WriteByte((byte)dtValue.Minute);
        packet.WriteByte((byte)dtValue.Second);
      }

      if (dtValue.Microsecond > 0)
      {
        long val = dtValue.Microsecond;
        for (int x = 0; x < 4; x++)
        {
          packet.WriteByte((byte)(val & 0xff));
          val >>= 8;
        }
      }
    }

    static internal MySqlDateTime Parse(string s)
    {
      MySqlDateTime dt = new MySqlDateTime();
      return dt.ParseMySql(s);
    }

    static internal MySqlDateTime Parse(string s, Common.DBVersion version)
    {
      MySqlDateTime dt = new MySqlDateTime();
      return dt.ParseMySql(s);
    }

    private MySqlDateTime ParseMySql(string s)
    {
      string[] parts = s.Split('-', ' ', ':', '/', '.');

      int year = int.Parse(parts[0]);
      int month = int.Parse(parts[1]);
      int day = int.Parse(parts[2]);

      int hour = 0, minute = 0, second = 0, microsecond = 0;
      if (parts.Length > 3)
      {
        hour = int.Parse(parts[3]);
        minute = int.Parse(parts[4]);
        second = int.Parse(parts[5]);
      }

      if (parts.Length > 6)
      {
        microsecond = int.Parse(parts[6].PadRight(6, '0'));
      }

      return new MySqlDateTime(type, year, month, day, hour, minute, second, microsecond);
    }

    IMySqlValue IMySqlValue.ReadValue(MySqlPacket packet, long length, bool nullVal)
    {

      if (nullVal) return new MySqlDateTime(type, true);

      if (length >= 0)
      {
        string value = packet.ReadString(length);
        return ParseMySql(value);
      }

      long bufLength = packet.ReadByte();
      int year = 0, month = 0, day = 0;
      int hour = 0, minute = 0, second = 0, microsecond = 0;
      if (bufLength >= 4)
      {
        year = packet.ReadInteger(2);
        month = packet.ReadByte();
        day = packet.ReadByte();
      }

      if (bufLength > 4)
      {
        hour = packet.ReadByte();
        minute = packet.ReadByte();
        second = packet.ReadByte();
      }

      if (bufLength > 7)
      {
        microsecond = packet.Read3ByteInt();
        packet.ReadByte();
      }

      return new MySqlDateTime(type, year, month, day, hour, minute, second, microsecond);
    }

    void IMySqlValue.SkipValue(MySqlPacket packet)
    {
      int len = packet.ReadByte();
      packet.Position += len;
    }

    #endregion

    /// <summary>Returns this value as a DateTime</summary>
    public DateTime GetDateTime()
    {
      if (!IsValidDateTime)
        throw new MySqlConversionException("Unable to convert MySQL date/time value to System.DateTime");

      DateTimeKind kind = DateTimeKind.Unspecified;
      if (type == MySqlDbType.Timestamp)
      {
        if (TimezoneOffset == 0)
          kind = DateTimeKind.Utc;
        else 
          kind = DateTimeKind.Local;
      }

      return new DateTime(year, month, day, hour, minute, second, kind).AddTicks(microsecond * 10);
    }

    private static string FormatDateCustom(string format, int monthVal, int dayVal, int yearVal)
    {
      format = format.Replace("MM", "{0:00}");
      format = format.Replace("M", "{0}");
      format = format.Replace("dd", "{1:00}");
      format = format.Replace("d", "{1}");
      format = format.Replace("yyyy", "{2:0000}");
      format = format.Replace("yy", "{3:00}");
      format = format.Replace("y", "{4:0}");

      int year2digit = yearVal - ((yearVal / 1000) * 1000);
      year2digit -= ((year2digit / 100) * 100);
      int year1digit = year2digit - ((year2digit / 10) * 10);

      return String.Format(format, monthVal, dayVal, yearVal, year2digit, year1digit);
    }

    /// <summary>Returns a MySQL specific string representation of this value</summary>
    public override string ToString()
    {
      if (this.IsValidDateTime)
      {
        DateTime d = new DateTime(year, month, day, hour, minute, second).AddTicks(microsecond * 10);
        return (type == MySqlDbType.Date) ? d.ToString("d") : d.ToString();
      }

      string dateString = FormatDateCustom(
          CultureInfo.CurrentUICulture.DateTimeFormat.ShortDatePattern, month, day, year);
      if (type == MySqlDbType.Date)
        return dateString;

      DateTime dt = new DateTime(1, 2, 3, hour, minute, second).AddTicks(microsecond * 10);
      dateString = String.Format("{0} {1}", dateString, dt.ToString("yyyy-MM-dd hh:mm:ss"));
      return dateString;
    }

    /// <summary></summary>
    /// <param name="val"></param>
    /// <returns></returns>
    public static explicit operator DateTime(MySqlDateTime val)
    {
      if (!val.IsValidDateTime) return DateTime.MinValue;
      return val.GetDateTime();
    }

    internal static void SetDSInfo(MySqlSchemaCollection sc)
    {
      string[] types = new string[] { "DATE", "DATETIME", "TIMESTAMP" };
      MySqlDbType[] dbtype = new MySqlDbType[] { MySqlDbType.Date, 
        MySqlDbType.DateTime, MySqlDbType.Timestamp };

      // we use name indexing because this method will only be called
      // when GetSchema is called for the DataSourceInformation 
      // collection and then it wil be cached.
      for (int x = 0; x < types.Length; x++)
      {
        MySqlSchemaRow row = sc.AddRow();
        row["TypeName"] = types[x];
        row["ProviderDbType"] = dbtype[x];
        row["ColumnSize"] = 0;
        row["CreateFormat"] = types[x];
        row["CreateParameters"] = null;
        row["DataType"] = "System.DateTime";
        row["IsAutoincrementable"] = false;
        row["IsBestMatch"] = true;
        row["IsCaseSensitive"] = false;
        row["IsFixedLength"] = true;
        row["IsFixedPrecisionScale"] = true;
        row["IsLong"] = false;
        row["IsNullable"] = true;
        row["IsSearchable"] = true;
        row["IsSearchableWithLike"] = false;
        row["IsUnsigned"] = false;
        row["MaximumScale"] = 0;
        row["MinimumScale"] = 0;
        row["IsConcurrencyType"] = DBNull.Value;
        row["IsLiteralSupported"] = false;
        row["LiteralPrefix"] = null;
        row["LiteralSuffix"] = null;
        row["NativeDataType"] = null;
      }
    }

    #region IComparable Members

    int IComparable.CompareTo(object obj)
    {
      MySqlDateTime otherDate = (MySqlDateTime)obj;

      if (Year < otherDate.Year) return -1;
      else if (Year > otherDate.Year) return 1;

      if (Month < otherDate.Month) return -1;
      else if (Month > otherDate.Month) return 1;

      if (Day < otherDate.Day) return -1;
      else if (Day > otherDate.Day) return 1;

      if (Hour < otherDate.Hour) return -1;
      else if (Hour > otherDate.Hour) return 1;

      if (Minute < otherDate.Minute) return -1;
      else if (Minute > otherDate.Minute) return 1;

      if (Second < otherDate.Second) return -1;
      else if (Second > otherDate.Second) return 1;

      if (Microsecond < otherDate.Microsecond) return -1;
      else if (Microsecond > otherDate.Microsecond) return 1;

      return 0;
    }

    #endregion

  }
}
