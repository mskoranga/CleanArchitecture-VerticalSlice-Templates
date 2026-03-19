namespace CleanArchitecture.VerticalSlice.Application.Abstractions;

public interface IFeatureService
{
    Task<bool> IsEnabledAsync(string featureName, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> GetEnabledFeaturesAsync(CancellationToken cancellationToken = default);
}
