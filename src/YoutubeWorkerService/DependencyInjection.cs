using System.Threading.Channels;

namespace YoutubeWorkerService;

public static class DependencyInjection
{
    public static IServiceCollection AddUnboundedChannel<T>(this IServiceCollection services)
    {
        services.AddSingleton(Channel.CreateUnbounded<T>(new UnboundedChannelOptions { SingleReader = true }));
        services.AddSingleton<ChannelReader<T>>(svc => svc.GetRequiredService<Channel<T>>().Reader);
        services.AddSingleton<ChannelWriter<T>>(svc => svc.GetRequiredService<Channel<T>>().Writer);

        return services;
    }
}