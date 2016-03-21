namespace Java2Dotnet.Spider.Core
{
	/// <summary>
	/// Listener of Spider on page processing. Used for monitor and such on.
	/// </summary>
	public interface ISpiderListener
	{
		void OnSuccess(Request request);

		void OnError(Request request);

		void OnClose();
	}
}
