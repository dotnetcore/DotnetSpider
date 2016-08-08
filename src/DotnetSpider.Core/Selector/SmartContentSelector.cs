using System;
using System.Collections.Generic;
using System.Text;

namespace DotnetSpider.Core.Selector
{
	public class SmartContentSelector : ISelector
	{
		public dynamic Select(dynamic node)
		{
			string html = node ;
			html = html.Replace("(?is)<!DOCTYPE.*?>", "");
			html = html.Replace("(?is)<!--.*?-->", "");             // remove html comment
			html = html.Replace("(?is)<script.*?>.*?</script>", ""); // remove javascript
			html = html.Replace("(?is)<style.*?>.*?</style>", "");   // remove css
			html = html.Replace("&.{2,5};|&#.{2,5};", " ");         // remove special char
			html = html.Replace("(?is)<.*?>", "");
			List<string> lines;
			int blocksWidth = 3;
			int threshold = 86;
			int start;
			int end;
			StringBuilder text = new StringBuilder();
			IList<int> indexDistribution = new List<int>();

			lines = new List<string>(html.Split('\n'));

			for (int i = 0; i < lines.Count - blocksWidth; i++)
			{
				int wordsNum = 0;
				for (int j = i; j < i + blocksWidth; j++)
				{
					lines[j] = lines[j].Replace("\\s+", "");
					wordsNum += lines[j].Length;
				}
				indexDistribution.Add(wordsNum);
			}

			start = -1; end = -1;
			bool boolstart = false, boolend = false;

			for (int i = 0; i < indexDistribution.Count - 1; i++)
			{
				if (indexDistribution[i] > threshold && !boolstart)
				{
					if (indexDistribution[i + 1] != 0 || indexDistribution[i + 2] != 0 || indexDistribution[i + 3] != 0)
					{
						boolstart = true;
						start = i;
						continue;
					}
				}
				if (boolstart)
				{
					if (indexDistribution[i] == 0 || indexDistribution[i + 1] == 0)
					{
						end = i;
						boolend = true;
					}
				}
				StringBuilder tmp = new StringBuilder();
				if (boolend)
				{
					for (int ii = start; ii <= end; ii++)
					{
						if (lines[ii].Length < 5)
						{
							continue;
						}
						tmp.Append(lines[ii] + "\n");
					}
					string str = tmp.ToString();

					if (str.Contains("Copyright")) continue;
					text.Append(str);
					boolstart = boolend = false;
				}
			}
			return  text.ToString();
		}

		public List<dynamic> SelectList(dynamic text)
		{
			throw new NotImplementedException();
		}
	}
}
