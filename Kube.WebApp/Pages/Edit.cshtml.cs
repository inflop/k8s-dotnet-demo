using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using Kube.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Kube.WebApp.Pages;

public class EditModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly HttpClient _httpClient;

    public EditModel(ILogger<IndexModel> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient("TodoApi");
    }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if ((id ?? 0) <= 0)
        {
            Item = new TodoItem();
            return Page();
        }
            
        var response = await _httpClient.GetAsync($"/Todo/{id.Value}");

        if (!response.IsSuccessStatusCode)
            return StatusCode((int)response.StatusCode);

        Item = await response.Content.ReadFromJsonAsync<TodoItem>();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        bool isNew = Item.Id <= 0;
        var response = isNew ? await Add() : await Update();

        if (!response.IsSuccessStatusCode)
            return StatusCode((int)response.StatusCode);

        return RedirectToPage("./Index");
    }

    private async Task<HttpResponseMessage> Add()
    {
        return await _httpClient.PostAsync($"/Todo?title={Item.Title}", null);
    }

    private async Task<HttpResponseMessage> Update()
    {
        var content = new StringContent(JsonSerializer.Serialize(Item), Encoding.UTF8, "application/json");
        return await _httpClient.PutAsync($"/Todo", content);
    }

    [BindProperty]
    public TodoItem Item { get; set; }
}