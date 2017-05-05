//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using DotnetSpider.Core;
//using DotnetSpider.Core.Pipeline;
//using DotnetSpider.Extension.Common;
//using DotnetSpider.Extension.Model;

//namespace DotnetSpider.Extension.Pipeline
//{
//	/// <summary>
//	/// A pipeline combines the result in more than one page together.
//	/// Used for news and articles containing more than one web page. 
//	/// MultiPagePipeline will store parts of object and output them when all parts are extracted.
//	/// </summary>
//	public class MultiPagePipeline : BasePipeline
//	{
//		private DoubleKeyMap<string, string, bool> _pageMap = new DoubleKeyMap<string, string, bool>();
//		private DoubleKeyMap<string, string, IMultiPageModel> _objectMap = new DoubleKeyMap<string, string, IMultiPageModel>();

//		public override void Process(ResultItems resultItems)
//		{
//			IDictionary<string,dynamic> resultItemsAll = resultItems.Results;
//			List<object> deleteList = new List<object>();
//			foreach (DictionaryEntry entry in resultItemsAll)
//			{
//				bool needDelete;
//				HandleObject(entry, out needDelete);
//				if (needDelete)
//				{
//					deleteList.Add(entry.Key);
//				}
//			}

//			foreach (var key in deleteList)
//			{
//				resultItemsAll.Remove(key);
//			}
//		}

//		private void HandleObject(DictionaryEntry iterator, out bool needDelete)
//		{
//			needDelete = false;
//			object o = iterator.Value;
//			var model = o as IMultiPageModel;
//			if (model != null)
//			{
//				IMultiPageModel multiPageModel = model;
//				_pageMap.Put(multiPageModel.GetPageKey(), multiPageModel.GetPage(), true);
//				if (multiPageModel.GetOtherPages() != null)
//				{
//					foreach (string otherPage in multiPageModel.GetOtherPages())
//					{
//						bool aBoolean = _pageMap.Get(multiPageModel.GetPageKey(), otherPage);
//						if (!aBoolean)
//						{
//							_pageMap.Put(multiPageModel.GetPageKey(), otherPage, false);
//						}
//					}
//				}
//				//check if all pages are processed
//				Dictionary<string, bool> booleanMap = _pageMap.Get(multiPageModel.GetPageKey());
//				_objectMap.Put(multiPageModel.GetPageKey(), multiPageModel.GetPage(), multiPageModel);
//				if (booleanMap == null)
//				{
//					return;
//				}

//				if (booleanMap.Any(stringBooleanEntry => !stringBooleanEntry.Value))
//				{
//					needDelete = true;
//					return;
//				}
//				List<EntryObject> entryList = _objectMap.Get(multiPageModel.GetPageKey()).Select(entry => new EntryObject() { Entry = entry }).ToList();

//				if (entryList.Count != 0)
//				{
//					entryList.Sort();

//					IMultiPageModel value = entryList[0].Entry.Value;
//					for (int i = 1; i < entryList.Count; i++)
//					{
//						value = value.Combine(entryList[i].Entry.Value);
//					}
//					iterator.Value = value;
//				}
//			}
//		}

//		private class EntryObject : IComparable<EntryObject>
//		{
//			public KeyValuePair<string, IMultiPageModel> Entry { get; set; }

//			public int CompareTo(EntryObject other)
//			{
//				try
//				{
//					int i1 = int.Parse(Entry.Key);
//					int i2 = int.Parse(other.Entry.Key);
//					return i1 - i2;
//				}
//				catch (Exception)
//				{
//					return string.Compare(Entry.Key, other.Entry.Key, StringComparison.Ordinal);
//				}
//			}
//		}

//		public override void Dispose()
//		{
//			base.Dispose();
//			_pageMap = null;
//			_objectMap = null;
//		}
//	}
//}