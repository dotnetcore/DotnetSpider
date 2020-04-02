using System;
using System.Threading.Tasks;
using DotnetSpider.Portal.Data;
using DotnetSpider.Portal.Models.DockerRepository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DotnetSpider.Portal.Controllers.API
{
	[ApiController]
	[Route("api/v1.0/dockerRepositories")]
	public class DockerRepositoryController : Controller
	{
		private readonly PortalDbContext _dbContext;

		public DockerRepositoryController(PortalDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		[HttpPost]
		public async Task<IApiResult> AddAsync(AddRepositoryViewModel dto)
		{
			if (!ModelState.IsValid)
			{
				return new FailedResult("Arguments invalid");
			}

			if (await _dbContext.DockerRepositories.AnyAsync(x => x.Name == dto.Name.Trim()))
			{
				return new FailedResult("名称已经存在");
			}

			string registry = null;
			string schema = null;

			if (!string.IsNullOrWhiteSpace(dto.Registry))
			{
				if (Uri.TryCreate(dto.Registry, UriKind.RelativeOrAbsolute, out var uri))
				{
					schema = uri.Scheme;
					registry = $"{uri.Host}{(uri.Port == 80 || uri.Port == 443 ? "" : $":{uri.Port}")}";
				}
				else
				{
					return new FailedResult("Registry 格式不正确");
				}
			}

			if (await _dbContext.DockerRepositories.AnyAsync(x =>
				x.Registry == dto.Registry || x.Repository == dto.Repository))
			{
				return new FailedResult("镜像仓储已经存在");
			}

			var repository = new DockerRepository
			{
				Name = dto.Name,
				Schema = schema,
				Registry = registry,
				Repository = dto.Repository,
				UserName = dto.UserName,
				Password = dto.Password,
				CreationTime = DateTimeOffset.Now
			};
			_dbContext.DockerRepositories.Add(repository);
			await _dbContext.SaveChangesAsync();
			return new ApiResult("OK");
		}

		[HttpDelete("{id}")]
		public async Task<IApiResult> DeleteAsync(int id)
		{
			var item = await _dbContext.DockerRepositories.FirstOrDefaultAsync(x => x.Id == id);
			if (item != null)
			{
				_dbContext.DockerRepositories.Remove(item);
				await _dbContext.SaveChangesAsync();
			}

			return new ApiResult("OK");
		}
	}
}
