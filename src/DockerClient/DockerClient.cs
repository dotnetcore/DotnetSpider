using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DockerClient
{
	public class DockerClient
	{
		private static readonly ConcurrentDictionary<string, HttpClient> Cache =
			new ConcurrentDictionary<string, HttpClient>();

		private readonly string _host;

		public DockerClient(Uri host)
		{
			_host = host.ToString();
			Cache.TryAdd(_host, new HttpClient());
		}


		public async Task<DockerResult> CreateAsync(string image, string[] env, Dictionary<string, object> labels)
		{
			var response = await Cache[_host].PostAsync($"{_host}v1.39/containers/create", new StringContent(
				JsonConvert.SerializeObject(new
				{
					Image = image,
					Env = env,
					Labels = labels
				}), Encoding.UTF8, "application/json"));
			var responseContent = await response.Content.ReadAsStringAsync();
			var result = JsonConvert.DeserializeObject<DockerResult>(responseContent);
			result.Success = response.IsSuccessStatusCode;
			return result;
		}

		public async Task<DockerResult> StartAsync(string id)
		{
			var response = await Cache[_host].PostAsync($"{_host}v1.39/containers/{id}/start",
				new StringContent("", Encoding.UTF8, "application/json"));
			var responseContent = await response.Content.ReadAsStringAsync();

			var result = string.IsNullOrWhiteSpace(responseContent)
				? new DockerResult()
				: JsonConvert.DeserializeObject<DockerResult>(responseContent);
			result.Success = response.IsSuccessStatusCode;
			return result;
		}

		public async Task<DockerResult> StopAsync(string id)
		{
			var response = await Cache[_host].PostAsync($"{_host}v1.39/containers/{id}/stop",
				new StringContent("", Encoding.UTF8, "application/json"));
			var responseContent = await response.Content.ReadAsStringAsync();

			var result = string.IsNullOrWhiteSpace(responseContent)
				? new DockerResult()
				: JsonConvert.DeserializeObject<DockerResult>(responseContent);
			result.Success = response.IsSuccessStatusCode;
			return result;
		}

		public async Task<DockerResult> RestartAsync(string id)
		{
			var response = await Cache[_host].PostAsync($"{_host}v1.39/containers/{id}/restart",
				new StringContent("", Encoding.UTF8, "application/json"));
			var responseContent = await response.Content.ReadAsStringAsync();

			var result = string.IsNullOrWhiteSpace(responseContent)
				? new DockerResult()
				: JsonConvert.DeserializeObject<DockerResult>(responseContent);
			result.Success = response.IsSuccessStatusCode;
			return result;
		}

		public async Task<DockerResult> KillAsync(string id)
		{
			var response = await Cache[_host].PostAsync($"{_host}v1.39/containers/{id}/kill",
				new StringContent("", Encoding.UTF8, "application/json"));
			var responseContent = await response.Content.ReadAsStringAsync();

			var result = string.IsNullOrWhiteSpace(responseContent)
				? new DockerResult()
				: JsonConvert.DeserializeObject<DockerResult>(responseContent);
			result.Success = response.IsSuccessStatusCode;
			return result;
		}

		public async Task<DockerResult> PauseAsync(string id)
		{
			var response = await Cache[_host].PostAsync($"{_host}v1.39/containers/{id}/pause",
				new StringContent("", Encoding.UTF8, "application/json"));
			var responseContent = await response.Content.ReadAsStringAsync();

			var result = string.IsNullOrWhiteSpace(responseContent)
				? new DockerResult()
				: JsonConvert.DeserializeObject<DockerResult>(responseContent);
			result.Success = response.IsSuccessStatusCode;
			return result;
		}

		public async Task<DockerResult> UnpauseAsync(string id)
		{
			var response = await Cache[_host].PostAsync($"{_host}v1.39/containers/{id}/unpause",
				new StringContent("", Encoding.UTF8, "application/json"));
			var responseContent = await response.Content.ReadAsStringAsync();

			var result = string.IsNullOrWhiteSpace(responseContent)
				? new DockerResult()
				: JsonConvert.DeserializeObject<DockerResult>(responseContent);
			result.Success = response.IsSuccessStatusCode;
			return result;
		}

		public async Task<DockerResult> RemoveAsync(string id)
		{
			var response = await Cache[_host].DeleteAsync($"{_host}v1.39/containers/{id}");
			var responseContent = await response.Content.ReadAsStringAsync();

			var result = string.IsNullOrWhiteSpace(responseContent)
				? new DockerResult()
				: JsonConvert.DeserializeObject<DockerResult>(responseContent);
			result.Success = response.IsSuccessStatusCode;
			return result;
		}

		public async Task<bool> ExistsAsync(object filter)
		{
			var response = await Cache[_host]
				.GetAsync($"{_host}v1.39/containers/json?filters={JsonConvert.SerializeObject(filter)}");
			var responseContent = await response.Content.ReadAsStringAsync();

			var result = JsonConvert.DeserializeObject<JArray>(responseContent);
			return result.Any();
		}
	}
}