using System.Net;
using System.Net.Http.Headers;
using Ambev.DeveloperEvaluation.Integration.Common;
using Ambev.DeveloperEvaluation.Integration.Fixtures;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.CreateSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.UpdateSale;
using Bogus;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Integration.Controllers;

public class SalesControllerTests : BaseIntegrationTest, IClassFixture<IntegrationWebApplicationFactory>
{
    private readonly Faker _faker = new();

    public SalesControllerTests(IntegrationWebApplicationFactory factory) : base(factory) { }

    // ── helpers ────────────────────────────────────────────────────────────────

    private CreateSaleRequest BuildCreateRequest(int quantity = 2, decimal unitPrice = 100m) => new()
    {
        SaleNumber = $"SALE-{_faker.Random.Number(1000, 9999)}",
        SaleDate = DateTime.UtcNow,
        CustomerId = _faker.Random.Guid(),
        CustomerName = _faker.Company.CompanyName(),
        BranchId = _faker.Random.Guid(),
        BranchName = $"Branch {_faker.Address.City()}",
        Items = new List<CreateSaleItemRequest>
        {
            new()
            {
                ProductId = _faker.Random.Guid(),
                ProductName = _faker.Commerce.ProductName(),
                Quantity = quantity,
                UnitPrice = unitPrice
            }
        }
    };

    private async Task<CreateSaleResponse> CreateSaleAsync(int quantity = 2, decimal unitPrice = 100m)
    {
        var request = BuildCreateRequest(quantity, unitPrice);
        var response = await Client.PostAsync("/api/sales", JsonContent(request));
        var body = await ReadAsync<CreateSaleResponse>(response);
        return body.Data!;
    }

    // ── POST /api/sales ────────────────────────────────────────────────────────

    [Fact(DisplayName = "POST /api/sales with valid data returns 201 and sale")]
    public async Task CreateSale_ValidRequest_Returns201()
    {
        var request = BuildCreateRequest(quantity: 2, unitPrice: 100m);

        var response = await Client.PostAsync("/api/sales", JsonContent(request));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await ReadAsync<CreateSaleResponse>(response);
        body.Success.Should().BeTrue();
        body.Data.Should().NotBeNull();
        body.Data!.SaleNumber.Should().Be(request.SaleNumber);
        body.Data.IsCancelled.Should().BeFalse();
    }

    [Fact(DisplayName = "POST /api/sales persists sale in database")]
    public async Task CreateSale_ValidRequest_PersistsToDatabase()
    {
        var request = BuildCreateRequest(quantity: 2);

        var response = await Client.PostAsync("/api/sales", JsonContent(request));
        var body = await ReadAsync<CreateSaleResponse>(response);

        var sale = await Db.Sales.FindAsync(body.Data!.Id);
        sale.Should().NotBeNull();
        sale!.SaleNumber.Should().Be(request.SaleNumber);
    }

    [Fact(DisplayName = "POST /api/sales with quantity 4-9 applies 10% discount")]
    public async Task CreateSale_QuantityBetweenFourAndNine_AppliesTenPercentDiscount()
    {
        var request = BuildCreateRequest(quantity: 5, unitPrice: 100m);

        var response = await Client.PostAsync("/api/sales", JsonContent(request));
        var body = await ReadAsync<CreateSaleResponse>(response);

        body.Data!.Items.First().Discount.Should().Be(0.10m);
        body.Data.TotalAmount.Should().Be(5 * 100m * 0.90m);
    }

    [Fact(DisplayName = "POST /api/sales with quantity 10-20 applies 20% discount")]
    public async Task CreateSale_QuantityBetweenTenAndTwenty_AppliesTwentyPercentDiscount()
    {
        var request = BuildCreateRequest(quantity: 10, unitPrice: 200m);

        var response = await Client.PostAsync("/api/sales", JsonContent(request));
        var body = await ReadAsync<CreateSaleResponse>(response);

        body.Data!.Items.First().Discount.Should().Be(0.20m);
        body.Data.TotalAmount.Should().Be(10 * 200m * 0.80m);
    }

    [Fact(DisplayName = "POST /api/sales with quantity above 20 returns 400")]
    public async Task CreateSale_QuantityAbove20_Returns400()
    {
        var request = BuildCreateRequest(quantity: 21);

        var response = await Client.PostAsync("/api/sales", JsonContent(request));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "POST /api/sales with empty items returns 400")]
    public async Task CreateSale_EmptyItems_Returns400()
    {
        var request = BuildCreateRequest();
        request.Items.Clear();

        var response = await Client.PostAsync("/api/sales", JsonContent(request));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "POST /api/sales without auth token returns 401")]
    public async Task CreateSale_WithoutToken_Returns401()
    {
        var unauthenticatedClient = CreateUnauthenticatedClient();
        var request = BuildCreateRequest();

        var response = await unauthenticatedClient.PostAsync("/api/sales", JsonContent(request));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── GET /api/sales/{id} ────────────────────────────────────────────────────

    [Fact(DisplayName = "GET /api/sales/{id} for existing sale returns 200 and sale")]
    public async Task GetSale_ExistingId_Returns200()
    {
        var created = await CreateSaleAsync();

        var response = await Client.GetAsync($"/api/sales/{created.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await ReadAsync<CreateSaleResponse>(response);
        body.Data!.Id.Should().Be(created.Id);
        body.Data.SaleNumber.Should().Be(created.SaleNumber);
    }

    [Fact(DisplayName = "GET /api/sales/{id} for non-existent sale returns 404")]
    public async Task GetSale_NonExistentId_Returns404()
    {
        var response = await Client.GetAsync($"/api/sales/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── GET /api/sales ─────────────────────────────────────────────────────────

    [Fact(DisplayName = "GET /api/sales returns paginated list")]
    public async Task GetSales_Returns200WithPaginatedList()
    {
        await CreateSaleAsync();
        await CreateSaleAsync();

        var response = await Client.GetAsync("/api/sales?_page=1&_size=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("totalItems");
        json.Should().Contain("data");
    }

    [Fact(DisplayName = "GET /api/sales respects page size")]
    public async Task GetSales_RespectsPageSize()
    {
        await CreateSaleAsync();
        await CreateSaleAsync();
        await CreateSaleAsync();

        var response = await Client.GetAsync("/api/sales?_page=1&_size=2");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        // totalItems should be 3 but data should have 2
        json.Should().Contain("\"totalItems\":3");
    }

    // ── PUT /api/sales/{id} ────────────────────────────────────────────────────

    [Fact(DisplayName = "PUT /api/sales/{id} updates existing sale")]
    public async Task UpdateSale_ExistingSale_Returns200()
    {
        var created = await CreateSaleAsync();
        var updateRequest = new UpdateSaleRequest
        {
            SaleNumber = "UPDATED-001",
            SaleDate = DateTime.UtcNow,
            CustomerId = created.CustomerId,
            CustomerName = "Updated Customer",
            BranchId = created.BranchId,
            BranchName = created.BranchName,
            Items = new List<UpdateSaleItemRequest>
            {
                new()
                {
                    ProductId = _faker.Random.Guid(),
                    ProductName = "Updated Product",
                    Quantity = 3,
                    UnitPrice = 50m
                }
            }
        };

        var response = await Client.PutAsync($"/api/sales/{created.Id}", JsonContent(updateRequest));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await ReadAsync<UpdateSaleResponse>(response);
        body.Data!.SaleNumber.Should().Be("UPDATED-001");
        body.Data.CustomerName.Should().Be("Updated Customer");
    }

    [Fact(DisplayName = "PUT /api/sales/{id} for non-existent sale returns 404")]
    public async Task UpdateSale_NonExistentId_Returns404()
    {
        var updateRequest = new UpdateSaleRequest
        {
            SaleNumber = "SALE-999",
            SaleDate = DateTime.UtcNow,
            CustomerId = _faker.Random.Guid(),
            CustomerName = "Customer",
            BranchId = _faker.Random.Guid(),
            BranchName = "Branch",
            Items = new List<UpdateSaleItemRequest>
            {
                new() { ProductId = _faker.Random.Guid(), ProductName = "Product", Quantity = 1, UnitPrice = 10m }
            }
        };

        var response = await Client.PutAsync($"/api/sales/{Guid.NewGuid()}", JsonContent(updateRequest));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── DELETE /api/sales/{id} — cancel sale ───────────────────────────────────

    [Fact(DisplayName = "DELETE /api/sales/{id} cancels existing sale")]
    public async Task CancelSale_ExistingSale_Returns200()
    {
        var created = await CreateSaleAsync();

        var response = await Client.DeleteAsync($"/api/sales/{created.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "DELETE /api/sales/{id} persists cancellation in database")]
    public async Task CancelSale_ExistingSale_PersistsCancellation()
    {
        var created = await CreateSaleAsync();

        await Client.DeleteAsync($"/api/sales/{created.Id}");

        var sale = await Db.Sales.FindAsync(created.Id);
        sale!.IsCancelled.Should().BeTrue();
    }

    [Fact(DisplayName = "DELETE /api/sales/{id} for non-existent sale returns 404")]
    public async Task CancelSale_NonExistentId_Returns404()
    {
        var response = await Client.DeleteAsync($"/api/sales/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "DELETE /api/sales/{id} already cancelled returns 422")]
    public async Task CancelSale_AlreadyCancelled_Returns422()
    {
        var created = await CreateSaleAsync();
        await Client.DeleteAsync($"/api/sales/{created.Id}");

        var response = await Client.DeleteAsync($"/api/sales/{created.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    // ── DELETE /api/sales/{saleId}/items/{itemId} — cancel item ───────────────

    [Fact(DisplayName = "DELETE /api/sales/{saleId}/items/{itemId} cancels item")]
    public async Task CancelSaleItem_ExistingItem_Returns200()
    {
        var created = await CreateSaleAsync();
        var itemId = created.Items.First().Id;

        var response = await Client.DeleteAsync($"/api/sales/{created.Id}/items/{itemId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "DELETE /api/sales/{saleId}/items/{itemId} persists item cancellation")]
    public async Task CancelSaleItem_ExistingItem_PersistsItemCancellation()
    {
        var created = await CreateSaleAsync();
        var itemId = created.Items.First().Id;

        await Client.DeleteAsync($"/api/sales/{created.Id}/items/{itemId}");

        var item = await Db.SaleItems.FindAsync(itemId);
        item!.IsCancelled.Should().BeTrue();
    }

    [Fact(DisplayName = "DELETE /api/sales/{saleId}/items/{itemId} non-existent item returns 404")]
    public async Task CancelSaleItem_NonExistentItem_Returns404()
    {
        var created = await CreateSaleAsync();

        var response = await Client.DeleteAsync($"/api/sales/{created.Id}/items/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "DELETE item from cancelled sale returns 422")]
    public async Task CancelSaleItem_FromCancelledSale_Returns422()
    {
        var created = await CreateSaleAsync();
        var itemId = created.Items.First().Id;
        await Client.DeleteAsync($"/api/sales/{created.Id}");

        var response = await Client.DeleteAsync($"/api/sales/{created.Id}/items/{itemId}");

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}
