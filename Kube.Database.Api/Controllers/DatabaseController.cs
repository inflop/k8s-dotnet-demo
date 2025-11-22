using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kube.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Kube.Database.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class DatabaseController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<DatabaseController> _logger;
    private readonly TodoItemRepository _todoItemRepository;

    public DatabaseController(IConfiguration configuration, ILogger<DatabaseController> logger, TodoItemRepository todoItemRepository)
    {
        _configuration = configuration;
        _logger = logger;
        _todoItemRepository = todoItemRepository;
    }

    [HttpGet("test")]
    public string Test()
    {
        string msg = $"Hello from {nameof(DatabaseController)}: {DateTime.Now}";
        _logger.LogInformation(msg);
        return msg;
    }

    [HttpGet("connection")]
    public string GetConnectionString()
    {
        string connectionString = _configuration.GetValue<string>("ConnectionString");
        _logger.LogInformation($"connectionString: {connectionString}");
        return connectionString;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        IEnumerable<TodoItem> list = Enumerable.Empty<TodoItem>();
        try
        {
            list = await _todoItemRepository.Get();
            return Ok(list);
        }
        catch(Exception ex)
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
            var item = await _todoItemRepository.Get(id);

            if (item is null)
            {
                _logger.LogWarning("The item with id={id} was not found", id);
                return NotFound();
            }

            return Ok(item);
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
            await _todoItemRepository.Add(title);
            return Ok();
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
        try
        {
            var item = await _todoItemRepository.Get(todoItem.Id);
            if (item is null)
            {
                _logger.LogWarning("The item with id={id} was not found", todoItem.Id);
                return NotFound();
            }

            await _todoItemRepository.Update(todoItem.Id, todoItem.Title, todoItem.IsCompleted);
            return Ok();
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
            var item = await _todoItemRepository.Get(id);
            if (item is null)
            {
                _logger.LogWarning("The item with id={id} was not found", id);
                return NotFound();
            }

            await _todoItemRepository.Delete(id);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(500);
        }
    }
}