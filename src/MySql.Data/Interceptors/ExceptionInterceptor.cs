// Copyright © 2004, 2011, Oracle and/or its affiliates. All rights reserved.
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
using System.Text;

using System.Reflection;

namespace MySql.Data.MySqlClient
{
  /// <summary>
  /// BaseExceptionInterceptor is the base class that should be used for all userland 
  /// exception interceptors
  /// </summary>
  public abstract class BaseExceptionInterceptor
  {
    public abstract Exception InterceptException(Exception exception);

    protected MySqlConnection ActiveConnection { get; private set; }

    public virtual void Init(MySqlConnection connection)
    {
      ActiveConnection = connection;
    }
  }

  /// <summary>
  /// StandardExceptionInterceptor is the standard interceptor that simply throws the exception.
  /// It is the default action.
  /// </summary>
  internal sealed class StandardExceptionInterceptor : BaseExceptionInterceptor
  {
    public override Exception InterceptException(Exception exception)
    {
      return exception;
    }
  }

  /// <summary>
  /// ExceptionInterceptor is the "manager" class that keeps the list of registered interceptors
  /// for the given connection.
  /// </summary>
  internal sealed class ExceptionInterceptor : Interceptor
  {
    List<BaseExceptionInterceptor> interceptors = new List<BaseExceptionInterceptor>();

    public ExceptionInterceptor(MySqlConnection connection) 
    {
      this.connection = connection;

      LoadInterceptors(connection.Settings.ExceptionInterceptors);

      // we always have the standard interceptor
      interceptors.Add(new StandardExceptionInterceptor());

    }

    protected override void AddInterceptor(object o)
    {
      if (o == null)
        throw new ArgumentException(String.Format("Unable to instantiate ExceptionInterceptor"));

      if (!(o is BaseExceptionInterceptor))
        throw new InvalidOperationException(String.Format("Resources.TypeIsNotExceptionInterceptor",

		  o.GetType()));
      BaseExceptionInterceptor ie = o as BaseExceptionInterceptor;
      ie.Init(connection);
      interceptors.Insert(0, (BaseExceptionInterceptor)o);
    }

    public void Throw(Exception exception)
    {
      Exception e = exception;
      foreach (BaseExceptionInterceptor ie in interceptors)
      {
        e = ie.InterceptException(e);
      }
      throw e;
    }
 
  }
}
