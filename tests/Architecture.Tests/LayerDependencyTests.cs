using System.Reflection;
using CleanCqrs.Application;
using CleanCqrs.Domain.Products;
using NetArchTest.Rules;

namespace CleanCqrs.Architecture.Tests;

public sealed class LayerDependencyTests
{
    private const string ApplicationNamespace = "CleanCqrs.Application";
    private const string InfrastructureNamespace = "CleanCqrs.Infrastructure";
    private const string ApiNamespace = "CleanCqrs.Api";

    private static readonly Assembly DomainAssembly = typeof(Product).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(Application.DependencyInjection).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(Infrastructure.DependencyInjection).Assembly;

    [Fact]
    public void Domain_DependsOnNothing()
    {
        // The csproj has no references at all, so checking the other layers and the
        // frameworks most likely to creep in is enough.
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                ApplicationNamespace,
                InfrastructureNamespace,
                ApiNamespace,
                "MediatR",
                "FluentValidation",
                "Microsoft.EntityFrameworkCore",
                "Microsoft.AspNetCore")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(Because(result));

        // Belt and braces: the compiled assembly may reference the base class library only.
        DomainAssembly.GetReferencedAssemblies()
            .Select(a => a.Name)
            .Should().OnlyContain(name => name != null && name.StartsWith("System.", StringComparison.Ordinal));
    }

    [Fact]
    public void Application_DoesNotDependOnInfrastructureOrApi()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(InfrastructureNamespace, ApiNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(Because(result));
    }

    [Fact]
    public void Infrastructure_DoesNotDependOnApi()
    {
        var result = Types.InAssembly(InfrastructureAssembly)
            .ShouldNot()
            .HaveDependencyOn(ApiNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(Because(result));
    }

    private static string Because(TestResult result) =>
        $"these types break the rule: {string.Join(", ", result.FailingTypeNames ?? [])}";
}
