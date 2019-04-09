using System;
using System.Collections.Generic;
using System.Diagnostics;
using DotnetSpider.Portal.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;
using DotnetSpider.Portal.Entity;
using DotnetSpider.Portal.Models.Docker;
using MailKit.Net.Imap;
using Microsoft.EntityFrameworkCore;
using X.PagedList;

namespace DotnetSpider.Portal.Controllers
{
	public class DockerController : Controller
	{
		private readonly ILogger _logger;
		private readonly PortalDbContext _dbContext;

		public DockerController(PortalDbContext dbContext, ILogger<DockerController> logger)
		{
			_logger = logger;
			_dbContext = dbContext;
		}

		[HttpGet("image-repository/add")]
		public IActionResult AddImageRepository()
		{
			return View();
		}
		
		[HttpPost("image-repository")]
		public async Task<IActionResult> ImageRepository([FromBody] AddImageRepositoryViewModel dto)
		{
			if (!ModelState.IsValid)
			{
				return View("AddImageRepository", dto);
			}

			var items = await _dbContext.DockerImageRepositories.Where(x =>
				x.Name == dto.Name || x.Repository == dto.Repository).ToListAsync();

			if (items.Any(x => x.Name == dto.Name))
			{
				ModelState.AddModelError("Name", "名称已经存在");
			}

			if (items.Any(x => x.Repository == dto.Repository))
			{
				ModelState.AddModelError("Repository", "镜像仓储已经存在");
			}

			if (items.Any())
			{
				return View("AddImageRepository", dto);
			}
			else
			{
				_dbContext.DockerImageRepositories.Add(new DockerImageRepository
				{
					Name = dto.Name,
					Registry = dto.Registry,
					Repository = dto.Repository,
					CreationTime = DateTime.Now
				});
				await _dbContext.SaveChangesAsync();
				return Redirect("image-repository");
			}
		}
		
		[HttpDelete("image-repository/{id}")]
		public async Task<IActionResult> ImageRepository(int id)
		{
			var item = await _dbContext.DockerImageRepositories.FirstOrDefaultAsync(x => x.Id == id);
			if (item != null)
			{
				_dbContext.DockerImageRepositories.Remove(item);
				await _dbContext.SaveChangesAsync();
			}

			return Redirect("image-repository");
		}
		
		[HttpGet("image-repository")]
		public async Task<IActionResult> ImageRepository()
		{
			var list = await _dbContext.DockerImageRepositories.ToListAsync();
			return View(list);
		}
 
		[HttpPost("docker/image/payload")]
		public async Task<IActionResult> PayloadImage([FromBody] ImagePayload payload)
		{
			var repository =
				await _dbContext.DockerImageRepositories.FirstOrDefaultAsync(
					x => x.Repository == payload.Repository.Repo_Full_Name);
			if (repository != null)
			{
				if (payload.Push_Data.Tag == "latest")
				{
					_logger.LogWarning($"忽略仓库 {payload.Repository.Repo_Full_Name} 的 latest 版本镜像");
					return Ok();
				}

				var image = $"{repository.Registry}/{payload.Repository.Repo_Full_Name}:{payload.Push_Data.Tag}";
				if (!await _dbContext.DockerImages.AnyAsync(x => x.Repository == image))
				{
					_dbContext.DockerImages.Add(new DockerImage
					{
						Repository = image,
						CreationTime = DateTime.Now,
						DockerImageRepositoryId = repository.Id
					});

					await _dbContext.SaveChangesAsync();
					_logger.LogInformation($"镜像 {image} 添加成功");
				}
				else
				{
					_logger.LogInformation($"镜像 {image} 已经存在");
				}
			}
			else
			{
				_logger.LogWarning($"仓库 {payload.Repository.Repo_Full_Name} 未配置");
			}

			return Ok();
		}
	}
}