using System.Text.Json;
using Confluent.Kafka;
using Marketplace.Products.Application;

namespace Marketplace.Products.Infrastructure.Implementation;

public class KafkaProducer : IMessageProducer

{
    private readonly IProducer<string, string> _producer;

    public KafkaProducer(string bootstrapServers)
    {
        var config = new ProducerConfig { BootstrapServers = bootstrapServers };
        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task PublishMessageAsync<T>(string topic, string key, T message)
    {
        var kafkaMessage = new Message<string, string> { Key = key, Value = JsonSerializer.Serialize(message) };
        await _producer.ProduceAsync(topic, kafkaMessage);
    }

    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(10));
        _producer.Dispose();
    }
}