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
using System.Collections.Generic;

namespace MySql.Data.Common
{
  internal class Cache<KeyType, ValueType>
  {
    private int _capacity;
    private Queue<KeyType> _keyQ;
    private Dictionary<KeyType, ValueType> _contents;

    public Cache(int initialCapacity, int capacity)
    {
      _capacity = capacity;
      _contents = new Dictionary<KeyType, ValueType>(initialCapacity);

      if (capacity > 0)
        _keyQ = new Queue<KeyType>(initialCapacity);
    }

    public ValueType this[KeyType key]
    {
      get
      {
        ValueType val;
        if (_contents.TryGetValue(key, out val))
          return val;
        else
          return default(ValueType);
      }
      set { InternalAdd(key, value); }
    }

    public void Add(KeyType key, ValueType value)
    {
      InternalAdd(key, value);
    }

    private void InternalAdd(KeyType key, ValueType value)
    {
      if (!_contents.ContainsKey(key))
      {

        if (_capacity > 0)
        {
          _keyQ.Enqueue(key);

          if (_keyQ.Count > _capacity)
            _contents.Remove(_keyQ.Dequeue());
        }
      }

      _contents[key] = value;
    }
  }
}
