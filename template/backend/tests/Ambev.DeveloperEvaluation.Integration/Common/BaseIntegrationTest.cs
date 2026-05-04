using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Ambev.DeveloperEvaluation.Integration.Fixtures;
using Ambev.DeveloperEvaluation.ORM;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Ambev.DeveloperEvaluation.Integration.Common;

public abstract class BaseIntegrationTest : IAsyncLifetime
{
    protected readonly HttpClient Client;
    protected readonly DefaultContext Db;
    private readonly IntegrationWebApplicationFactory _factory;
    private readonly IServiceScope _scope;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    protected BaseIntegrationTest(IntegrationWebApplicationFactory factory)
    {
        _factory = factory;
        Client = factory.CreateClient();
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", TestConstants.GenerateToken());

        _scope = factory.Services.CreateScope();
        Db = _scope.ServiceProvider.GetRequiredService<DefaultContext>();
    }

    protected HttpClient CreateUnauthenticatedClient() => _factory.CreateClient();

    public Task InitializeAsync() => CleanDatabaseAsync();

    public async Task DisposeAsync()
    {
        await CleanDatabaseAsync();
        _scope.Dispose();
    }

    protected virtual async Task CleanDatabaseAsync()
    {
        Db.SaleItems.RemoveRange(Db.SaleItems);
        Db.Sales.RemoveRange(Db.Sales);
        await Db.SaveChangesAsync();
    }

    protected StringContent JsonContent<T>(T body) =>
        new(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

    protected async Task<ApiResponse<T>> ReadAsync<T>(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ApiResponse<T>>(json, JsonOptions)!;
    }

    protected async Task<ApiResponse> ReadAsync(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ApiResponse>(json, JsonOptions)!;
    }
}
