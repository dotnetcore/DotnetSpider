//// Copyright ?2004, 2010, Oracle and/or its affiliates. All rights reserved.
////
//// MySQL Connector/NET is licensed under the terms of the GPLv2
//// <http://www.gnu.org/licenses/old-licenses/gpl-2.0.html>, like most 
//// MySQL Connectors. There are special exceptions to the terms and 
//// conditions of the GPLv2 as it is applied to this software, see the 
//// FLOSS License Exception
//// <http://www.mysql.com/about/legal/licensing/foss-exception.html>.
////
//// This program is free software; you can redistribute it and/or modify 
//// it under the terms of the GNU General Public License as published 
//// by the Free Software Foundation; version 2 of the License.
////
//// This program is distributed in the hope that it will be useful, but 
//// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
//// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
//// for more details.
////
//// You should have received a copy of the GNU General Public License along 
//// with this program; if not, write to the Free Software Foundation, Inc., 
//// 51 Franklin St, Fifth Floor, Boston, MA 02110-1301  USA

//using System;
 
//using System.Collections;
//using System.Collections.Generic;
//using System.Data;
//using System.Threading;

//namespace MySql.Data.MySqlClient
//{
//  /// <summary>
//  /// Represents a single(not nested) TransactionScope
//  /// </summary>
//  internal class MySqlTransactionScope
//  {
//    public MySqlConnection connection;
//    public Transaction baseTransaction;
//    public MySqlTransaction simpleTransaction;
//    public int rollbackThreadId;

//    public MySqlTransactionScope(MySqlConnection con, Transaction trans,
//        MySqlTransaction simpleTransaction)
//    {
//      connection = con;
//      baseTransaction = trans;
//      this.simpleTransaction = simpleTransaction;
//    }

//    public void Rollback(SinglePhaseEnlistment singlePhaseEnlistment)
//    {
//      // prevent commands in main thread to run concurrently
//      Driver driver = connection.driver;
//      lock (driver)
//      {
//        rollbackThreadId = Thread.CurrentThread.ManagedThreadId;
//        while (connection.Reader != null)
//        {
//          // wait for reader to finish. Maybe we should not wait 
//          // forever and cancel it after some time?
//          System.Threading.Thread.Sleep(100);
//        }
//        simpleTransaction.Rollback();
//        singlePhaseEnlistment.Aborted();
//        DriverTransactionManager.RemoveDriverInTransaction(baseTransaction);

//        driver.CurrentTransaction = null;

//        if (connection.State == ConnectionState.Closed)
//          connection.CloseFully();
//        rollbackThreadId = 0;
//      }
//    }

//    public void SinglePhaseCommit(SinglePhaseEnlistment singlePhaseEnlistment)
//    {
//      simpleTransaction.Commit();
//      singlePhaseEnlistment.Committed();
//      DriverTransactionManager.RemoveDriverInTransaction(baseTransaction);
//      connection.driver.CurrentTransaction = null;

//      if (connection.State == ConnectionState.Closed)
//        connection.CloseFully();
//    }
//  }

//  internal sealed class MySqlPromotableTransaction : IPromotableSinglePhaseNotification, ITransactionPromoter
//  {
//    // Per-thread stack to manage nested transaction scopes
//    [ThreadStatic]
//    static Stack<MySqlTransactionScope> globalScopeStack;

//    MySqlConnection connection;
//    Transaction baseTransaction;
//    Stack<MySqlTransactionScope> scopeStack;


//    public MySqlPromotableTransaction(MySqlConnection connection, Transaction baseTransaction)
//    {
//      this.connection = connection;
//      this.baseTransaction = baseTransaction;
//    }

//    public Transaction BaseTransaction
//    {
//      get
//      {
//        if (scopeStack.Count > 0)
//          return scopeStack.Peek().baseTransaction;
//        else
//          return null;
//      }
//    }

//    public bool InRollback
//    {
//      get
//      {
//        if (scopeStack.Count > 0)
//        {
//          MySqlTransactionScope currentScope = scopeStack.Peek();
//          if (currentScope.rollbackThreadId == Thread.CurrentThread.ManagedThreadId)
//          {
//            return true;
//          }
//        }
//        return false;
//      }
//    }
//    void IPromotableSinglePhaseNotification.Initialize()
//    {
//      string valueName = Enum.GetName(
//      typeof(System.Transactions.IsolationLevel), baseTransaction.IsolationLevel);
//      System.Data.IsolationLevel dataLevel = (System.Data.IsolationLevel)Enum.Parse(
//           typeof(System.Data.IsolationLevel), valueName);
//      MySqlTransaction simpleTransaction = connection.BeginTransaction(dataLevel);

//      // We need to save the per-thread scope stack locally.
//      // We cannot always use thread static variable in rollback: when scope
//      // times out, rollback is issued by another thread.
//      if (globalScopeStack == null)
//      {
//        globalScopeStack = new Stack<MySqlTransactionScope>();
//      }

//      scopeStack = globalScopeStack;
//      scopeStack.Push(new MySqlTransactionScope(connection, baseTransaction,
//         simpleTransaction));
//    }

//    void IPromotableSinglePhaseNotification.Rollback(SinglePhaseEnlistment singlePhaseEnlistment)
//    {

//      MySqlTransactionScope current = scopeStack.Peek();
//      current.Rollback(singlePhaseEnlistment);
//      scopeStack.Pop();
//    }

//    void IPromotableSinglePhaseNotification.SinglePhaseCommit(SinglePhaseEnlistment singlePhaseEnlistment)
//    {
//      scopeStack.Pop().SinglePhaseCommit(singlePhaseEnlistment);
//    }

//    byte[] ITransactionPromoter.Promote()
//    {
//      throw new NotSupportedException();
//    }
//  }

//  internal class DriverTransactionManager
//  {
//    private static Hashtable driversInUse = new Hashtable();

//    public static Driver GetDriverInTransaction(Transaction transaction)
//    {
//      lock (driversInUse.SyncRoot)
//      {
//        Driver d = (Driver)driversInUse[transaction.GetHashCode()];
//        return d;
//      }
//    }

//    public static void SetDriverInTransaction(Driver driver)
//    {
//      lock (driversInUse.SyncRoot)
//      {
//        driversInUse[driver.CurrentTransaction.BaseTransaction.GetHashCode()] = driver;
//      }
//    }

//    public static void RemoveDriverInTransaction(Transaction transaction)
//    {
//      lock (driversInUse.SyncRoot)
//      {
//        driversInUse.Remove(transaction.GetHashCode());
//      }
//    }
//  }
//}

