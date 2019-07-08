using System.Diagnostics;
using System.Security.Claims;
using System.Security.Principal;

namespace DotnetSpider.Portal.Common
{
	/// <summary>
	/// Extension methods for <see cref="System.Security.Principal.IPrincipal"/> and <see cref="System.Security.Principal.IIdentity"/> .
	/// </summary>
	public static class PrincipalExtensions
	{
		/// <summary>
		/// Gets the name.
		/// </summary>
		/// <param name="principal">The principal.</param>
		/// <returns></returns>
		[DebuggerStepThrough]
		public static string GetDisplayName(this ClaimsPrincipal principal)
		{
			var name = principal.Identity.Name;
			if (!string.IsNullOrWhiteSpace(name)) return name;

			var sub = principal.FindFirst("sub");
			if (sub != null) return sub.Value;

			return string.Empty;
		}

		/// <summary>
		/// Determines whether this instance is authenticated.
		/// </summary>
		/// <param name="principal">The principal.</param>
		/// <returns>
		///   <c>true</c> if the specified principal is authenticated; otherwise, <c>false</c>.
		/// </returns>
		[DebuggerStepThrough]
		public static bool IsAuthenticated(this IPrincipal principal)
		{
			return principal != null && principal.Identity != null && principal.Identity.IsAuthenticated;
		}
	}
}