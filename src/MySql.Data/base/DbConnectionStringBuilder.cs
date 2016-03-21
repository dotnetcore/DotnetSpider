// Copyright ?2004, 2013, Oracle and/or its affiliates. All rights reserved.
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Globalization;

namespace MySql.Data.MySqlClient
{
  public class DbConnectionStringBuilder : IDictionary, ICollection, IEnumerable
  {
    private Dictionary<string, object> hash;
    private bool browsable;

    public DbConnectionStringBuilder()
    {
      hash = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
      browsable = false;
    }

    #region Properties

    public bool BrowsableConnectionString
    {
      get { return browsable; }
      set { browsable = value; }
    }

    public string ConnectionString
    {
      get { return GetConnectionString(); }
      set
      {
        Clear();
        ParseConnectionString(value);
      }
    }

    public virtual object this[string key]
    {
      get { return hash[key]; }
      set { Add(key, value); }
    }

    #endregion

    #region IDictionary Members

    public void Add(object key, object value)
    {
      hash[key.ToString()] = value;
    }

    public virtual void Clear()
    {
      hash.Clear();
    }

    public bool Contains(object key)
    {
      return hash.ContainsKey(key.ToString());
    }

    public IDictionaryEnumerator GetEnumerator()
    {
      return hash.GetEnumerator();
    }

    public bool IsFixedSize
    {
      get { return false; }
    }

    public bool IsReadOnly
    {
      get { return false; }
    }

    public ICollection Keys
    {
      get { return hash.Keys; }
    }

    public void Remove(object key)
    {
      hash.Remove(key.ToString());
    }

    public ICollection Values
    {
      get { return hash.Values; }
    }

    public object this[object key]
    {
      get
      {
        return this[(string)key];
      }
      set
      {
        this[(string)key] = value;
      }
    }

    #endregion

    #region ICollection Members

    void ICollection.CopyTo(Array array, int index)
    {
      throw new NotSupportedException();
    }

    public int Count
    {
      get { return hash.Count; }
    }

    public bool IsSynchronized
    {
      get { throw new NotSupportedException(); }
    }

    public object SyncRoot
    {
      get { throw new NotSupportedException(); }
    }

    #endregion

    #region IEnumerable Members

    IEnumerator IEnumerable.GetEnumerator()
    {
      return hash.GetEnumerator();
    }

    #endregion

    public virtual bool Remove(string key)
    {
      this.Remove((object)key);
      return !Contains(key);
    }

    public virtual object TryGetValue(string keyword, out object value)
    {
      if (!hash.ContainsKey(keyword))
      {
        value = null;
        return false;
      }
      value = hash[keyword];
      return true;
    }

    private void ParseConnectionString(string connectString)
    {
      if (connectString == null) return;

      StringBuilder key = new StringBuilder();
      StringBuilder value = new StringBuilder();
      bool keyDone = false;

      foreach (char c in connectString)
      {
        if (c == '=')
          keyDone = true;
        else if (c == ';')
        {
          string keyStr = key.ToString().Trim();
          string valueStr = value.ToString().Trim();
          valueStr = CleanValue(valueStr);
          if (keyStr.Length > 0)
            this[keyStr] = valueStr;
          keyDone = false;
          key.Remove(0, key.Length);
          value.Remove(0, value.Length);
        }
        else if (keyDone)
          value.Append(c);
        else
          key.Append(c);
      }

      if (key.Length == 0) return;
      this[key.ToString().Trim()] = CleanValue(value.ToString().Trim());
    }

    private string CleanValue(string value)
    {
      if ((value.StartsWith("'") && value.EndsWith("'")) ||
          (value.StartsWith("\"") && value.EndsWith("\"")))
      {
        value = value.Substring(1);
        value = value.Substring(0, value.Length - 1);
      }
      return value;
    }

    private string GetConnectionString()
    {
      StringBuilder builder = new StringBuilder();
      string delimiter = "";
      foreach (string key in this.Keys)
      {
        builder.AppendFormat(CultureInfo.CurrentCulture, "{0}{1}={2}",
            delimiter, key, this[key]);
        delimiter = ";";
      }
      return builder.ToString();
    }

    public virtual bool ContainsKey(string keyword)
    {
      return hash.ContainsKey(keyword); 
    }

    public string ToString()
    {
      return GetConnectionString();
    }

    /*        private void ParseConnectionString(string value)
            {
                String[] keyvalues = src.Split(';');
                String[] newkeyvalues = new String[keyvalues.Length];
                int x = 0;

                // first run through the array and check for any keys that
                // have ; in their value
                foreach (String keyvalue in keyvalues)
                {
                    // check for trailing ; at the end of the connection string
                    if (keyvalue.Length == 0) continue;

                    // this value has an '=' sign so we are ok
                    if (keyvalue.IndexOf('=') >= 0)
                    {
                        newkeyvalues[x++] = keyvalue;
                    }
                    else
                    {
                        newkeyvalues[x - 1] += ";";
                        newkeyvalues[x - 1] += keyvalue;
                    }
                }

                Hashtable hash = new Hashtable();

                // now we run through our normalized key-values, splitting on equals
                for (int y = 0; y < x; y++)
                {
                    String[] parts = newkeyvalues[y].Split('=');

                    // first trim off any space and lowercase the key
                    parts[0] = parts[0].Trim().ToLower();
                    parts[1] = parts[1].Trim();

                    // we also want to clear off any quotes
                    if (parts[1].Length >= 2)
                    {
                        if ((parts[1][0] == '"' && parts[1][parts[1].Length - 1] == '"') ||
                            (parts[1][0] == '\'' && parts[1][parts[1].Length - 1] == '\''))
                        {
                            parts[1] = parts[1].Substring(1, parts[1].Length - 2);
                        }
                    }
                    else
                    {
                        parts[1] = parts[1];
                    }
                    parts[0] = parts[0].Trim('\'', '"');

                    hash[parts[0]] = parts[1];
                }
                return hash;
            }*/
  }
}
