using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetSpider.Core
{
	public interface IExecuteRecord
	{
		 
        bool Add(string taskId, string name, string identity);
        void Remove(string taskId, string name, string identity);

    }
}
