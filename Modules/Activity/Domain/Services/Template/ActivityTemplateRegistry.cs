using static HrManagement.Api.Modules.Activity.Domain.Entities.ActivityEnums;

namespace HrManagement.Api.Modules.Activity.Domain.Services.Template;

/// <summary>
/// Registry for activity config providers.
/// Collects all IActivityConfigProvider implementations via DI and provides methods to query them.
/// </summary>
public class ActivityTemplateRegistry
{
    private readonly IEnumerable<IActivityConfigProvider> _providers;
    private readonly Dictionary<string, IActivityConfigProvider> _providerMap;

    public ActivityTemplateRegistry(IEnumerable<IActivityConfigProvider> providers)
    {
        _providers = providers;
        _providerMap = providers.ToDictionary(p => p.TemplateId, p => p);
    }

    /// <summary>
    /// Get all available templates
    /// </summary>
    public IEnumerable<TemplateInfo> GetAllTemplates()
    {
        return _providers.Select(p => new TemplateInfo
        {
            TemplateId = p.TemplateId,
            TemplateName = p.TemplateName,
            Type = p.Type,
            Description = p.Description
        });
    }

    /// <summary>
    /// Get templates by activity type
    /// </summary>
    public IEnumerable<TemplateInfo> GetTemplatesByType(ActivityType type)
    {
        return _providers
            .Where(p => p.Type == type)
            .Select(p => new TemplateInfo
            {
                TemplateId = p.TemplateId,
                TemplateName = p.TemplateName,
                Type = p.Type,
                Description = p.Description
            });
    }

    /// <summary>
    /// Get provider by template ID
    /// </summary>
    public IActivityConfigProvider? GetProviderByTemplateId(string templateId)
    {
        return _providerMap.TryGetValue(templateId, out var provider) ? provider : null;
    }

    /// <summary>
    /// Check if a template exists
    /// </summary>
    public bool TemplateExists(string templateId)
    {
        return _providerMap.ContainsKey(templateId);
    }
}
