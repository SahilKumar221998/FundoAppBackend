using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Consumer
{
    public class RabbitMqConsumerService:BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RabbitMqConsumerService> _logger;

        public RabbitMqConsumerService(IServiceProvider serviceProvider, ILogger<RabbitMqConsumerService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("RabbitMqConsumerService is starting.");

            await Task.Run(() =>
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var consumer = scope.ServiceProvider.GetRequiredService<RabbitMqConsumer>();
                    consumer.StartConsuming(stoppingToken);
                }
            }, stoppingToken);

            _logger.LogInformation("RabbitMqConsumerService is stopping.");
        }
    }
}
