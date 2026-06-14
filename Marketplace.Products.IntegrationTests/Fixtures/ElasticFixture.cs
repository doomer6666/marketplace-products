using DotNet.Testcontainers.Builders;
using Elastic.Clients.Elasticsearch;
using Testcontainers.Elasticsearch;

namespace Marketplace.Products.IntegrationTests.Fixtures;

public class ElasticFixture : IAsyncLifetime
{
    private readonly ElasticsearchContainer _container;

    public ElasticFixture()
    {
        _container = new ElasticsearchBuilder("docker.elastic.co/elasticsearch/elasticsearch:8.17.0")
                     .WithEnvironment("xpack.security.enabled", "false")
                     .WithEnvironment("discovery.type", "single-node")
                     .WithEnvironment("ES_JAVA_OPTS", "-Xms512m -Xmx512m")
                     .WithEnvironment("node.store.allow_mmap", "false")
                     .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(r => r.ForPort(9200)))
                     .Build();
    }

    public ElasticsearchClient Client { get; private set; } = null!;
    public string ConnectionString => $"http://{_container.Hostname}:{_container.GetMappedPublicPort(9200)}";

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        var settings = new ElasticsearchClientSettings(new Uri(ConnectionString));
        Client = new ElasticsearchClient(settings);
    }

    public async Task DisposeAsync() => await _container.DisposeAsync();

    public async Task ClearDatabase()
    {
        var existsResponse = await Client.Indices.ExistsAsync("products");
        if (existsResponse.Exists)
        {
            await Client.Indices.DeleteAsync("products");
        }
    }

    public async Task ForceRefresh() => await Client.Indices.RefreshAsync("products");
}