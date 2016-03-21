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
//using System.Diagnostics;


//namespace MySql.Data.MySqlClient
//{
//  internal class SystemPerformanceMonitor : PerformanceMonitor
//  {
//    private static PerformanceCounter procedureHardQueries;
//    private static PerformanceCounter procedureSoftQueries;

//    public SystemPerformanceMonitor(MySqlConnection connection) : base(connection)
//    {
//      string categoryName = Resources.PerfMonCategoryName;

//      if (connection.Settings.UsePerformanceMonitor && procedureHardQueries == null)
//      {
//        try
//        {
//          procedureHardQueries = new PerformanceCounter(categoryName,
//                                                        "HardProcedureQueries", false);
//          procedureSoftQueries = new PerformanceCounter(categoryName,
//                                                        "SoftProcedureQueries", false);
//        }
//        catch (Exception ex)
//        {
//          MySqlTrace.LogError(connection.ServerThread, ex.Message);
//        }
//      }
//    }

//#if DEBUG
//    private void EnsurePerfCategoryExist()
//    {
//      CounterCreationDataCollection ccdc = new CounterCreationDataCollection();
//      CounterCreationData ccd = new CounterCreationData();
//      ccd.CounterType = PerformanceCounterType.NumberOfItems32;
//      ccd.CounterName = "HardProcedureQueries";
//      ccdc.Add(ccd);

//      ccd = new CounterCreationData();
//      ccd.CounterType = PerformanceCounterType.NumberOfItems32;
//      ccd.CounterName = "SoftProcedureQueries";
//      ccdc.Add(ccd);

//      if (!PerformanceCounterCategory.Exists(Resources.PerfMonCategoryName))
//        PerformanceCounterCategory.Create(Resources.PerfMonCategoryName, null, ccdc);
//    }
//#endif

//    public void AddHardProcedureQuery()
//    {
//      if (!Connection.Settings.UsePerformanceMonitor ||
//          procedureHardQueries == null) return;
//      procedureHardQueries.Increment();
//    }

//    public void AddSoftProcedureQuery()
//    {
//      if (!Connection.Settings.UsePerformanceMonitor ||
//          procedureSoftQueries == null) return;
//      procedureSoftQueries.Increment();
//    }
//  }
//}