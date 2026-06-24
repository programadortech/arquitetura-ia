using System.Reflection;
using NetArchTest.Rules;
using Plataforma2A.Auth.Application.Abstractions;
using Plataforma2A.Auth.Domain.Common;

namespace Plataforma2A.Auth.ArchitectureTests;

/// <summary>Impõe a regra de dependência da Clean Architecture e bane MediatR/AutoMapper.</summary>
public class DependencyRuleTests
{
    private static readonly Assembly DomainAssembly = typeof(Entity<>).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(IUseCaseDispatcher).Assembly;

    [Fact]
    public void Domain_nao_depende_de_outras_camadas()
    {
        var result = Types.InAssembly(DomainAssembly)
            .Should()
            .NotHaveDependencyOnAny(
                "Plataforma2A.Auth.Application",
                "Plataforma2A.Auth.Infrastructure",
                "Plataforma2A.Auth.Api")
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Application_nao_depende_de_Infrastructure()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .Should()
            .NotHaveDependencyOn("Plataforma2A.Auth.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Nenhum_tipo_referencia_MediatR_nem_AutoMapper()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .Should()
            .NotHaveDependencyOnAny("MediatR", "AutoMapper")
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join(", ", result.FailingTypeNames ?? []));
    }
}
