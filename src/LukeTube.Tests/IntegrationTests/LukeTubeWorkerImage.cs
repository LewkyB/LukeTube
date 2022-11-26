using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Xunit;

namespace LukeTube.Tests.IntegrationTests;

[UsedImplicitly]
public sealed class LukeTubeWorkerImage : IDockerImage, IAsyncLifetime
{
    // static LukeTubeWorkerImage()
    // {
    //     TestcontainersSettings.Logger = new MyLogger();
    // }
    //
    // public sealed class MyLogger : ILogger, IDisposable
    // {
    //     public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    //     {
    //         File.AppendAllText("diagnosticWorker.log", formatter.Invoke(state, exception));
    //     }
    //
    //     public bool IsEnabled(LogLevel logLevel)
    //     {
    //         return true;
    //     }
    //
    //     public IDisposable BeginScope<TState>(TState state)
    //     {
    //         return this;
    //     }
    //
    //     public void Dispose()
    //     {
    //     }
    // }
    // TODO: set this up with HTTPS
    // public const ushort HttpPort = 82;

    private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);

    private readonly IDockerImage _image = new DockerImage(string.Empty, "luketube-worker", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());

    public async Task InitializeAsync()
    {
        await _semaphoreSlim.WaitAsync()
            .ConfigureAwait(false);

        try
        {
            _ = await new ImageFromDockerfileBuilder()
                .WithName(this)
                .WithDockerfileDirectory(CommonDirectoryPath.GetSolutionDirectory(), ".")
                .WithDockerfile("LukeTubeWorker.Dockerfile")
                .WithBuildArgument("RESOURCE_REAPER_SESSION_ID", ResourceReaper.DefaultSessionId.ToString("D")) // https://github.com/testcontainers/testcontainers-dotnet/issues/602.
                .WithDeleteIfExists(false)
                .Build()
                .ConfigureAwait(false);
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    public string Repository => _image.Repository;

    public string Name => _image.Name;

    public string Tag => _image.Tag;

    public string FullName => _image.FullName;

    public string GetHostname()
    {
        return _image.GetHostname();
    }
}