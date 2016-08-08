//using System;
//using DotnetSpider.Core;

//namespace DotnetSpider.Extension.Model.Attribute
//{
//	/// <summary>
//	/// Define how the result string is convert to an object for field.
//	/// </summary>
//	[AttributeUsage(AttributeTargets.Property)]
//	public class Formatter : System.Attribute
//	{
//		private Type _formatterType;

//		public Formatter(Type formatter, string[] value = null, bool useDefaultFormatter = false, Type subType = null)
//		{
//			Value = value;
//			SubType = subType;
//			FormatterType = formatter;
//			UseDefaultFormatter = useDefaultFormatter;
//		}

//		/// <summary>
//		/// Set formatter params.
//		/// </summary>
//		public string[] Value { get; }

//		/// <summary>
//		/// Specific the class of field of class of elements in collection for field. 
//		/// It is not necessary to be set because we can detect the class by class of field,
//		/// unless you use a collection as a field. 
//		/// </summary>
//		public Type SubType { get; }

//		public bool UseDefaultFormatter { get; }

//		/// <summary>
//		/// If there are more than one formatter for a class, just specify the implement.
//		/// </summary>
//		public Type FormatterType
//		{
//			get { return _formatterType; }
//			private set
//			{
//				if (value == null)
//				{
//					throw new SpiderExceptoin("Formatter type can't be null.");
//				}
//				_formatterType = value;
//			}
//		}
//	}
//}
