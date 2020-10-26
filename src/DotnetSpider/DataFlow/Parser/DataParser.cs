using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DotnetSpider.DataFlow.Storage;
using DotnetSpider.Http;
using DotnetSpider.Infrastructure;
using DotnetSpider.Selector;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DotnetSpider.DataFlow.Parser
{
	/// <summary>
	/// 数据解析器
	/// </summary>
	public abstract class DataParser : DataFlowBase
	{
		private readonly List<Func<DataFlowContext, IEnumerable<Request>>> _followRequestQueriers;
		private readonly List<Func<Request, bool>> _requiredValidator;

		/// <summary>
		/// 选择器的生成方法
		/// </summary>
		public Func<DataFlowContext, ISelectable> SelectableBuilder { get; protected set; }

		/// <summary>
		/// 数据解析
		/// </summary>
		/// <param name="context">处理上下文</param>
		/// <returns></returns>
		protected abstract Task ParseAsync(DataFlowContext context);

		protected DataParser()
		{
			_followRequestQueriers = new List<Func<DataFlowContext, IEnumerable<Request>>>();
			_requiredValidator = new List<Func<Request, bool>>();
		}

		public void AddFollowRequestQuerier(ISelector selector)
		{
			_followRequestQueriers.Add(context =>
			{
				var requests = context.Selectable.SelectList(selector)
					.Where(x => x != null)
					.SelectMany(x => x.Links())
					.Select(x =>
					{
						var request = context.CreateNewRequest(new Uri(x));
						request.RequestedTimes = 0;
						return request;
					});
				return requests;
			});
		}

		public void AddRequiredValidator(Func<Request, bool> requiredValidator)
		{
			_requiredValidator.Add(requiredValidator);
		}

		public void AddRequiredValidator(string pattern)
		{
			_requiredValidator.Add(request => Regex.IsMatch(request.RequestUri.ToString(), pattern));
		}

		protected virtual void AddParsedResult<T>(DataFlowContext context, IEnumerable<T> results)
			where T : EntityBase<T>, new()
		{
			if (results != null)
			{
				var type = typeof(T);
				var items = context.GetData(type);
				if (items == null)
				{
					var list = new List<T>();
					list.AddRange(results);
					context.AddData(type, list);
				}
				else
				{
					items.AddRange(results);
				}
			}
		}

		internal void UseHtmlSelectableBuilder()
		{
			SelectableBuilder = context =>
			{
				var text = context.Response.ReadAsString().TrimStart();
				return CreateHtmlSelectable(context, text);
			};
		}

		private ISelectable CreateHtmlSelectable(DataFlowContext context, string text)
		{
			var uri = context.Request.RequestUri;
			var domain = uri.Port == 80 || uri.Port == 443
				? $"{uri.Scheme}://{uri.Host}"
				: $"{uri.Scheme}://{uri.Host}:{uri.Port}";
			return new HtmlSelectable(text, domain, context.Options.RemoveOutboundLinks);
		}

		/// <summary>
		/// 数据解析
		/// </summary>
		/// <param name="context">处理上下文</param>
		/// <returns></returns>
		public override async Task HandleAsync(DataFlowContext context)
		{
			context.NotNull(nameof(context));
			context.Response.NotNull(nameof(context.Response));

			if (!IsValidRequest(context.Request))
			{
				Logger.LogInformation(
					$"{GetType().Name} ignore parse request {context.Request.RequestUri}, {context.Request.Hash}");
				return;
			}

			var request = context.Request;
			if (context.Selectable == null)
			{
				if (SelectableBuilder != null)
				{
					context.Selectable = SelectableBuilder(context);
				}
				else
				{
					var text = context.Response.ReadAsString().TrimStart();
					if (text.StartsWith("<!DOCTYPE html>") || text.StartsWith("<html>"))
					{
						context.Selectable = CreateHtmlSelectable(context, text);
					}
					else
					{
						try
						{
							var token = (JObject)JsonConvert.DeserializeObject(text);
							context.Selectable = new JsonSelectable(token);
						}
						catch
						{
							context.Selectable = new TextSelectable(text);
						}
					}
				}
			}

			await ParseAsync(context);

			var requests = new List<Request>();

			if (_followRequestQueriers != null)
			{
				foreach (var followRequestQuerier in _followRequestQueriers)
				{
					var followRequests = followRequestQuerier(context);
					if (followRequests != null)
					{
						requests.AddRange(followRequests);
					}
				}
			}

			foreach (var followRequest in requests)
			{
				if (IsValidRequest(followRequest))
				{
					// 在此强制设制 Owner, 防止用户忘记导致出错
					followRequest.Owner = request.Owner;
					followRequest.Agent = context.Response.Agent;
					context.AddFollowRequests(followRequest);
				}
			}
		}

		private bool IsValidRequest(Request request)
		{
			return _requiredValidator.Count <= 0 ||
			       _requiredValidator.Any(validator => validator(request));
		}
	}
}
