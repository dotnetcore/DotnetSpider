namespace Java2Dotnet.Spider.Extension.Model
{
	public interface ISpiderEntity
	{
	}

	//public static class SpiderEntityExtensions
	//{
	//	public static Dictionary<string, object> ToDictionary(this ISpiderEntity entity)
	//	{
	//		var properties = entity.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
	//		return properties.ToDictionary(propertyInfo => propertyInfo.Name, propertyInfo => propertyInfo.GetValue(entity));
	//	}
	//}
}
