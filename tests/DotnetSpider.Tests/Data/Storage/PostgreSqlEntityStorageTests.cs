//using System.Data;
//using DotnetSpider.Data.Storage;
//using MySql.Data.MySqlClient;
//using Npgsql;
//using Xunit;
//
//namespace DotnetSpider.Tests.Data.Storage
//{
//	public class PostgreSqlEntityStorageTests : MySqlEntityStorageTests
//	{
//		protected override string Escape => "\"";
//		
//		protected override string GetConnectionString()
//		{
//			return "Server=localhost;Username=postgres;Password=1qazZAQ!;Database=postgres;Timeout=0;Command Timeout=0";
//		}
//
//		protected override IDbConnection CreateConnection()
//		{
//			return new NpgsqlConnection(GetConnectionString());
//		}
//		
//		protected override StorageBase CreateStorage(StorageType type)
//		{
//			return new PostgreSqlEntityStorage(type, GetConnectionString());
//		}
//	}
//}