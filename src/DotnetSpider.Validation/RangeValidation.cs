using System;
using System.Data;
using System.Data.Common;

namespace DotnetSpider.Validation
{
	public class RangeValidation : AbstractValidation
	{
		public RangeValidation(DbConnection conn, string sql, string arguments, string description, ValidateLevel level = ValidateLevel.Error) : base(conn, sql, arguments, description, level)
		{
		}

		public float Min { get; set; }
		public float Max { get; set; }

		public override ValidateResult Validate()
		{
			try
			{
				string tmpValue = GetValue();

				float value;
				if (!float.TryParse(tmpValue, out value))
				{
					throw new Exception($"Value {tmpValue} 不能转换成 float");
				}
				return new ValidateResult
				{
					IsPass = value >= Min && value <= Max,
					Message = "Success",
					Level = Level,
					Description = Description,
					Sql = Sql,
					Arguments = Arguments,
					ActualValue = tmpValue
				};
			}
			catch (Exception e)
			{
				return new ValidateResult
				{
					IsPass = false,
					Message = e.Message,
					Level = Level,
					Description = Description,
					Sql = Sql,
					Arguments = Arguments
				};
			}
		}

		public override void CheckArguments()
		{
			base.CheckArguments();

			string[] values = Arguments.Split(',');
			Min = float.Parse(values[0]);
			Max = float.Parse(values[1]);
		}

		public override string ToString()
		{
			return $"Min: {Min} Max: {Max}";
		}
	}
}
