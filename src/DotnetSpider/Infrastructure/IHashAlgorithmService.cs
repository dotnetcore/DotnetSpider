namespace DotnetSpider.Infrastructure
{
	public interface IHashAlgorithmService
	{
		byte[] ComputeHash(byte[] bytes);
	}
}
