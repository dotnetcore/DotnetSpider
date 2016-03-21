namespace MySql.Data.Common
{
	using System.Text;
	
	public static class BytesExtensions{
		public static string GetString(this byte[] bytes){
				StringBuilder builder = new StringBuilder();
			foreach (var b in bytes)
			{
				builder.Append(b);
			}
			return builder.ToString();
		}
	}

}