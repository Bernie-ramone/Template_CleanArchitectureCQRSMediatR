using CleanCqrs.Domain.Products;
using FluentValidation;

namespace CleanCqrs.Application.Products.CreateProduct;

/// <summary>
/// Validates <see cref="CreateProductCommand"/> input.
/// </summary>
public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    /// <summary>Defines the validation rules.</summary>
    public CreateProductCommandValidator()
    {
        RuleFor(c => c.Name).NotEmpty().MaximumLength(Product.NameMaxLength);
        RuleFor(c => c.Price).GreaterThan(0);
    }
}
