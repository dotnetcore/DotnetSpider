using System;

namespace DotnetSpider.Http
{
	public static class HttpUtilities
	{
		public static bool IsHttpUri(Uri uri) => IsSupportedScheme(uri.Scheme);

		public static bool IsSupportedScheme(string scheme) => IsSupportedNonSecureScheme(scheme) ||
		                                                       IsSupportedSecureScheme(scheme);

		public static bool IsSupportedNonSecureScheme(string scheme) =>
			string.Equals(scheme, "http", StringComparison.OrdinalIgnoreCase) ||
			IsNonSecureWebSocketScheme(scheme);

		public static bool IsSupportedSecureScheme(string scheme) =>
			string.Equals(scheme, "https", StringComparison.OrdinalIgnoreCase) ||
			IsSecureWebSocketScheme(scheme);

		public static bool IsNonSecureWebSocketScheme(string scheme) =>
			string.Equals(scheme, "ws", StringComparison.OrdinalIgnoreCase);

		public static bool IsSecureWebSocketScheme(string scheme) =>
			string.Equals(scheme, "wss", StringComparison.OrdinalIgnoreCase);
	}
}
