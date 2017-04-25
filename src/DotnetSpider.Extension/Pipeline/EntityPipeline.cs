//using System;
//using System.Collections.Generic;
//using DotnetSpider.Core;
//using DotnetSpider.Core.Pipeline;
//using Newtonsoft.Json.Linq;

//namespace DotnetSpider.Extension.Pipeline
//{
//	public class EntityPipeline : IPipeline
//	{
//		private readonly List<BaseEntityPipeline> _pipelines;
//		private readonly string _entityName;

//		public ISpider Spider =>  

//		public EntityPipeline(string entityName, List<BaseEntityPipeline> pipelines)
//		{
//			_entityName = entityName;
//			_pipelines = pipelines;

//		}

//		public virtual void InitPipeline(ISpider spider)
//		{
//			foreach (var pipeline in _pipelines)
//			{
//				pipeline.InitPipeline(spider);
//			}
//		}

//		public List<BaseEntityPipeline> GetEntityPipelines()
//		{
//			return _pipelines;
//		}

//		protected virtual void Process(params ResultItems[] resultItems)
//		{
//			if (resultItems == null || resultItems.Length == 0)
//			{
//				return;
//			}

//			List<JObject> list = new List<JObject>();
//			foreach (var resultItem in resultItems)
//			{
//				dynamic data = resultItem.GetResultItem(_entityName);

//				if (data != null)
//				{
//					if (data is JObject)
//					{
//						list.Add(data);
//					}
//					else
//					{
//						list.AddRange(data);
//					}
//				}
//			}

//			if (list.Count > 0)
//			{
//				foreach (var pipeline in _pipelines)
//				{
//					pipeline.Process(list);
//				}
//			}
//		}

//		public void Dispose()
//		{
//		}
//	}
//}
