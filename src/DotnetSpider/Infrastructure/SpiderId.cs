using System;

namespace DotnetSpider.Infrastructure
{
	public readonly struct SpiderId
	{
		public readonly string Id;
		public readonly string Name;

		public SpiderId(string id, string name)
		{
			id.NotNullOrWhiteSpace("Id");
			if (id.Length > 36)
			{
				throw new ArgumentException("Id 长度不能超过 36 个字符");
			}

			Id = id;
			Name = name;
		}

		public override string ToString()
		{
			return Id;
		}
	}
}
