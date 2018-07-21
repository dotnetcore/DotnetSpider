namespace DotnetSpider.Core.Processor
{
	public interface IBeforeProcessorHandler
	{
		void Handle(ref Page page);
	}
}
