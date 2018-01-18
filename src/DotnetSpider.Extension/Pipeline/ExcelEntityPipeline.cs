using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using DotnetSpider.Core;
using OfficeOpenXml;

namespace DotnetSpider.Extension.Pipeline
{
	/// <summary>
	/// 把解析到的爬虫实体数据存到Excel中
	/// </summary>
	public class ExcelEntityPipeline : BaseEntityPipeline
	{
		private readonly Dictionary<string, ExcelPackage> _packages = new Dictionary<string, ExcelPackage>();
		private readonly Dictionary<string, int> _rowRecords = new Dictionary<string, int>();

		/// <summary>
		/// 把解析到的爬虫实体数据存到Excel中
		/// </summary>
		/// <param name="entityName">爬虫实体类的名称</param>
		/// <param name="datas">实体类数据</param>
		/// <param name="spider">爬虫</param>
		/// <returns>最终影响结果数量(如数据库影响行数)</returns>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public override int Process(string entityName, IEnumerable<dynamic> datas, ISpider spider)
		{
			int count = 0;
			if (EntityAdapters.TryGetValue(entityName, out var metadata))
			{
				switch (metadata.PipelineMode)
				{
					case PipelineMode.Insert:
					case PipelineMode.InsertAndIgnoreDuplicate:
						{
							WriteToExcel(entityName, datas, spider, metadata);
							break;
						}
					case PipelineMode.InsertNewAndUpdateOld:
						{
							throw new NotImplementedException("Sql Server not suport InsertNewAndUpdateOld yet.");
						}
					case PipelineMode.Update:
						{
							throw new NotImplementedException("Sql Server not suport InsertNewAndUpdateOld yet.");
						}
					default:
						{
							WriteToExcel(entityName, datas, spider, metadata);
							break;
						}
				}
			}
			return count;
		}

		/// <summary>
		/// 取得数据管道中所有EXCEL文件的路径
		/// </summary>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public IEnumerable<string> GetFiles()
		{
			return _packages.Keys;
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public override void Dispose()
		{
			base.Dispose();

			var folder = Path.Combine(Env.BaseDirectory, "excels");
			if (!Directory.Exists(folder))
			{
				Directory.CreateDirectory(folder);
			}

			foreach (var package in _packages)
			{
				var path = package.Key;
				if (File.Exists(path))
				{
					File.Delete(path);
				}
				package.Value.SaveAs(new FileInfo(path));
			}
		}

		private void WriteToExcel(string entityName, IEnumerable<dynamic> datas, ISpider spider, EntityAdapter adapter)
		{
			var excelPath = Path.Combine(Env.BaseDirectory, "excels", $"{spider.Name}_{spider.Identity}.xlsx");
			var sheetName = entityName;
			var sheetIndex = $"{excelPath}.{sheetName}";

			if (!_packages.ContainsKey(excelPath))
			{
				_packages.Add(excelPath, new ExcelPackage());
			}
			if (!_rowRecords.ContainsKey(sheetIndex))
			{
				_rowRecords.Add(sheetIndex, 1);
			}

			var p = _packages[excelPath];

			var sheet = p.Workbook.Worksheets[sheetName];

			int row = 1;
			var columns = adapter.Columns.Where(c => !Env.IdColumns.Contains(c.Name.ToLower())).ToList();

			if (sheet == null)
			{
				sheet = p.Workbook.Worksheets.Add(sheetName);

				for (int i = 1; i < columns.Count + 1; ++i)
				{
					var column = columns[i - 1];
					sheet.Cells[1, i].Value = columns[i - 1].Name.ToLower();
				}
				row = IncreaseRowIndex(sheetIndex);
			}

			foreach (var data in datas)
			{
				for (int j = 1; j < columns.Count + 1; ++j)
				{
					var column = columns[j - 1].Property;
					sheet.Cells[row, j].Value = column.GetValue(data)?.ToString();
				}
				row = IncreaseRowIndex(sheetIndex);
			}
		}

		private int IncreaseRowIndex(string sheet)
		{
			var row = _rowRecords[sheet] + 1;
			_rowRecords[sheet] = row;
			return row;
		}
	}
}
