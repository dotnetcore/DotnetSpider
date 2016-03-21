namespace Java2Dotnet.Spider.Validation
{
	public class ValidateResult
	{
		public bool IsPass { get; set; }
		public string Message { get; set; }
		public string Arguments { get; set; }
		public string Description { get; set; }
		public string Sql { get; set; }
		public string ActualValue { get; set; }
		public ValidateLevel Level { get; set; }
	}
}
