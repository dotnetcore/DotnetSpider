using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DotnetSpider.Infrastructure;

namespace DotnetSpider.Portal.Common
{
	public class OrderCondition<TEntity, TKey>
	{
		public Expression<Func<TEntity, TKey>> Expression { get; }
		public bool Desc { get; }

		public OrderCondition(Expression<Func<TEntity, TKey>> expression, bool desc = false)
		{
			Expression = expression ?? throw new ArgumentNullException(nameof(expression));
			Desc = desc;
		}
	}

	public static class PagedQueryExtensions
	{
		public static async Task<PagedResult<TEntity>> PagedQueryAsync<TEntity>(
			this IQueryable<TEntity> queryable,
			int page, int limit,
			Expression<Func<TEntity, bool>> where = null)
			where TEntity : class
		{
			return await queryable.PagedQueryAsync<TEntity, object>(page, limit, where);
		}

		public static async Task<PagedResult<TEntity>> PagedQueryAsync<TEntity, TOrderKey>(
			this IQueryable<TEntity> queryable,
			int page, int limit,
			Expression<Func<TEntity, bool>> where = null, OrderCondition<TEntity, TOrderKey> orderBy = null)
			where TEntity : class
		{
			return await queryable.PagedQueryAsync<TEntity, TOrderKey, object>(page, limit, where, orderBy);
		}

		public static async Task<PagedResult<TEntity>> PagedQueryAsync<TEntity, TOrderKey, TThenOrderKey>(
			this IQueryable<TEntity> queryable,
			int page, int limit,
			Expression<Func<TEntity, bool>> where = null, OrderCondition<TEntity, TOrderKey> orderBy = null,
			OrderCondition<TEntity, TThenOrderKey> thenBy = null)
			where TEntity : class
		{
			return await queryable.PagedQueryAsync<TEntity, TOrderKey, TThenOrderKey, object>(page, limit, where,
				orderBy, thenBy);
		}

		public static Task<PagedResult<TEntity>> PagedQueryAsync<TEntity, TOrderKey, TThenOrderKey1,
			TThenOrderKey2>(
			this IQueryable<TEntity> queryable,
			int page, int limit,
			Expression<Func<TEntity, bool>> where = null, OrderCondition<TEntity, TOrderKey> orderBy = null,
			OrderCondition<TEntity, TThenOrderKey1> thenBy1 = null,
			OrderCondition<TEntity, TThenOrderKey2> thenBy2 = null)
			where TEntity : class
		{
			page = page < 1 ? 1 : page;
			limit = limit < 1 ? 10 : limit;
			var entities = where == null ? queryable : queryable.Where(where);
			if (orderBy != null)
			{
				entities = !orderBy.Desc
					? entities.OrderBy(orderBy.Expression)
					: entities.OrderByDescending(orderBy.Expression);
			}

			if (thenBy1 != null)
			{
				if (orderBy == null)
				{
					throw new ArgumentException("Order by should not be null when use then by");
				}

				entities = !thenBy1.Desc
					? ((IOrderedQueryable<TEntity>)entities).ThenBy(thenBy1.Expression)
					: ((IOrderedQueryable<TEntity>)entities).OrderByDescending(thenBy1.Expression);
			}

			if (thenBy2 != null)
			{
				if (orderBy == null)
				{
					throw new ArgumentException("Order by should not be null when use then by");
				}

				entities = !thenBy2.Desc
					? ((IOrderedQueryable<TEntity>)entities).ThenBy(thenBy2.Expression)
					: ((IOrderedQueryable<TEntity>)entities).OrderByDescending(thenBy2.Expression);
			}

			var total = entities.Count();
			var data = total == 0 ? new List<TEntity>() : entities.Skip((page - 1) * limit).Take(limit).ToList();
			return Task.FromResult(new PagedResult<TEntity>(page, limit, total, data));
		}
	}
}
