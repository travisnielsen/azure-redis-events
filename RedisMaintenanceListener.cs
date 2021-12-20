using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using StackExchange.Redis.Maintenance;

namespace AzureRedisEvents
{
    public class RedisMaintenanceListener : IHostedService
    {
        private readonly ILogger _logger;
        private readonly string _redisConnectionString;

        public RedisMaintenanceListener(ILogger<RedisMaintenanceListener> logger, IConfiguration config)
        {
            _logger = logger;
            _redisConnectionString = config.GetSection("RedisConnection").Value;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("RedisMaintenanceListener is starting.");
            ConsumeEvents(_redisConnectionString);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("RedisMaintenanceListener is stopping.");
            return Task.CompletedTask;
        }

        private void ConsumeEvents(string connectionString)
        {
            try
            {
                ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(connectionString);
                _logger.LogInformation("Successful connection to Redis");

                redis.ServerMaintenanceEvent += (object sender, ServerMaintenanceEvent e) =>
                {
                    if (e is AzureMaintenanceEvent azureEvent)
                    {
                        string eventMessage;

                        if (azureEvent.StartTimeUtc != null)
                        {
                            eventMessage = $"Azure Redis Maintenance Event {azureEvent.NotificationTypeString} starts at {azureEvent.StartTimeUtc}. IP address={azureEvent.IPAddress}. Is replica={azureEvent.IsReplica}";
                        }
                        else
                        {
                            eventMessage = $"Azure Redis Maintenance Event {azureEvent.NotificationTypeString}. IP address={azureEvent.IPAddress}. Is replica={azureEvent.IsReplica}";
                        }

                        _logger.LogWarning(eventMessage);
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }
    }
}