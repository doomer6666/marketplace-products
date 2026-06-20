namespace Marketplace.Products.Application;

public interface IDevToolsService
{
    public Task GenerateFakeProducts(int count);
}