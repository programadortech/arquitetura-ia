using NetArchTest.Rules;
using Plataforma2ASmart.Auth.Domain.Common;

namespace Plataforma2ASmart.Auth.ArchitectureTests;

public class DependencyRuleTests
{
    private static readonly System.Reflection.Assembly Domain = typeof(Entity<>).Assembly;
    private static readonly System.Reflection.Assembly Application = typeof(Auth.Application.DependencyInjection).Assembly;
    private static readonly System.Reflection.Assembly Infrastructure = typeof(Auth.Infrastructure.DependencyInjection).Assembly;

    private const string ApplicationNs = "Plataforma2ASmart.Auth.Application";
    private const string InfrastructureNs = "Plataforma2ASmart.Auth.Infrastructure";
    private const string ApiNs = "Plataforma2ASmart.Auth.Api";

    [Fact]
    public void Domain_nao_depende_de_outras_camadas()
    {
        var result = Types.InAssembly(Domain)
            .Should()
            .NotHaveDependencyOnAny(ApplicationNs, InfrastructureNs, ApiNs)
            .GetResult();

        Assert.True(result.IsSuccessful, Describe(result));
    }

    [Fact]
    public void Application_nao_depende_de_Infrastructure_nem_Api()
    {
        var result = Types.InAssembly(Application)
            .Should()
            .NotHaveDependencyOnAny(InfrastructureNs, ApiNs)
            .GetResult();

        Assert.True(result.IsSuccessful, Describe(result));
    }

    [Fact]
    public void Infrastructure_nao_depende_de_Api()
    {
        var result = Types.InAssembly(Infrastructure)
            .Should()
            .NotHaveDependencyOnAny(ApiNs)
            .GetResult();

        Assert.True(result.IsSuccessful, Describe(result));
    }

    private static string Describe(TestResult result)
        => result.IsSuccessful ? "" : "Violações: " + string.Join(", ", result.FailingTypeNames ?? []);
}
