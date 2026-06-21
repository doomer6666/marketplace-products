using Testcontainers.Kafka;

namespace Marketplace.Products.IntegrationTests.Fixtures;

public class KafkaFixture : IAsyncLifetime
{
    private readonly KafkaContainer _container = new KafkaBuilder("confluentinc/cp-kafka:7.6.0").Build();

    public string BootstrapServers => _container.GetBootstrapAddress();

    public async Task InitializeAsync() => await _container.StartAsync();

    public async Task DisposeAsync() => await _container.DisposeAsync();
}