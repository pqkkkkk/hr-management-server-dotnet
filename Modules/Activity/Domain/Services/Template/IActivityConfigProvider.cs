using System.Text.Json;
using HrManagement.Api.Modules.Activity.Domain.Entities;
using static HrManagement.Api.Modules.Activity.Domain.Entities.ActivityEnums;

namespace HrManagement.Api.Modules.Activity.Domain.Services.Template;

// Type alias to avoid namespace conflict with ActivityLog folder
using ActivityLogEntity = HrManagement.Api.Modules.Activity.Domain.Entities.ActivityLog;

/// <summary>
/// Interface for activity configuration providers (Strategy pattern).
/// Each activity type/template implements this to provide its own schema, validation, and point calculation logic.
/// </summary>
public interface IActivityConfigProvider
{
    /// <summary>
    /// Unique identifier for this template (e.g., "RUNNING_SIMPLE")
    /// </summary>
    string TemplateId { get; }

    /// <summary>
    /// Display name for this template
    /// </summary>
    string TemplateName { get; }

    /// <summary>
    /// Activity type this template belongs to
    /// </summary>
    ActivityType Type { get; }

    /// <summary>
    /// Description of this template
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Get the configuration schema for FE to render dynamic form
    /// </summary>
    ConfigSchema GetConfigSchema();

    /// <summary>
    /// Validate activity config against this template's schema.
    /// Called when creating/updating an activity.
    /// </summary>
    /// <param name="config">The config JSON to validate</param>
    /// <returns>Validation result with success/failure status and error message</returns>
    ValidationResult ValidateConfig(JsonDocument? config);

    /// <summary>
    /// Validate an activity log submission against the activity config.
    /// Config is accessed via log.Participant.Activity.Config navigation.
    /// </summary>
    /// <param name="log">The activity log to validate (with navigation properties loaded)</param>
    /// <returns>Validation result with success/failure status and error message</returns>
    ValidationResult ValidateLog(ActivityLogEntity log);

    /// <summary>
    /// Calculate points for an approved activity log.
    /// Config is accessed via log.Participant.Activity.Config navigation.
    /// </summary>
    /// <param name="log">The activity log to calculate points for (with navigation properties loaded)</param>
    /// <returns>Points earned</returns>
    int CalculatePoints(ActivityLogEntity log);
}
