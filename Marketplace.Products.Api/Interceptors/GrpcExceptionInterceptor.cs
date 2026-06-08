using FluentValidation;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Marketplace.Products.Api.Interceptors;

public class GrpcExceptionInterceptor(ILogger<GrpcExceptionInterceptor> logger) : Interceptor
{
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            return await continuation(request, context);
        }
        catch (ValidationException ex)
        {
            var errors = string.Join("; ", ex.Errors.Select(e => e.ErrorMessage));
            logger.LogWarning($"Ошибка валидации gRPC: {errors}");

            throw new RpcException(new Status(StatusCode.InvalidArgument, errors));
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogWarning($"Товар не найден: {ex.Message}");

            throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Критическая ошибка gRPC");

            throw new RpcException(new Status(StatusCode.Internal, "Внутренняя ошибка сервера"));
        }
    }
}