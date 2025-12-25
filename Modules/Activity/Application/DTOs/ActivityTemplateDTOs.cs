using HrManagement.Api.Modules.Activity.Domain.Entities;
using static HrManagement.Api.Modules.Activity.Domain.Entities.ActivityEnums;

namespace HrManagement.Api.Modules.Activity.Application.DTOs;

#region Response DTOs

/// <summary>
/// Response DTO for template information.
/// </summary>
public record TemplateInfoResponse
{
    public string TemplateId { get; init; } = string.Empty;
    public string TemplateName { get; init; } = string.Empty;
    public ActivityType Type { get; init; }
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Creates a response from a TemplateInfo entity.
    /// </summary>
    public static TemplateInfoResponse FromEntity(TemplateInfo entity)
    {
        return new TemplateInfoResponse
        {
            TemplateId = entity.TemplateId,
            TemplateName = entity.TemplateName,
            Type = entity.Type,
            Description = entity.Description
        };
    }
}

/// <summary>
/// Response DTO for configuration field.
/// </summary>
public record ConfigFieldResponse
{
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = "number";
    public string Label { get; init; } = string.Empty;
    public object? DefaultValue { get; init; }
    public bool Required { get; init; }
    public string? Description { get; init; }

    /// <summary>
    /// Creates a response from a ConfigField entity.
    /// </summary>
    public static ConfigFieldResponse FromEntity(ConfigField entity)
    {
        return new ConfigFieldResponse
        {
            Name = entity.Name,
            Type = entity.Type,
            Label = entity.Label,
            DefaultValue = entity.DefaultValue,
            Required = entity.Required,
            Description = entity.Description
        };
    }
}

/// <summary>
/// Response DTO for configuration schema.
/// </summary>
public record ConfigSchemaResponse
{
    public List<ConfigFieldResponse> Fields { get; init; } = new();

    /// <summary>
    /// Creates a response from a ConfigSchema entity.
    /// </summary>
    public static ConfigSchemaResponse FromEntity(ConfigSchema entity)
    {
        return new ConfigSchemaResponse
        {
            Fields = entity.Fields.Select(ConfigFieldResponse.FromEntity).ToList()
        };
    }
}

#endregion
