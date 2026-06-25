using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Common;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Plataforma2ASmart.Auth.Application;

namespace Plataforma2ASmart.Auth.UnitTests;

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
        services.AddApplication();
        services.AddScoped<IUseCase<PingRequest, Result<string>>, PingHandler>();

        using var provider = services.BuildServiceProvider();
        var dispatcher = provider.GetRequiredService<IUseCaseDispatcher>();

        var result = await dispatcher.SendAsync(new PingRequest("oi"));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("pong: oi");
    }
}
