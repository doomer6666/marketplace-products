using System.Text.Json;
using Confluent.Kafka;
using Marketplace.Products.Application;

namespace Marketplace.Products.Infrastructure.Implementation;

public class KafkaProducer(string bootstrapServers) : IMessageProducer
{
    public async Task PublishMessageAsync<T>(string topic, string key, T message)
    {
        var config = new ProducerConfig { BootstrapServers = bootstrapServers };

        using var producer = new ProducerBuilder<string, string>(config).Build();

        var kafkaMessage = new Message<string, string> { Key = key, Value = JsonSerializer.Serialize(message) };

        await producer.ProduceAsync(topic, kafkaMessage);
    }
}