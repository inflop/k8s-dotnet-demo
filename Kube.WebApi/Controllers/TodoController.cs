using System.Collections.Generic;
using System.Linq;
using System;
using System.Net.Http;
using Kube.Contracts;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using System.Text;

namespace Kube.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class TodoController : ControllerBase
{
    private readonly ILogger<TodoController> _logger;
    private readonly HttpClient _httpClient;

    public TodoController(ILogger<TodoController> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient("DatabaseApi");
    }

    [HttpGet("test")]
    public string Test()
    {
        string msg = $"Hello from {nameof(TodoController)}: {DateTime.Now}";
        _logger.LogInformation(msg);
        return msg;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        IEnumerable<TodoItem> list = Enumerable.Empty<TodoItem>();
        try
        {
            var response = await _httpClient.GetAsync("/Database");

            response.EnsureSuccessStatusCode();

            list = await response.Content.ReadFromJsonAsync<IEnumerable<TodoItem>>();

            return Ok(list);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode((int)ex.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(500);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/Database/{id}");

            response.EnsureSuccessStatusCode();

            var item = await response.Content.ReadFromJsonAsync<TodoItem>();

            return Ok(item);
        }
        catch(HttpRequestException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode((int)ex.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(500);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Add(string title)
    {
        try
        {
            var response = await _httpClient.PostAsync($"/Database?title={title}", null);

            response.EnsureSuccessStatusCode();

            return Ok();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode((int)ex.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(500);
        }
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] TodoItem todoItem)
    {
        _logger.LogInformation("{id}; {title}; {isCompleted}", todoItem.Id, todoItem.Title, todoItem.IsCompleted);

        try
        {
            var content = new StringContent(JsonSerializer.Serialize(todoItem), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"/Database", content);

            response.EnsureSuccessStatusCode();

            return Ok();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode((int)ex.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(500);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"/Database/{id}");

            response.EnsureSuccessStatusCode();

            return Ok();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode((int)ex.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(500);
        }
    }
}
