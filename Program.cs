using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace AzureRedisEvents
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services.AddHostedService<RedisMaintenanceListener>();
                })
                .Build()
                .RunAsync();
        }
    }
}
