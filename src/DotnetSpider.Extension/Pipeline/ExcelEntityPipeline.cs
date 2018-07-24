using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using DotnetSpider.Common;
using DotnetSpider.Core;
using DotnetSpider.Extraction.Model;
using OfficeOpenXml;

namespace DotnetSpider.Extension.Pipeline
{
	/// <summary>
	/// 把解析到的爬虫实体数据存到Excel中
	/// </summary>
	public class ExcelEntityPipeline : ModelPipeline
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

		private void WriteToExcel(IModel model, IList<dynamic> datas, dynamic sender)
		{
			var excelPath = Path.Combine(Env.BaseDirectory, "excels", $"{sender.Name}_{sender.Identity}.xlsx");
			var sheetName = model.Table.Name;
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
			var columns = model.Fields.ToList();

			if (sheet == null)
			{
				sheet = p.Workbook.Worksheets.Add(sheetName);

				for (int i = 1; i < columns.Count + 1; ++i)
				{
					sheet.Cells[1, i].Value = columns[i - 1].Name.ToLower();
				}

				row = IncreaseRowIndex(sheetIndex);
			}

			foreach (var data in datas)
			{
				for (int j = 1; j < columns.Count + 1; ++j)
				{
					var column = columns[j - 1].Name;
					sheet.Cells[row, j].Value = data[column];
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
		/// <param name="model">数据模型</param>
		/// <param name="datas">数据</param>
		/// <param name="logger">日志接口</param>
		/// <param name="sender">调用方</param>
		/// <returns>最终影响结果数量(如数据库影响行数)</returns>
		[MethodImpl(MethodImplOptions.Synchronized)]
		protected override int Process(IModel model, IList<dynamic> datas, ILogger logger, dynamic sender = null)
		{
			if (datas == null || datas.Count == 0)
			{
				return 0;
			}

			WriteToExcel(model, datas, sender);
			return datas.Count;
		}
	}
}