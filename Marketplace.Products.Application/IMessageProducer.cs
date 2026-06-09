namespace Marketplace.Products.Application;

public interface IMessageProducer
{
    public Task PublishMessageAsync<T>(string topic, string key, T message);
}