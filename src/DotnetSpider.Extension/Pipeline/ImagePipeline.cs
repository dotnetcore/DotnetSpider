//using System;
//using System.Collections;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.IO;
//using System.Net;
//using System.Reflection;
//using DotnetSpider.Core;
//using DotnetSpider.Core.Utils;
//using DotnetSpider.Extension.Model.Attribute;

//namespace DotnetSpider.Extension.Pipeline
//{
//	public class ImagePipeline<T> : FilePersistentBase, IPageModelPipeline<T>
//	{
//		private readonly ConcurrentDictionary<Type, List<PropertyInfo>> _cache = new ConcurrentDictionary<Type, List<PropertyInfo>>();
//		private readonly static WebClient WebClient = new WebClient();

//		public void Process( List<T>  data, ISpider spider)
//		{
//			if (data == null)
//			{
//				return;
//			}

//			foreach (var pair in data)
//			{
//				Type type = pair.Key;

//				List<PropertyInfo> downloadPropertyInfos = null;
//				if (!type.IsGenericType)
//				{
//					if (_cache.ContainsKey(type))
//					{
//						downloadPropertyInfos = _cache[type];
//					}
//					else
//					{
//						PropertyInfo[] propertyInfos = type.GetProperties(BindingFlags.GetProperty);
//						downloadPropertyInfos = new List<PropertyInfo>();
//						foreach (var propertyInfo in propertyInfos)
//						{
//							List<Download> downloads = new List<Download>(propertyInfo.GetCustomAttributes<Download>());
//							if (downloads != null && downloads.Count > 0)
//							{
//								downloadPropertyInfos.Add(propertyInfo);
//							}
//						}

//						_cache.TryAdd(type, downloadPropertyInfos);
//					}
//				}
//				else
//				{
//					IList list = pair.Value;
//					if (list.Count > 0)
//					{
//						type = list[0].GetType();

//						if (_cache.ContainsKey(type))
//						{
//							downloadPropertyInfos = _cache[type];
//						}
//						else
//						{
//							PropertyInfo[] propertyInfos = type.GetProperties(BindingFlags.GetProperty);
//							downloadPropertyInfos = new List<PropertyInfo>();
//							foreach (var propertyInfo in propertyInfos)
//							{
//								List<Download> downloads = new List<Download>(propertyInfo.GetCustomAttributes<Download>());
//								if (downloads != null && downloads.Count > 0)
//								{
//									downloadPropertyInfos.Add(propertyInfo);
//								}
//							}

//							_cache.TryAdd(type, downloadPropertyInfos);
//						}
//					}
//				}

//				if (downloadPropertyInfos == null || downloadPropertyInfos.Count == 0)
//				{
//					return;
//				}

//				string direcotryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"data\dotnetspider\") + PathSeperator + spider.Identify + PathSeperator;

//				foreach (var value in data.Values)
//				{
//					foreach (var downloadPropertyInfo in downloadPropertyInfos)
//					{
//						//check 这里需要做一下URL检测, 如果不是URL则跳过下载, 可以考虑用正则或者限定图片,文件
//						string url = downloadPropertyInfo.GetMethod.Invoke(value, null).ToString();
//						string fileName = Path.GetFileName(url);
//						try
//						{
//							WebClient.DownloadFile(url, Path.Combine(direcotryPath, fileName));
//						}
//						catch (Exception)
//						{
//							Logger.SaveLog($"Download image: {fileName} failed.");
//						}
//					}
//				}
//			}
//		}

//		public void Dispose()
//		{
//			_cache.Clear();
//		}
//	}
//}
