namespace HrManagement.Api.Modules.Activity.Domain.Entities;

/// <summary>
/// Represents a field in the configuration schema that FE will use to render form
/// </summary>
public class ConfigField
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "number"; // number, string, boolean
    public string Label { get; set; } = string.Empty;
    public object? DefaultValue { get; set; }
    public bool Required { get; set; } = true;
    public string? Description { get; set; }

    public ConfigField() { }

    public ConfigField(string name, string type, string label, object? defaultValue = null, bool required = true, string? description = null)
    {
        Name = name;
        Type = type;
        Label = label;
        DefaultValue = defaultValue;
        Required = required;
        Description = description;
    }
}

/// <summary>
/// Configuration schema returned to FE for rendering dynamic form
/// </summary>
public class ConfigSchema
{
    public List<ConfigField> Fields { get; set; } = new();
}

/// <summary>
/// Result of validating an activity log
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }

    public static ValidationResult Success() => new() { IsValid = true };
    public static ValidationResult Fail(string message) => new() { IsValid = false, ErrorMessage = message };
}

/// <summary>
/// Template info for API responses
/// </summary>
public class TemplateInfo
{
    public string TemplateId { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public ActivityEnums.ActivityType Type { get; set; }
    public string Description { get; set; } = string.Empty;
}
