using System.Collections.Generic;

namespace DotnetSpider.Core
{
	/// <summary>
	/// Object contains extract results. 
	/// It is contained in Page and will be processed in pipeline.
	/// </summary>
	public class ResultItems
	{
		private readonly Dictionary<string, dynamic> _fields = new Dictionary<string, dynamic>();
		private readonly object _locker = new object();

		public const string CountOfResultsKey = "__CountOfResultsKey";
		public const string CountOfEffectedRows = "__CountOfEffectedRowsKey";

		public Dictionary<string, dynamic> Results
		{
			get
			{
				lock (_locker)
				{
					return _fields;
				}
			}
		}

		public Request Request { get; set; }

		/// <summary>
		/// Whether to skip the result. 
		/// Result which is skipped will not be processed by Pipeline.
		/// </summary>
		public bool IsEmpty => _fields.Count == 0;

		public dynamic GetResultItem(string key)
		{
			lock (_locker)
			{
				return _fields.ContainsKey(key) ? _fields[key] : null;
			}
		}

		public ResultItems AddOrUpdateResultItem(string key, dynamic value)
		{
			lock (_locker)
			{
				if (_fields.ContainsKey(key))
				{
					_fields[key] = value;
				}
				else
				{
					_fields.Add(key, value);
				}
				return this;
			}
		}
	}
}