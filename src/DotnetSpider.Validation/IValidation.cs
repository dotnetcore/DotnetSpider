namespace DotnetSpider.Validation
{
	public interface IValidation
	{
		ValidateResult Validate();
		void CheckArguments();
	}
}
