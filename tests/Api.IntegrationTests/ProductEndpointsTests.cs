using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CleanCqrs.Api.IntegrationTests;

public sealed class ProductEndpointsTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private const string ProblemJson = "application/problem+json";
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Post_WithValidProduct_Returns201AndTheProductCanBeRead()
    {
        var name = UniqueName();

        var created = await _client.PostAsJsonAsync("/api/products", new { name, price = 49.90m });

        created.StatusCode.Should().Be(HttpStatusCode.Created);
        created.Headers.Location.Should().NotBeNull();

        var product = await _client.GetFromJsonAsync<ProductDto>(created.Headers.Location);
        product!.Name.Should().Be(name);
        product.Price.Should().Be(49.90m);
    }

    [Fact]
    public async Task Post_WithInvalidProduct_Returns400ValidationProblemDetails()
    {
        var response = await _client.PostAsJsonAsync("/api/products", new { name = "", price = 0m });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType!.MediaType.Should().Be(ProblemJson);

        var problem = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>();
        problem!.Status.Should().Be(StatusCodes.Status400BadRequest);
        problem.Errors.Keys.Should().BeEquivalentTo("Name", "Price");
    }

    [Fact]
    public async Task Post_WithDuplicateName_Returns409ProblemDetails()
    {
        var product = new { name = UniqueName(), price = 10m };
        (await _client.PostAsJsonAsync("/api/products", product)).EnsureSuccessStatusCode();

        var response = await _client.PostAsJsonAsync("/api/products", product);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        response.Content.Headers.ContentType!.MediaType.Should().Be(ProblemJson);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem!.Extensions["errorCode"]!.ToString().Should().Be("Product.DuplicateName");
    }

    [Fact]
    public async Task Get_WhenProductDoesNotExist_Returns404ProblemDetails()
    {
        var response = await _client.GetAsync($"/api/products/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType!.MediaType.Should().Be(ProblemJson);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem!.Status.Should().Be(StatusCodes.Status404NotFound);
    }

    // Tests share one database per class, so names must not collide.
    private static string UniqueName() => $"Product {Guid.NewGuid():N}";

    private sealed record ProductDto(Guid Id, string Name, decimal Price);
}
