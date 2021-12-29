using System.Collections.Generic;
using AutoMapper;
using DotnetSpider.Infrastructure;

namespace DotnetSpider.Portal.Common
{
	public static class MapperExtensions
	{
		public static PagedResult<T2> ToPagedQueryResult<T1, T2>(this IMapper mapper, PagedResult<T1> source)
		{
			return new(source.Page, source.Limit, source.Count, mapper.Map<List<T2>>(source.Data));
		}
	}
}
