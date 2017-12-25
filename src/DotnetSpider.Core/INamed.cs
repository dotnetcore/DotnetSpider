namespace DotnetSpider.Core
{
	/// <summary>
	/// 名称接口定义
	/// </summary>
	public interface INamed
	{
		/// <summary>
		/// 名称
		/// </summary>
		string Name { get; set; }
	}

	public abstract class Named : INamed
	{
		private string _name;
		private static object NameGetOrSetLocker = new object();

		public string Name
		{
			get
			{
				lock (NameGetOrSetLocker)
				{
					if (string.IsNullOrEmpty(_name))
					{
						_name = GetType().Name;
					}
				}
				return _name;
			}
			set
			{
				lock (NameGetOrSetLocker)
				{
					_name = value;
				}
			}
		}
	}
}
