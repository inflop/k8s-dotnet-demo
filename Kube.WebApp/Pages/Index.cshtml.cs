using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Kube.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Kube.WebApp.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly HttpClient _httpClient;

    public IndexModel(ILogger<IndexModel> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient("TodoApi");
    }

    public async Task OnGetAsync()
    {
        await RefreshList();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"/Todo/{id}");

        if (!response.IsSuccessStatusCode)
            return StatusCode((int)response.StatusCode);

        return RedirectToPage();
    }

    private async Task RefreshList()
    {
        var response = await _httpClient.GetAsync("/Todo");
        List = await response.Content.ReadFromJsonAsync<IEnumerable<TodoItem>>();
    }

    [BindProperty]
    public IEnumerable<TodoItem> List { get; private set; }
}