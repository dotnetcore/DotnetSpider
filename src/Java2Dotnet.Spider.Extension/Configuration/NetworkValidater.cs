using System;

namespace Java2Dotnet.Spider.Extension.Configuration
{
	public abstract class NetworkValidater
	{
		[Flags]
		public enum Types
		{
			Defalut,
			Vps
		}

		public abstract Types Type { get; internal set; }
	}

	public class DefaultNetworkValidater : NetworkValidater
	{
		public override Types Type { get; internal set; } = Types.Defalut;
	}

	public class VpsNetworkValidater : NetworkValidater
	{
		public override Types Type { get; internal set; } = Types.Vps;

		public int InterfaceNum { get; set; } = 2;
	}
}
