namespace DockerClient
{
	public class DockerResult
	{
		public string Id { get; set; }
		public bool Success { get; set; }

		public string Message { get; set; }

		public string[] Warnings { get; set; }
	}
}