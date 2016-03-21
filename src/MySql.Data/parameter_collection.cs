// Copyright (c) 2004-2008 MySQL AB, 2008-2009 Sun Microsystems, Inc.
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
using System.ComponentModel;
using System.Collections.Generic;


namespace MySql.Data.MySqlClient
{
  /// <summary>
  /// Represents a collection of parameters relevant to a <see cref="MySqlCommand"/> as well as their respective mappings to columns in a <see cref="System.Data.DataSet"/>. This class cannot be inherited.
  /// </summary>
  /// <include file='docs/MySqlParameterCollection.xml' path='MyDocs/MyMembers[@name="Class"]/*'/>
  public sealed partial class MySqlParameterCollection
  {
    List<MySqlParameter> items = new List<MySqlParameter>();
    private Dictionary<string,int> indexHashCS;
    private Dictionary<string,int> indexHashCI;
    //turns to true if any parameter is unnamed
    internal bool containsUnnamedParameters;

    internal MySqlParameterCollection(MySqlCommand cmd)
    {
      indexHashCS = new Dictionary<string, int>();
      indexHashCI = new Dictionary<string,int>(StringComparer.CurrentCultureIgnoreCase);
      containsUnnamedParameters = false;
      Clear();
    }

    /// <summary>
    /// Gets the number of MySqlParameter objects in the collection.
    /// </summary>
    public override int Count
    {
      get { return items.Count; }
    }

    #region Public Methods

    /// <summary>
    /// Gets the <see cref="MySqlParameter"/> at the specified index.
    /// </summary>
    /// <overloads>Gets the <see cref="MySqlParameter"/> with a specified attribute.
    /// [C#] In C#, this property is the indexer for the <see cref="MySqlParameterCollection"/> class.
    /// </overloads>
    public new MySqlParameter this[int index]
    {
      get { return InternalGetParameter(index); }
      set { InternalSetParameter(index, value); }
    }

    /// <summary>
    /// Gets the <see cref="MySqlParameter"/> with the specified name.
    /// </summary>
    public new MySqlParameter this[string name]
    {
      get { return InternalGetParameter(name); }
      set { InternalSetParameter(name, value); }
    }

    /// <summary>
    /// Adds the specified <see cref="MySqlParameter"/> object to the <see cref="MySqlParameterCollection"/>.
    /// </summary>
    /// <param name="value">The <see cref="MySqlParameter"/> to add to the collection.</param>
    /// <returns>The newly added <see cref="MySqlParameter"/> object.</returns>
    public MySqlParameter Add(MySqlParameter value)
    {
      return InternalAdd(value, -1);
    }

    /// <summary>
    /// Adds a <see cref="MySqlParameter"/> to the <see cref="MySqlParameterCollection"/> given the specified parameter name and value.
    /// </summary>
    /// <param name="parameterName">The name of the parameter.</param>
    /// <param name="value">The <see cref="MySqlParameter.Value"/> of the <see cref="MySqlParameter"/> to add to the collection.</param>
    /// <returns>The newly added <see cref="MySqlParameter"/> object.</returns>
    [Obsolete("Add(String parameterName, Object value) has been deprecated.  Use AddWithValue(String parameterName, Object value)")]
    public MySqlParameter Add(string parameterName, object value)
    {
      return Add(new MySqlParameter(parameterName, value));
    }

    public MySqlParameter AddWithValue(string parameterName, object value)
    {
      return Add(new MySqlParameter(parameterName, value));
    }

    /// <summary>
    /// Adds a <see cref="MySqlParameter"/> to the <see cref="MySqlParameterCollection"/> given the parameter name and the data type.
    /// </summary>
    /// <param name="parameterName">The name of the parameter.</param>
    /// <param name="dbType">One of the <see cref="MySqlDbType"/> values. </param>
    /// <returns>The newly added <see cref="MySqlParameter"/> object.</returns>
    public MySqlParameter Add(string parameterName, MySqlDbType dbType)
    {
      return Add(new MySqlParameter(parameterName, dbType));
    }

    /// <summary>
    /// Adds a <see cref="MySqlParameter"/> to the <see cref="MySqlParameterCollection"/> with the parameter name, the data type, and the column length.
    /// </summary>
    /// <param name="parameterName">The name of the parameter.</param>
    /// <param name="dbType">One of the <see cref="MySqlDbType"/> values. </param>
    /// <param name="size">The length of the column.</param>
    /// <returns>The newly added <see cref="MySqlParameter"/> object.</returns>
    public MySqlParameter Add(string parameterName, MySqlDbType dbType, int size)
    {
      return Add(new MySqlParameter(parameterName, dbType, size));
    }

    #endregion

    /// <summary>
    /// Removes all items from the collection.
    /// </summary>
    public override void Clear()
    {
      foreach (MySqlParameter p in items)
        p.Collection = null;
      items.Clear();
      indexHashCS.Clear();
      indexHashCI.Clear();
    }

    void CheckIndex(int index)
    {
      if (index < 0 || index >= Count)
        throw new IndexOutOfRangeException("Parameter index is out of range.");
    }

    private MySqlParameter InternalGetParameter(int index)
    {
      CheckIndex(index);
      return items[index];
    }

    private MySqlParameter InternalGetParameter(string parameterName)
    {
      int index = IndexOf(parameterName);
      if (index < 0)
      {
        // check to see if the user has added the parameter without a
        // parameter marker.  If so, kindly tell them what they did.
        if (parameterName.StartsWith("@", StringComparison.Ordinal) ||
                    parameterName.StartsWith("?", StringComparison.Ordinal))
        {
          string newParameterName = parameterName.Substring(1);
          index = IndexOf(newParameterName);
          if (index != -1)
            return items[index];
        }
        throw new ArgumentException("Parameter '" + parameterName + "' not found in the collection.");
      }
      return items[index];
    }

    private void InternalSetParameter(string parameterName, MySqlParameter value)
    {
      int index = IndexOf(parameterName);
      if (index < 0)
        throw new ArgumentException("Parameter '" + parameterName + "' not found in the collection.");
      InternalSetParameter(index, value);
    }

    private void InternalSetParameter(int index, MySqlParameter value)
    {
      MySqlParameter newParameter = value as MySqlParameter;
      if (newParameter == null)
        throw new ArgumentException("Resources.NewValueShouldBeMySqlParameter");

      CheckIndex(index);
      MySqlParameter p = (MySqlParameter)items[index];

      // first we remove the old parameter from our hashes
      indexHashCS.Remove(p.ParameterName);
      indexHashCI.Remove(p.ParameterName);

      // then we add in the new parameter
      items[index] = newParameter;
      indexHashCS.Add(value.ParameterName, index);
      indexHashCI.Add(value.ParameterName, index);
    }

    /// <summary>
    /// Gets the location of the <see cref="MySqlParameter"/> in the collection with a specific parameter name.
    /// </summary>
    /// <param name="parameterName">The name of the <see cref="MySqlParameter"/> object to retrieve. </param>
    /// <returns>The zero-based location of the <see cref="MySqlParameter"/> in the collection.</returns>
    public override int IndexOf(string parameterName)
    {
      int i = -1;
      if (!indexHashCS.TryGetValue(parameterName, out i) &&
        !indexHashCI.TryGetValue(parameterName, out i))
        return -1;
      return i;
    }

    /// <summary>
    /// Gets the location of a <see cref="MySqlParameter"/> in the collection.
    /// </summary>
    /// <param name="value">The <see cref="MySqlParameter"/> object to locate. </param>
    /// <returns>The zero-based location of the <see cref="MySqlParameter"/> in the collection.</returns>
    /// <overloads>Gets the location of a <see cref="MySqlParameter"/> in the collection.</overloads>
    public override int IndexOf(object value)
    {
      MySqlParameter parameter = value as MySqlParameter;
      if (null == parameter)
        throw new ArgumentException("Argument must be of type DbParameter", "value");
      return items.IndexOf(parameter);
    }

    internal void ParameterNameChanged(MySqlParameter p, string oldName, string newName)
    {
      int index = IndexOf(oldName);
      indexHashCS.Remove(oldName);
      indexHashCI.Remove(oldName);

      indexHashCS.Add(newName, index);
      indexHashCI.Add(newName, index);
    }

    private MySqlParameter InternalAdd(MySqlParameter value, int index)
    {
      if (value == null)
        throw new ArgumentException("The MySqlParameterCollection only accepts non-null MySqlParameter type objects.", "value");

      // if the parameter is unnamed, then assign a default name
      if (String.IsNullOrEmpty(value.ParameterName))
        value.ParameterName = String.Format("Parameter{0}", GetNextIndex());

      // make sure we don't already have a parameter with this name
      if (IndexOf(value.ParameterName) >= 0)
      {
        throw new MySqlException(
            String.Format("Resources.ParameterAlreadyDefined", value.ParameterName));
      }
      else
      {
        string inComingName = value.ParameterName;
        if (inComingName[0] == '@' || inComingName[0] == '?')
          inComingName = inComingName.Substring(1, inComingName.Length - 1);
        if (IndexOf(inComingName) >= 0)
          throw new MySqlException(
              String.Format("Resources.ParameterAlreadyDefined", value.ParameterName));

	  }

      if (index == -1)
      {
        items.Add(value);
        index = items.Count - 1;
      }
      else
      {
        items.Insert(index, value);
        AdjustHashes(index, true);
      }

      indexHashCS.Add(value.ParameterName, index);
      indexHashCI.Add(value.ParameterName, index);

      value.Collection = this;
      return value;
    }

    private int GetNextIndex()
    {
      int index = Count+1;

      while (true)
      {
        string name = "Parameter" + index.ToString();
        if (!indexHashCI.ContainsKey(name)) break;
        index++;
      }
      return index;
    }

    private static void AdjustHash(Dictionary<string,int> hash, string parameterName, int keyIndex, bool addEntry)
    {
      if (!hash.ContainsKey(parameterName)) return;
      int index = (int)hash[parameterName];
      if (index < keyIndex) return;
      hash[parameterName] = addEntry ? ++index : --index;
    }

    /// <summary>
    /// This method will update all the items in the index hashes when
    /// we insert a parameter somewhere in the middle
    /// </summary>
    /// <param name="keyIndex"></param>
    /// <param name="addEntry"></param>
    private void AdjustHashes(int keyIndex, bool addEntry)
    {
      for (int i = 0; i < Count; i++)
      {
        string name = (items[i] as MySqlParameter).ParameterName;
        AdjustHash(indexHashCI, name, keyIndex, addEntry);
        AdjustHash(indexHashCS, name, keyIndex, addEntry);
      }
    }

    private MySqlParameter GetParameterFlexibleInternal(string baseName)
    {
      int index = IndexOf(baseName);
      if (-1 == index)
        index = IndexOf("?" + baseName);
      if (-1 == index)
        index = IndexOf("@" + baseName);
      if (-1 != index)
        return this[index];
      return null;
    }

    internal MySqlParameter GetParameterFlexible(string parameterName, bool throwOnNotFound)
    {
      string baseName = parameterName;
      MySqlParameter p = GetParameterFlexibleInternal(baseName);
      if (p != null) return p;

      if (parameterName.StartsWith("@", StringComparison.Ordinal) || parameterName.StartsWith("?", StringComparison.Ordinal))
        baseName = parameterName.Substring(1);
      p = GetParameterFlexibleInternal(baseName);
      if (p != null) return p;

      if (throwOnNotFound)
        throw new ArgumentException("Parameter '" + parameterName + "' not found in the collection.");
      return null;
    }
  }
}
