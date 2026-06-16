using System.Text.Json;
using Confluent.Kafka;
using Marketplace.Products.Application;
using Marketplace.Products.Domain.Events;
using Microsoft.Extensions.Hosting;

namespace Marketplace.Products.Infrastructure.Implementation;

public class KafkaProductSyncWorker(
    string bootstrapServers,
    IProductSearchWriter searchRepository) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = "elasticsearch-sync-group",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe("product-sync-topic");

        while (!stoppingToken.IsCancellationRequested)
        {
            var consumeResult = consumer.Consume(stoppingToken);
            var messageValue = consumeResult.Message.Value;

            var syncEvent = JsonSerializer.Deserialize<ProductSyncEvent>(messageValue);

            if (syncEvent is null)
            {
                continue;
            }

            switch (syncEvent.Action)
            {
                case EventAction.Create:
                case EventAction.Update:
                    if (syncEvent.Product is not null)
                    {
                        await searchRepository.IndexProductAsync(syncEvent.Product);
                    }

                    break;

                case EventAction.Delete:
                    await searchRepository.DeleteProductAsync(syncEvent.Id);
                    break;
            }
        }

        consumer.Close();
    }
}