using DotnetSpider.Core;
using DotnetSpider.Core.Pipeline;
using DotnetSpider.Extension.Model;
using OfficeOpenXml;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace DotnetSpider.Extension.Pipeline
{
	/// <summary>
	/// 把解析到的爬虫实体数据存到Excel中
	/// </summary>
	/// <typeparam name="T">数据对象</typeparam>
	public class ExcelPipeline<T> : BasePipeline where T : class
	{
		private readonly ExcelPackage _package;
		private int _rowRecord = 1;
		private readonly ExcelWorksheet _worksheet;
		private readonly PropertyInfo[] _properties;
		private readonly string _entityName;

		/// <summary>
		/// Excel 文件路径
		/// </summary>
		public string ExcelPath { get; private set; }

		/// <summary>
		/// 构造方法
		/// </summary>
		public ExcelPipeline()
		{
			_package = new ExcelPackage();
			_worksheet = _package.Workbook.Worksheets.Add("sheet1");
			var type = typeof(T);
			_properties = type.GetProperties();
			_entityName = type.FullName;

			if (_properties.Length == 0)
			{
				throw new ArgumentException($"Type {type} contains no property.");
			}
			for (int i = 1; i < _properties.Length + 1; ++i)
			{
				var column = _properties[i - 1];
				_worksheet.Cells[1, i].Value = column.Name;
			}
			IncreaseRowIndex();
			ExcelPath = Path.Combine(Env.BaseDirectory, "excels", $"{_entityName.Replace("+", ".")}.xlsx");
			var folder = Path.Combine(Env.BaseDirectory, "excels");
			if (!Directory.Exists(folder))
			{
				Directory.CreateDirectory(folder);
			}
		}

		/// <summary>
		/// 把解析到的爬虫实体数据存到Excel中
		/// </summary>
		/// <param name="resultItems">数据结果</param>
		/// <param name="spider">爬虫</param>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public override void Process(IEnumerable<ResultItems> resultItems, ISpider spider)
		{
			foreach (var resultItem in resultItems)
			{
				var data = resultItem.GetResultItem(_entityName);
				if (data != null)
				{
					if (data is IEnumerable)
					{
						foreach (var entity in data)
						{
							AddRow(entity);
						}
					}
					else
					{
						AddRow(data);
					}
				}
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public override void Dispose()
		{
			base.Dispose();

			if (File.Exists(ExcelPath))
			{
				File.Delete(ExcelPath);
			}
			_package.SaveAs(new FileInfo(ExcelPath));
		}

		private void AddRow(dynamic data)
		{
			for (int j = 1; j < _properties.Length + 1; ++j)
			{
				var column = _properties[j - 1];
				_worksheet.Cells[_rowRecord, j].Value = column.GetValue(data)?.ToString();
			}
			_rowRecord = IncreaseRowIndex();
		}

		private int IncreaseRowIndex()
		{
			++_rowRecord;
			return _rowRecord;
		}
	}
}
