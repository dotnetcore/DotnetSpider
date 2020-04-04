using System.Collections.Generic;
using AutoMapper;
using DotnetSpider.Infrastructure;

namespace DotnetSpider.Portal.Common
{
	public static class MapperExtensions
	{
		public static PagedQueryResult<T2> ToPagedQueryResult<T1, T2>(this IMapper mapper, PagedQueryResult<T1> source)
		{
			return new PagedQueryResult<T2>(source.Page, source.Limit, source.Count, mapper.Map<List<T2>>(source.Data));
		}
	}
}
