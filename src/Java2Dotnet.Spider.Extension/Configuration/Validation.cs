using Java2Dotnet.Spider.Validation;
using System;
using System.Collections.Generic;
using System.Data.Common;

namespace Java2Dotnet.Spider.Extension.Configuration
{
	public class Validations
	{
		public string Corporation { get; set; }
		public string EmailUser { get; set; }
		public string EmailPassword { get; set; }
		public string EmailSmtpServer { get; set; }
		public int EmailSmtpPort { get; set; } = 25;
		public string EmailTo { get; set; }
		public string ConnectString { get; set; }
		public DataSource Source { get; set; } = DataSource.MySql;
		public List<Validation> Rules { get; set; }

		internal List<IValidation> GetValidations()
		{
			if (string.IsNullOrEmpty(ConnectString) || string.IsNullOrEmpty(EmailTo) || string.IsNullOrEmpty(EmailPassword) || string.IsNullOrEmpty(EmailSmtpServer) || string.IsNullOrEmpty(EmailUser))
			{
				return null;
			}

			var conn = DataSourceUtil.GetConnection(Source, ConnectString);
			List<IValidation> results = new List<IValidation>();
			foreach (var rule in Rules)
			{
				results.Add(rule.GetValidation(conn));
			}
			return results;
		}
	}

	public abstract class Validation
	{
		[Flags]
		public enum Types
		{
			Equal,
			Range
		}

		public string Arguments { get; set; }
		public string Sql { get; set; }
		public string Description { get; set; }
		public ValidateLevel Level { get; set; }

		public abstract Types Type { get; internal set; }

		public abstract IValidation GetValidation(DbConnection conn);
	}

	public class EqualValidation : Validation
	{
		public override Types Type { get; internal set; } = Types.Equal;

		public override IValidation GetValidation(DbConnection conn)
		{
			return new Spider.Validation.EqualValidation(conn, Sql, Arguments, Description, Level);
		}
	}

	public class RangeValidation : Validation
	{
		public override Types Type { get; internal set; } = Types.Equal;

		public override IValidation GetValidation(DbConnection conn)
		{
			return new Spider.Validation.RangeValidation(conn, Sql, Arguments, Description, Level);
		}
	}
}