//#if !NET_CORE

//using System;
//using System.Data;
//using System.IO;
//using NPOI.HSSF.UserModel;
//using NPOI.SS.UserModel;
//using NPOI.XSSF.UserModel;

//namespace DotnetSpider.Core.Infrastructure
//{
//	/// <summary>
//	/// Excel的辅助类
//	/// </summary>
//	public class ExcelClient
//	{
//		/// <summary>
//		/// 读取excel到datatable中
//		/// </summary>
//		/// <param name="excelPath">excel地址</param>
//		/// <param name="sheetIndex">sheet索引</param>
//		/// <returns>成功返回datatable，失败返回null</returns>
//		public DataTable GetContent(string excelPath, int sheetIndex)
//		{
//			string sheetName = "";
//			return GetContent(excelPath, sheetIndex, ref sheetName);
//		}

//		/// <summary>
//		/// 读取excel到datatable中
//		/// </summary>
//		/// <param name="excelPath">excel地址</param>
//		/// <param name="sheetIndex">sheet索引</param>
//		/// <param name="sheetName"></param>
//		/// <returns>成功返回datatable，失败返回null</returns>
//		public DataTable GetContent(string excelPath, int sheetIndex, ref string sheetName)
//		{
//			IWorkbook workbook = null;//全局workbook
//			DataTable table = null;
//			try
//			{
//				FileInfo fileInfo = new FileInfo(excelPath);//判断文件是否存在
//				if (fileInfo.Exists)
//				{
//					FileStream fileStream = fileInfo.OpenRead();//打开文件，得到文件流
//					switch (fileInfo.Extension)
//					{
//						//xls是03，用HSSFWorkbook打开，.xlsx是07或者10用XSSFWorkbook打开
//						case ".xls": workbook = new HSSFWorkbook(fileStream); break;
//						case ".xlsx": workbook = new XSSFWorkbook(fileStream); break;
//					}
//					fileStream.Close();//关闭文件流
//				}
//				if (workbook != null)
//				{
//					var sheet = workbook.GetSheetAt(sheetIndex);//sheet
//					sheetName = sheet.SheetName;
//					table = new DataTable();//初始化一个table

//					IRow headerRow = sheet.GetRow(0);//获取第一行，一般为表头
//					int cellCount = headerRow.LastCellNum;//得到列数

//					for (int i = headerRow.FirstCellNum; i < cellCount; i++)
//					{
//						var cell = headerRow.GetCell(i);
//						if (cell == null)
//						{
//							throw new Exception("不能有空列名");
//						}
//						DataColumn column = new DataColumn(cell.ToString());//初始化table的列
//						table.Columns.Add(column);
//					}
//					//遍历读取cell
//					for (int i = (sheet.FirstRowNum + 1); i <= sheet.LastRowNum; i++)
//					{
//						IRow row = sheet.GetRow(i);//得到一行
//						DataRow dataRow = table.NewRow();//新建一个行

//						for (int j = row.FirstCellNum; j < cellCount; j++)
//						{
//							ICell cell = row.GetCell(j);//得到cell
//							if (cell == null)//如果cell为null，则赋值为空
//							{
//								dataRow[j] = "";
//							}
//							else
//							{
//								dataRow[j] = row.GetCell(j).ToString();//否则赋值
//							}
//						}

//						table.Rows.Add(dataRow);//把行 加入到table中
//					}
//				}
//				else
//				{
//					sheetName = null;
//				}

//				return table;
//			}
//			catch (Exception)
//			{
//				return table;
//			}
//			finally
//			{
//				//释放资源
//				table?.Dispose();
//			}
//		}

//		public int GetSheetCount(string excelPath)
//		{
//			IWorkbook workbook = null;//全局workbook

//			try
//			{
//				FileInfo fileInfo = new FileInfo(excelPath);//判断文件是否存在
//				if (fileInfo.Exists)
//				{
//					FileStream fileStream = fileInfo.OpenRead();//打开文件，得到文件流
//					switch (fileInfo.Extension)
//					{
//						//xls是03，用HSSFWorkbook打开，.xlsx是07或者10用XSSFWorkbook打开
//						case ".xls": workbook = new HSSFWorkbook(fileStream); break;
//						case ".xlsx": workbook = new XSSFWorkbook(fileStream); break;
//					}
//					fileStream.Close();//关闭文件流
//				}
//				if (workbook != null)
//				{
//					return workbook.NumberOfSheets;
//				}
//				else
//				{
//					return 0;
//				}
//			}
//			catch (Exception)
//			{
//				return -1;
//			}
//		}

//		//public string ExportContent<T>(List<T> data, string path)
//		//{
//		//	DirectoryInfo diretory = new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Report"));
//		//	if (!diretory.Exists)
//		//	{
//		//		diretory.Create();
//		//	}

//		//	string realFilePath = Path.Combine(diretory.FullName, path + "_" + DateTimeUtil.RunId.ToString("yyyy_MM_dd") + ".xls");

//		//	Type type = typeof(T);

//		//	PropertyInfo[] propertyInfos = type.GetProperties();
//		//	Dictionary<PropertyInfo, ColumnAttribute> pMapC = new Dictionary<PropertyInfo, ColumnAttribute>();

//		//	foreach (var p in propertyInfos)
//		//	{
//		//		ColumnAttribute c = p.GetCustomAttribute<ColumnAttribute>();
//		//		if (c != null)
//		//		{
//		//			pMapC.Add(p, c);
//		//		}
//		//	}

//		//	if (pMapC.Count == 0)
//		//	{
//		//		return null;
//		//	}

//		//	HSSFWorkbook book = new HSSFWorkbook();
//		//	ISheet sheet = book.CreateSheet("sheet1");

//		//	int j = 0;
//		//	IRow header = sheet.CreateRow(0);
//		//	foreach (var entry in pMapC)
//		//	{
//		//		header.CreateCell(j).SetCellValue(entry.Value.Name);
//		//		++j;
//		//	}

//		//	for (int i = 0; i < data.Count; ++i)
//		//	{
//		//		IRow row = sheet.CreateRow(i + 1);

//		//		j = 0;
//		//		foreach (var entry in pMapC)
//		//		{
//		//			object value = entry.Key.GetValue(data[i]);
//		//			row.CreateCell(j).SetCellValue(value == null ? "" : value.ToString());

//		//			++j;
//		//		}
//		//	}

//		//	if (File.Exists(realFilePath))
//		//	{
//		//		File.Delete(realFilePath);
//		//	}
//		//	using (FileStream stream = File.Open(realFilePath, FileMode.CreateNew, FileAccess.ReadWrite))
//		//	{
//		//		book.Write(stream);
//		//		stream.Flush();
//		//	}
//		//	return realFilePath;
//		//}
//	}
//}

//#endif