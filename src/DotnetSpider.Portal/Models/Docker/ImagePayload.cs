using System;

namespace DotnetSpider.Portal.Models.Docker
{
//	{
//	"Repository": {
//	"Date_Created": "2019-04-09T10:55:15",
//	"Name": "helloworld",
//	"NameSpace": "zlzforever",
//	"Region": "cn-shanghai",
//	"Repo_Full_Name": "zlzforever/helloworld"
//},
//"Push_Data": {
//"Pushed_At": "2019-04-09T16:40:13",
//"Digest": "sha256:f4bb82a511f77a0bd1efcbc930bfc68afc625f3f5fa7183fc02de915b5f0a7af",
//"Tag": "latest"
//}
//}
	public class ImagePayload
	{
		public RepositoryInfo Repository { get; set; }

		public RepositoryPushData Push_Data { get; set; }

		public class RepositoryInfo
		{
			public DateTime Date_Created { get; set; }

			public string Name { get; set; }

			public string NameSpace { get; set; }

			public string Region { get; set; }

			public string Repo_Full_Name { get; set; }
		}

		public class RepositoryPushData
		{
			public DateTime Pushed_At { get; set; }

			public string Digest { get; set; }

			public string Tag { get; set; }
		}
	}
}