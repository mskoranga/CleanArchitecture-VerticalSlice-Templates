namespace CleanArchitecture.VerticalSlice.Application.Abstractions
{
    public interface IHandler<in TRequest, TResponse>
    {
        Task<TResponse> HandleAsync(TRequest command, CancellationToken cancellationToken);
    }
}
