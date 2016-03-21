// Copyright © 2004, 2013, Oracle and/or its affiliates. All rights reserved.
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

namespace MySql.Data.MySqlClient.Memcached
{
  using System;
  using System.Collections.Generic;
  using System.Text;
  using System.IO;
  using System.Net;
  using MySql.Data.Common;
  

  /// <summary>
  /// An interface of the client memcached protocol. This class is abstract for 
  /// implementation of the Memcached client interface see <see cref="TextClient"/> for the 
  /// text protocol version and <see cref="BinaryClient"/> for the binary protocol version.
  /// </summary>
  public abstract class Client
  {
    /// <summary>
    /// The port used by the connection.
    /// </summary>
    protected uint port;

    /// <summary>
    /// The server DNS or IP address used by the connection.
    /// </summary>
    protected string server;

    /// <summary>
    /// The network stream used by the connecition.
    /// </summary>
    protected Stream stream;

    /// <summary>
    /// Factory method for creating  instances of <see cref="Client"/> that implement a connection with the requested features.
    /// The connection object returned must be explicitely opened see method <see cref="Client.Open"/>.
    /// </summary>
    /// <param name="server">The Memcached server DNS or IP address.</param>
    /// <param name="port">The port for the Memcached server</param>
    /// <param name="flags">A set of flags indicating characterestics requested.</param>
    /// <returns>An instance of a client connection ready to be used.</returns>
    public static Client GetInstance(string server, uint port, MemcachedFlags flags)
    {
      if ((flags | MemcachedFlags.TextProtocol) != 0)
      {
        return new TextClient(server, port);
      }
      else if ( ( flags | MemcachedFlags.BinaryProtocol ) != 0 )
      {
        return new BinaryClient(server, port);
      }
      return null;
    }

    /// <summary>
    /// Opens the client connection.
    /// </summary>
    public virtual void Open()
    {
      this.stream = StreamCreator.GetStream(server, port, null, 10, new DBVersion(), 60);
    }

    /// <summary>
    /// Closes the client connection.
    /// </summary>
    public virtual void Close()
    {
      stream.Dispose();
    }

    protected Client(string server, uint port)
    {
      this.server = server;
      this.port = port;
    }

    /// <summary>
    /// Adds a new key/value pair with the given TimeSpan expiration.
    /// </summary>
    /// <param name="key">The key for identifying the entry.</param>
    /// <param name="data">The data to associate with the key.</param>
    /// <param name="expiration">The interval of timespan, use TimeSpan.Zero for no expiration.</param>
    public abstract void Add(string key, object data, TimeSpan expiration);

    /// <summary>
    /// Appens the data to the existing data for the associated key.
    /// </summary>
    /// <param name="key">The key for identifying the entry.</param>
    /// <param name="data">The data to append with the data associated with the key.</param>
    public abstract void Append(string key, object data);

    /// <summary>
    /// Executes the Check-and-set Memcached operation.
    /// </summary>
    /// <param name="key">The key for identifying the entry.</param>
    /// <param name="data">The data to use in the CAS.</param>
    /// <param name="expiration">The interval of timespan, use TimeSpan.Zero for no expiration.</param>
    /// <param name="casUnique">The CAS unique value to use.</param>
    /// <exception cref="MemcachedException"></exception>
    public abstract void Cas(string key, object data, TimeSpan expiration, ulong casUnique);

    /// <summary>
    /// Decrements the value associated with a key by the given amount.
    /// </summary>
    /// <param name="key">The key associated with the value to decrement.</param>
    /// <param name="amount">The amount to decrement the value.</param>
    public abstract void Decrement(string key, int amount);

    /// <summary>
    /// Removes they pair key/value given the specified key.
    /// </summary>
    /// <param name="key"></param>
    public abstract void Delete(string key);

    /// <summary>
    /// Removes all entries from the storage, effectively invalidating the whole cache.
    /// </summary>
    /// <param name="delay">The interval after which the cache will be cleaned. Can be TimeSpan.Zero for immediately.</param>
    public abstract void FlushAll(TimeSpan delay);

    /// <summary>
    /// Get the key/value pair associated with a given key.
    /// </summary>
    /// <param name="key">The key for which to returm the key/value.</param>
    /// <returns>The key/value associated with the key or a MemcachedException if it does not exists.</returns>
    public abstract KeyValuePair<string, object> Get(string key);

    /// <summary>
    /// Increments the value associated with a key by the given amount.
    /// </summary>
    /// <param name="key">The key associated with the value to increment.</param>
    /// <param name="amount">The amount to increment the value.</param>
    public abstract void Increment(string key, int amount);

    /// <summary>
    /// Prepends the data to the existing data for the associated key.
    /// </summary>
    /// <param name="key">The key for identifying the entry.</param>
    /// <param name="data">The data to append with the data associated with the key.</param>
    public abstract void Prepend(string key, object data);

    /// <summary>
    /// Replaces the value associated with the given key with another value.
    /// </summary>
    /// <param name="key">The key for identifying the entry.</param>
    /// <param name="data">The data to replace the value associated with the key.</param>
    /// <param name="expiration">The interval of timespan, use TimeSpan.Zero for no expiration.</param>
    public abstract void Replace(string key, object data, TimeSpan expiration);

    /// <summary>
    /// Set the value of a given key.
    /// </summary>
    /// <param name="key">The key for identifying the entry.</param>
    /// <param name="data">The data to associate with the given key.</param>
    /// <param name="expiration">The interval of timespan, use TimeSpan.Zero for no expiration.</param>
    public abstract void Set(string key, object data, TimeSpan expiration);
  }

  /// <summary>
  /// A set of flags for requesting new instances of connections
  /// </summary>
  [Flags]
  public enum MemcachedFlags : ushort
  {
    /// <summary>
    /// Requests a connection implememting the text protocol.
    /// </summary>
    TextProtocol = 0x1,
    /// <summary>
    /// Requests a connection implementing the binary protocol.
    /// </summary>
    BinaryProtocol = 0x2,
    /// <summary>
    /// Requests a TCP connection. Currently UDP is not supported.
    /// </summary>
    Tcp = 0x4
  }
}
