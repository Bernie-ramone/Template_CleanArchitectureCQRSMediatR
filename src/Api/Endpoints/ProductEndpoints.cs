using CleanCqrs.Api.Errors;
using CleanCqrs.Application.Products.CreateProduct;
using CleanCqrs.Application.Products.GetProductById;
using MediatR;

namespace CleanCqrs.Api.Endpoints;

/// <summary>
/// HTTP endpoints for products.
/// </summary>
internal static class ProductEndpoints
{
    private const string GetProductByIdRoute = "GetProductById";

    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products").WithTags("Products");

        group.MapPost("/", async (CreateProductRequest request, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new CreateProductCommand(request.Name, request.Price), cancellationToken);

            return result.IsSuccess
                ? Results.CreatedAtRoute(GetProductByIdRoute, new { id = result.Value }, new { id = result.Value })
                : result.ToProblem();
        });

        group.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetProductByIdQuery(id), cancellationToken);

            return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblem();
        })
        .WithName(GetProductByIdRoute);

        return app;
    }
}

/// <summary>
/// HTTP request body for creating a product. Kept separate from the command so the API contract can evolve independently.
/// </summary>
/// <param name="Name">Unique product name.</param>
/// <param name="Price">Unit price.</param>
internal sealed record CreateProductRequest(string Name, decimal Price);
