// Copyright © 2009, 2013, Oracle and/or its affiliates. All rights reserved.
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
using System.Data;

using MySql.Data.Types;
using System.Diagnostics;
using System.Collections.Generic;

namespace MySql.Data.MySqlClient
{
  internal class ResultSet
  {
    private Driver driver;
    private bool hasRows;
    private bool[] uaFieldsUsed;
    private MySqlField[] fields;
    private IMySqlValue[] values;
    private Dictionary<string, int> fieldHashCS;
    private Dictionary<string, int> fieldHashCI;
    private int rowIndex;
    private bool readDone;
    private bool isSequential;
    private int seqIndex;
    private bool isOutputParameters;
    private int affectedRows;
    private long insertedId;
    private int statementId;
    private int totalRows;
    private int skippedRows;
    private bool cached;
    private List<IMySqlValue[]> cachedValues;

    public ResultSet(int affectedRows, long insertedId)
    {
      this.affectedRows = affectedRows;
      this.insertedId = insertedId;
      readDone = true;
    }

    public ResultSet(Driver d, int statementId, int numCols)
    {
      affectedRows = -1;
      insertedId = -1;
      driver = d;
      this.statementId = statementId;
      rowIndex = -1;
      LoadColumns(numCols);
      isOutputParameters = IsOutputParameterResultSet();
      hasRows = GetNextRow();
      readDone = !hasRows;
    }

    #region Properties

    public bool HasRows
    {
      get { return hasRows; }
    }

    public int Size
    {
      get { return fields == null ? 0 : fields.Length; }
    }

    public MySqlField[] Fields
    {
      get { return fields; }
    }

    public IMySqlValue[] Values
    {
      get { return values; }
    }

    public bool IsOutputParameters
    {
      get { return isOutputParameters; }
      set { isOutputParameters = value; }
    }

    public int AffectedRows
    {
      get { return affectedRows; }
    }

    public long InsertedId
    {
      get { return insertedId; }
    }

    public int TotalRows
    {
      get { return totalRows; }
    }

    public int SkippedRows
    {
      get { return skippedRows; }
    }

    public bool Cached
    {
      get { return cached; }
      set
      {
        cached = value;
        if (cached && cachedValues == null)
          cachedValues = new List<IMySqlValue[]>();
      }
    }

    #endregion

    /// <summary>
    /// return the ordinal for the given column name
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public int GetOrdinal(string name)
    {
      // first we try a quick hash lookup
      int ordinal;
      if (fieldHashCS.TryGetValue(name, out ordinal))
        return ordinal;

      // ok that failed so we use our CI hash      
      if (fieldHashCI.TryGetValue( name, out ordinal ))
        return ordinal;

      // Throw an exception if the ordinal cannot be found.
      throw new IndexOutOfRangeException(
          String.Format("Resources.CouldNotFindColumnName", name));

	}

    /// <summary>
    /// Retrieve the value as the given column index
    /// </summary>
    /// <param name="index">The column value to retrieve</param>
    /// <returns>The value as the given column</returns>
    public IMySqlValue this[int index]
    {
      get
      {
        if (rowIndex < 0)
          throw new MySqlException("Resources.AttemptToAccessBeforeRead");

		// keep count of how many columns we have left to access
		uaFieldsUsed[index] = true;

        if (isSequential && index != seqIndex)
        {
          if (index < seqIndex)
            throw new MySqlException("Resources.ReadingPriorColumnUsingSeqAccess");

		  while (seqIndex < (index - 1))
            driver.SkipColumnValue(values[++seqIndex]);
          values[index] = driver.ReadColumnValue(index, fields[index], values[index]);
          seqIndex = index;
        }

        return values[index];
      }
    }

    private bool GetNextRow()
    {
      bool fetched = driver.FetchDataRow(statementId, Size);
      if (fetched)
        totalRows++;
      return fetched;
    }


    public bool NextRow(CommandBehavior behavior)
    {
      if (readDone)
      {
        if (Cached) return CachedNextRow(behavior);
        return false;
      }

      if ((behavior & CommandBehavior.SingleRow) != 0 && rowIndex == 0)
        return false;

      isSequential = (behavior & CommandBehavior.SequentialAccess) != 0;
      seqIndex = -1;

      // if we are at row index >= 0 then we need to fetch the data row and load it
      if (rowIndex >= 0)
      {
        bool fetched = false;
        try
        {
          fetched = GetNextRow();
        }
        catch (MySqlException ex)
        {
          if (ex.IsQueryAborted)
          {
            // avoid hanging on Close()
            readDone = true;
          }
          throw;
        }

        if (!fetched)
        {
          readDone = true;
          return false;
        }
      }

      if (!isSequential) ReadColumnData(false);
      rowIndex++;
      return true;
    }

    private bool CachedNextRow(CommandBehavior behavior)
    {
      if ((behavior & CommandBehavior.SingleRow) != 0 && rowIndex == 0)
        return false;
      if (rowIndex == (totalRows - 1)) return false;
      rowIndex++;
      values = cachedValues[rowIndex];
      return true;
    }

    /// <summary>
    /// Closes the current resultset, dumping any data still on the wire
    /// </summary>
    public void Close()
    {
      if (!readDone)
      {

        // if we have rows but the user didn't read the first one then mark it as skipped
        if (HasRows && rowIndex == -1)
          skippedRows++;
        try
        {
          while (driver.IsOpen && driver.SkipDataRow())
          {
            totalRows++;
            skippedRows++;
          }
        }
        catch (System.IO.IOException)
        {
          // it is ok to eat IO exceptions here, we just want to 
          // close the result set
        }
        readDone = true;
      }
      else if (driver == null)
        CacheClose();

      driver = null;
      if (Cached) CacheReset();
    }

    private void CacheClose()
    {
      skippedRows = totalRows - rowIndex - 1;
    }

    private void CacheReset()
    {
      if (!Cached) return;
      rowIndex = -1;
      affectedRows = -1;
      insertedId = -1;
      skippedRows = 0;
    }

    public bool FieldRead(int index)
    {
      Debug.Assert(Size > index);
      return uaFieldsUsed[index];
    }

    public void SetValueObject(int i, IMySqlValue valueObject)
    {
      Debug.Assert(values != null);
      Debug.Assert(i < values.Length);
      values[i] = valueObject;
    }

    private bool IsOutputParameterResultSet()
    {
      if (driver.HasStatus(ServerStatusFlags.OutputParameters)) return true;

      if (fields.Length == 0) return false;

      for (int x = 0; x < fields.Length; x++)
        if (!fields[x].ColumnName.StartsWith("@" + StoredProcedure.ParameterPrefix, StringComparison.OrdinalIgnoreCase)) return false;
      return true;
    }

    /// <summary>
    /// Loads the column metadata for the current resultset
    /// </summary>
    private void LoadColumns(int numCols)
    {
      fields = driver.GetColumns(numCols);

      values = new IMySqlValue[numCols];
      uaFieldsUsed = new bool[numCols];
      fieldHashCS = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
      fieldHashCI = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

      for (int i = 0; i < fields.Length; i++)
      {
        string columnName = fields[i].ColumnName;
        if (!fieldHashCS.ContainsKey(columnName))
          fieldHashCS.Add(columnName, i);
        if (!fieldHashCI.ContainsKey(columnName))
          fieldHashCI.Add(columnName, i);
        values[i] = fields[i].GetValueObject();
      }
    }

    private void ReadColumnData(bool outputParms)
    {
      for (int i = 0; i < Size; i++)
        values[i] = driver.ReadColumnValue(i, fields[i], values[i]);

      // if we are caching then we need to save a copy of this row of data values
      if (Cached)
        cachedValues.Add((IMySqlValue[])values.Clone());

      // we don't need to worry about caching the following since you won't have output
      // params with TableDirect commands
      if (outputParms)
      {
        bool rowExists = driver.FetchDataRow(statementId, fields.Length);
        rowIndex = 0;
        if (rowExists)
          throw new MySqlException("Resources.MoreThanOneOPRow");
      }
    }
  }
}
