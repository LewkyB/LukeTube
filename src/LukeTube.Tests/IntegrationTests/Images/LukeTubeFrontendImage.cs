using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;
using JetBrains.Annotations;
using Xunit;

namespace LukeTube.Tests.IntegrationTests.Images;

[UsedImplicitly]
public sealed class LukeTubeFrontendImage : IDockerImage, IAsyncLifetime
{
    // TODO: set this up with HTTPS
    public const ushort HttpPort = 81;

    private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);

    private readonly IDockerImage _image = new DockerImage(string.Empty, "luketube-frontend", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());

    public async Task InitializeAsync()
    {
        await _semaphoreSlim.WaitAsync()
            .ConfigureAwait(false);

        try
        {
            _ = await new ImageFromDockerfileBuilder()
                .WithName(this)
                .WithDockerfileDirectory(CommonDirectoryPath.GetSolutionDirectory(), "LukeTube/client")
                .WithDockerfile("Dockerfile")
                // TODO: make sure reaper is setup right for front end container
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