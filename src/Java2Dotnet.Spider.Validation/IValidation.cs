namespace Java2Dotnet.Spider.Validation
{
	public interface IValidation
	{
		ValidateResult Validate();
		void CheckArguments();
	}
}
