using System;

namespace DotnetSpider.Core.Pipeline
{
	/// <summary>
	/// Pipeline is the persistent and offline process part of crawler. 
	/// The interface Pipeline can be implemented to customize ways of persistent.
	/// </summary>
	public interface IPipeline : IDisposable
	{
		ISpider Spider { get; }

		void InitPipeline(ISpider spider);

		/// <summary>
		/// Process extracted results.
		/// </summary>
		/// <param name="resultItems"></param>
		void Process(params ResultItems[] resultItems);
	}
}