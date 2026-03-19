using CleanArchitecture.VerticalSlice.Application.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;

namespace CleanArchitecture.VerticalSlice.Infrastructure.FeatureManagement;

public class FeatureService : IFeatureService
{
    private readonly IFeatureManager _featureManager;
    private readonly ILogger<FeatureService> _logger;

    public FeatureService(IFeatureManager featureManager, ILogger<FeatureService> logger)
    {
        _featureManager = featureManager;
        _logger = logger;
    }

    public async Task<bool> IsEnabledAsync(string featureName, CancellationToken cancellationToken = default)
    {
        var isEnabled = await _featureManager.IsEnabledAsync(featureName);
        
        _logger.LogDebug(
            "Feature {FeatureName} is {Status}",
            featureName,
            isEnabled ? "enabled" : "disabled");
        
        return isEnabled;
    }

    public async Task<IEnumerable<string>> GetEnabledFeaturesAsync(CancellationToken cancellationToken = default)
    {
        var features = new List<string>();
        
        await foreach (var featureName in _featureManager.GetFeatureNamesAsync())
        {
            if (await _featureManager.IsEnabledAsync(featureName))
            {
                features.Add(featureName);
            }
        }
        
        return features;
    }
}
