using System.Text.Json;
using Confluent.Kafka;
using FluentAssertions;
using Marketplace.Products.Application.DTOs;
using Marketplace.Products.Application.Events;
using Marketplace.Products.Domain;
using Marketplace.Products.Infrastructure.Implementation;
using Marketplace.Products.IntegrationTests.Fixtures;

namespace Marketplace.Products.IntegrationTests.Infrastructure;

public class KafkaProducerTests(KafkaFixture fixture) : IClassFixture<KafkaFixture>
{
    private readonly KafkaProducer _producer = new(fixture.BootstrapServers);

    [Fact]
    public async Task PublishMessage_ShouldDeliverMessageToTopic()
    {
        // Arrange
        const string topic = "test-sync-topic";
        var key = Guid.NewGuid().ToString();
        var message = new ProductMessageDto(Guid.Parse(key),
            "Test",
            "Test",
            10,
            1,
            ProductCategory.ELECTRONICS,
            DateTime.UtcNow,
            DateTime.UtcNow);
        var syncEvent =
            new ProductSyncEvent { Id = Guid.Parse(key), Action = EventAction.Create, MessageDto = message };

        var config = new ConsumerConfig
                     {
                         BootstrapServers = fixture.BootstrapServers,
                         GroupId = "test-group",
                         AutoOffsetReset = AutoOffsetReset.Earliest
                     };
        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe(topic);

        // Act
        await _producer.PublishMessageAsync(topic, key, syncEvent);

        // Assert
        var consumeResult = consumer.Consume(TimeSpan.FromSeconds(5));

        consumeResult.Should().NotBeNull("Сообщение должно было дойти до Kafka");
        consumeResult.Message.Key.Should().Be(key);

        var receivedEvent = JsonSerializer.Deserialize<ProductSyncEvent>(consumeResult.Message.Value);
        receivedEvent.Should().NotBeNull();
        receivedEvent!.Action.Should().Be(EventAction.Create);
        receivedEvent.MessageDto!.Name.Should().Be("Test");
    }
}