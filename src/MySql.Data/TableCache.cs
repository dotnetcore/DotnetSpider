// Copyright ?2011, Oracle and/or its affiliates. All rights reserved.
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
using System.Collections.Generic;

using System.Diagnostics;
using System.Text;
using System.Globalization;

namespace MySql.Data.MySqlClient
{
  internal class TableCache
  {
    private static BaseTableCache cache;

    static TableCache()
    {
      cache = new BaseTableCache(480 /* 8 hour max by default */);
    }

    public static void AddToCache(string commandText, ResultSet resultSet)
    {
      cache.AddToCache(commandText, resultSet);
    }

    public static ResultSet RetrieveFromCache(string commandText, int cacheAge)
    {
      return (ResultSet)cache.RetrieveFromCache(commandText, cacheAge);
    }

    public static void RemoveFromCache(string commandText)
    {
      cache.RemoveFromCache(commandText);
    }

    public static void DumpCache()
    {
      cache.Dump();
    }
  }

  public class BaseTableCache
  {
    protected int MaxCacheAge;
    private Dictionary<string, CacheEntry> cache = new Dictionary<string, CacheEntry>();

    public BaseTableCache(int maxCacheAge)
    {
      MaxCacheAge = maxCacheAge;
    }

    public virtual void AddToCache(string commandText, object resultSet)
    {
      CleanCache();
      CacheEntry entry = new CacheEntry();
      entry.CacheTime = DateTime.Now;
      entry.CacheElement = resultSet;
      lock (cache)
      {
        if (cache.ContainsKey(commandText)) return;
        cache.Add(commandText, entry);
      }
    }

    public virtual object RetrieveFromCache(string commandText, int cacheAge)
    {
      CleanCache();
      lock (cache)
      {
        if (!cache.ContainsKey(commandText)) return null;
        CacheEntry entry = cache[commandText];
        if (DateTime.Now.Subtract(entry.CacheTime).TotalSeconds > cacheAge) return null;
        return entry.CacheElement;
      }
    }

    public void RemoveFromCache(string commandText)
    {
      lock (cache)
      {
        if (!cache.ContainsKey(commandText)) return;
        cache.Remove(commandText);
      }
    }

    public virtual void Dump()
    {
      lock (cache)
        cache.Clear();
    }

    protected virtual void CleanCache()
    {
      DateTime now = DateTime.Now;
      List<string> keysToRemove = new List<string>();

      lock (cache)
      {
        foreach (string key in cache.Keys)
        {
          TimeSpan diff = now.Subtract(cache[key].CacheTime);
          if (diff.TotalSeconds > MaxCacheAge)
            keysToRemove.Add(key);
        }

        foreach (string key in keysToRemove)
          cache.Remove(key);
      }
    }

    private struct CacheEntry
    {
      public DateTime CacheTime;
      public object CacheElement;
    }
  }

}