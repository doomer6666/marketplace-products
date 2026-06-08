using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Marketplace.Products.Api.Interceptors;

public class LoggingInterceptor(ILogger<LoggingInterceptor> logger) : Interceptor
{
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
        where TRequest : class
        where TResponse : class
    {
        logger.LogInformation($"Метод: {context.Method}. Запрос: {request}");

        var response = await continuation(request, context);

        logger.LogInformation($"Метод {context.Method} выполнен. Ответ: {response}");

        return response;
    }
}