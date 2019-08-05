using System;
using System.Data.Common;
using System.IO;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Quartz.Impl.AdoJobStore.Common;

namespace DotnetSpider.Portal
{
	public class MySqlDbProvider : IDbProvider
	{
		public string Provider { get; set; }

		public MySqlDbProvider()
		{
			var configurationBuilder = new ConfigurationBuilder();
			if (File.Exists("appsettings.json"))
			{
				configurationBuilder.AddJsonFile("appsettings.json");
			}

			configurationBuilder.AddCommandLine(Environment.GetCommandLineArgs());
			configurationBuilder.AddEnvironmentVariables();
			var configuration = configurationBuilder.Build();
			var ds = configuration["quartz.jobStore.dataSource"];
			ConnectionString = configuration[$"quartz.dataSource.{ds}.connectionString"];
			var metadata = new MySqlMetadata(configuration);
			metadata.Init2();
			Metadata = metadata;
		}

		public void Initialize()
		{
		}

		public DbCommand CreateCommand()
		{
			return new MySqlCommand();
		}

		public DbConnection CreateConnection()
		{
			return new MySqlConnection(ConnectionString);
		}

		public void Shutdown()
		{
		}

		public string ConnectionString { get; set; }

		public DbMetadata Metadata { get; }
	}

	public class MySqlMetadata : DbMetadata
	{
		private readonly IConfiguration _configuration;
		private Enum _dbBinaryType;

		public MySqlMetadata(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public void Init2()
		{
			var dbBinaryTypeName = string.IsNullOrWhiteSpace(
				_configuration["quartz.dbprovider.MySql.dbBinaryTypeName"])
				? "Blob"
				: _configuration["quartz.dbprovider.MySql.dbBinaryTypeName"];
			_dbBinaryType = (Enum)Enum.Parse(Type.GetType(_configuration["quartz.dbprovider.MySql.parameterDbType"]),
				dbBinaryTypeName);

			DbBinaryTypeName = dbBinaryTypeName;

			var parameterDbTypePropertyName =
				string.IsNullOrWhiteSpace(_configuration["quartz.dbprovider.MySql.parameterDbTypePropertyName"])
					? "MySqlDbType"
					: _configuration["quartz.dbprovider.MySql.parameterDbTypePropertyName"].Trim();
			ParameterDbTypePropertyName = parameterDbTypePropertyName;
			ConnectionType = Type.GetType(_configuration["quartz.dbprovider.MySql.connectionType"]);
			CommandType = Type.GetType(_configuration["quartz.dbprovider.MySql.commandType"]);
			ParameterDbType = Type.GetType(_configuration["quartz.dbprovider.MySql.parameterDbType"]);
			ParameterType = Type.GetType(_configuration["quartz.dbprovider.MySql.parameterType"]);
			ParameterDbTypeProperty = ParameterType.GetProperty(parameterDbTypePropertyName);
			if (ParameterDbTypeProperty == null)
			{
				throw new ArgumentException($"Couldn't parse parameter db type for database type '{ProductName}'");
			}
			ExceptionType = Type.GetType(_configuration["quartz.dbprovider.MySql.exceptionType"]);
		}

		public override string AssemblyName => _configuration["quartz.dbprovider.MySql.assemblyName"];

		public override string ProductName => "MySQL, MySQL provider";

		public override string ParameterNamePrefix =>
			string.IsNullOrWhiteSpace(_configuration["quartz.dbprovider.MySql.parameterNamePrefix"])
				? "?"
				: _configuration["quartz.dbprovider.MySql.parameterNamePrefix"].Trim();

		public override bool BindByName =>
			string.IsNullOrWhiteSpace(_configuration["quartz.dbprovider.MySql.bindByName"]) ||
			bool.Parse(_configuration["quartz.dbprovider.MySql.bindByName"].Trim());


		public override Enum DbBinaryType => _dbBinaryType;
	}
}
