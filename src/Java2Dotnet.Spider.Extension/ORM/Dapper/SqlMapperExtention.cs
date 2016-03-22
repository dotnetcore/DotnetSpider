//#if !NET_CORE

//using System;
//using System.Data;
//using System.Linq;
//using Dapper;
//using System.Data.Common;


//namespace Java2Dotnet.Spider.Extension.ORM.Dapper
//{
//	public static class SqlMapperExtention
//	{
//		#region util

//		public static long GetIdentity(this DbConnection conn, string primarykey, string tableName)
//		{
//			if (string.IsNullOrEmpty(primarykey)) primarykey = "id";
//			if (string.IsNullOrEmpty(tableName))
//			{
//				throw new ArgumentException("tableName参数不能为空，为查询的表名");
//			}
//			string query = $"SELECT max({primarykey}) as Id FROM {tableName}";
//			NewId identity = conn.Query<NewId>(query).Single();
//			return identity.Id;
//		}

//		public static long GetIdentity(this DbConnection conn, string primarykey, string tableName, DbTransaction transaction)
//		{
//			if (string.IsNullOrEmpty(primarykey)) primarykey = "id";
//			if (string.IsNullOrEmpty(tableName))
//			{
//				throw new ArgumentException("tableName参数不能为空，为查询的表名");
//			}
//			string query = $"SELECT max({primarykey}) as Id FROM {tableName}";
//			NewId identity = conn.Query<NewId>(query, null, transaction).Single();
//			return identity.Id;
//		}

//		private class NewId
//		{
//			// ReSharper disable once UnusedAutoPropertyAccessor.Local
//			public long Id { get; set; }
//		}

//		#endregion
//	}
//}

//#endif