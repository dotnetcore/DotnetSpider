namespace DotnetSpider;

public sealed class ExitException(string msg) : SpiderException(msg);