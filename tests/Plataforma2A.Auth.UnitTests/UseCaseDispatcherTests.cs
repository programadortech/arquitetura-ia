using Microsoft.Extensions.DependencyInjection;
using Plataforma2A.Auth.Application;
using Plataforma2A.Auth.Application.Abstractions;
using Plataforma2A.Auth.Application.Common;

namespace Plataforma2A.Auth.UnitTests;

public sealed record PingRequest(string Message) : IUseCaseRequest<Result<string>>;

public sealed class PingHandler : IUseCase<PingRequest, Result<string>>
{
    public Task<Result<string>> HandleAsync(PingRequest request, CancellationToken cancellationToken)
        => Task.FromResult(Result<string>.Success($"pong: {request.Message}"));
}

public class UseCaseDispatcherTests
{
    [Fact]
    public async Task Dispatcher_resolve_e_invoca_o_handler_retornando_Result()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddApplication(typeof(PingHandler).Assembly);

        using var provider = services.BuildServiceProvider();
        var dispatcher = provider.GetRequiredService<IUseCaseDispatcher>();

        var result = await dispatcher.SendAsync(new PingRequest("oi"));

        Assert.True(result.IsSuccess);
        Assert.Equal("pong: oi", result.Value);
    }

    [Fact]
    public void Result_Failure_carrega_erros_e_marca_falha()
    {
        var result = Result<string>.Failure(new Error("teste.erro", "deu ruim", ErrorType.Validation));

        Assert.True(result.IsFailure);
        Assert.Single(result.Errors);
        Assert.Equal("teste.erro", result.Errors[0].Code);
    }
}
