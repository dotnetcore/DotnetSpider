using System.Collections.Generic;
using Java2Dotnet.Spider.Core;

namespace Java2Dotnet.Spider.Extension.Model
{
	public interface IEntityExtractor
	{
		dynamic Process(Page page);
		string EntityName { get; }
	}
}
