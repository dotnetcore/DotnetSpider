using Java2Dotnet.Spider.Core;

namespace Java2Dotnet.Spider.Extension.Model
{
	/// <summary>
	/// Interface to be implemented by page models that need to do something after fields are extracted.
	/// </summary>
	public interface IAfterExtractor
	{
		void AfterProcess(Page page);
	}
}
