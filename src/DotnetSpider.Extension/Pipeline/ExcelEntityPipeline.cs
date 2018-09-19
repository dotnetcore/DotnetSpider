using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using DotnetSpider.Core;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extraction.Model;
using OfficeOpenXml;

namespace DotnetSpider.Extension.Pipeline
{
	/// <summary>
	/// 把解析到的爬虫实体数据存到Excel中
	/// </summary>
	public class ExcelEntityPipeline : EntityPipeline
	{
		private readonly Dictionary<string, ExcelPackage> _packages = new Dictionary<string, ExcelPackage>();
		private readonly Dictionary<string, int> _rowRecords = new Dictionary<string, int>();

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

		private void WriteToExcel(IEnumerable<IBaseEntity> datas, dynamic sender)
		{
			var tableInfo = new TableInfo(datas.First().GetType());
			var excelPath = Path.Combine(Env.BaseDirectory, "excels", $"{sender.Name}_{sender.Identity}.xlsx");
			var sheetName = tableInfo.Schema.TableName;
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

			if (sheet == null)
			{
				sheet = p.Workbook.Worksheets.Add(sheetName);

				for (int i = 1; i < tableInfo.Columns.Count + 1; ++i)
				{
					sheet.Cells[1, i].Value = tableInfo.Columns.ElementAt(i - 1).Name.ToLower();
				}

				row = IncreaseRowIndex(sheetIndex);
			}
			var properties = new Dictionary<string, PropertyInfo>();
			foreach (var property in datas.First().GetType().GetProperties())
			{
				properties.Add(property.Name, property);
			}
			foreach (var data in datas)
			{
				for (int j = 1; j < tableInfo.Columns.Count + 1; ++j)
				{
					var column = tableInfo.Columns.ElementAt(j - 1).Name;
					sheet.Cells[row, j].Value = properties[column].GetValue(data);
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

		/// <summary>
		/// 把解析到的爬虫实体数据存到Excel中
		/// </summary>
		/// <param name="datas">数据</param>
		/// <param name="logger">日志接口</param>
		/// <param name="sender">调用方</param>
		/// <returns>最终影响结果数量(如数据库影响行数)</returns>
		[MethodImpl(MethodImplOptions.Synchronized)]
		protected override int Process(IEnumerable<IBaseEntity> datas, dynamic sender = null)
		{
			if (datas == null || !datas.Any())
			{
				return 0;
			}

			WriteToExcel(datas, sender);
			return datas.Count();
		}
	}
}