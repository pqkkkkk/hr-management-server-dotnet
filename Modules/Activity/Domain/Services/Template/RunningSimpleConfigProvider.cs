using System.Text.Json;
using HrManagement.Api.Modules.Activity.Domain.Entities;
using static HrManagement.Api.Modules.Activity.Domain.Entities.ActivityEnums;

namespace HrManagement.Api.Modules.Activity.Domain.Services.Template;

// Type alias to avoid namespace conflict with ActivityLog folder
using ActivityLogEntity = HrManagement.Api.Modules.Activity.Domain.Entities.ActivityLog;

/// <summary>
/// Configuration provider for simple running activities.
/// Points = distance × points_per_km × weekend_bonus
/// </summary>
public class RunningSimpleConfigProvider : IActivityConfigProvider
{
    public string TemplateId => "RUNNING_SIMPLE";
    public string TemplateName => "Running - Simple Points";
    public ActivityType Type => ActivityType.RUNNING;
    public string Description => "Tính điểm đơn giản dựa trên số km chạy. Có hệ số nhân điểm cho ngày cuối tuần.";

    public ConfigSchema GetConfigSchema()
    {
        return new ConfigSchema
        {
            Fields = new List<ConfigField>
            {
                new("min_pace", "number", "Pace tối thiểu (phút/km)", 4.0, true,
                    "Pace tối thiểu cho phép. Pace = thời gian / khoảng cách"),
                new("max_pace", "number", "Pace tối đa (phút/km)", 15.0, true,
                    "Pace tối đa cho phép"),
                new("min_distance_per_log", "number", "Khoảng cách tối thiểu/lần (km)", 1.0, true,
                    "Khoảng cách tối thiểu cho mỗi lần ghi nhận"),
                new("max_distance_per_day", "number", "Khoảng cách tối đa/ngày (km)", 42.0, true,
                    "Khoảng cách tối đa được tính điểm trong một ngày"),
                new("points_per_km", "number", "Điểm/km", 10, true,
                    "Số điểm nhận được cho mỗi km chạy"),
                new("bonus_weekend_multiplier", "number", "Hệ số cuối tuần", 1.5, true,
                    "Hệ số nhân điểm cho hoạt động vào Thứ 7 và Chủ Nhật")
            }
        };
    }

    public ValidationResult ValidateConfig(JsonDocument? config)
    {
        var schema = GetConfigSchema();
        var requiredFields = schema.Fields.Where(f => f.Required).ToList();

        // Config is required if template has required fields
        if (requiredFields.Count > 0 && config == null)
        {
            return ValidationResult.Fail("Activity config is required for this template.");
        }

        if (config == null) return ValidationResult.Success();

        try
        {
            var root = config.RootElement;

            // Validate all required fields are present
            foreach (var field in requiredFields)
            {
                if (!root.TryGetProperty(field.Name, out var prop))
                {
                    return ValidationResult.Fail($"Required config field '{field.Name}' ({field.Label}) is missing.");
                }

                // Check for null/undefined values
                if (prop.ValueKind == JsonValueKind.Null || prop.ValueKind == JsonValueKind.Undefined)
                {
                    return ValidationResult.Fail($"Required config field '{field.Name}' ({field.Label}) cannot be null.");
                }

                // Validate number fields have valid numeric values
                if (field.Type == "number" && prop.ValueKind != JsonValueKind.Number)
                {
                    return ValidationResult.Fail($"Config field '{field.Name}' ({field.Label}) must be a number.");
                }
            }

            // Validate specific business rules for this template
            var minPace = GetDecimalValue(root, "min_pace", 4.0m);
            var maxPace = GetDecimalValue(root, "max_pace", 15.0m);
            if (minPace >= maxPace)
            {
                return ValidationResult.Fail("Pace tối thiểu phải nhỏ hơn pace tối đa.");
            }

            var minDistance = GetDecimalValue(root, "min_distance_per_log", 1.0m);
            var maxDistance = GetDecimalValue(root, "max_distance_per_day", 42.0m);
            if (minDistance >= maxDistance)
            {
                return ValidationResult.Fail("Khoảng cách tối thiểu phải nhỏ hơn khoảng cách tối đa.");
            }

            var pointsPerKm = GetDecimalValue(root, "points_per_km", 10m);
            if (pointsPerKm <= 0)
            {
                return ValidationResult.Fail("Điểm/km phải lớn hơn 0.");
            }

            var weekendMultiplier = GetDecimalValue(root, "bonus_weekend_multiplier", 1.5m);
            if (weekendMultiplier < 1)
            {
                return ValidationResult.Fail("Hệ số cuối tuần phải >= 1.");
            }

            return ValidationResult.Success();
        }
        catch (Exception ex)
        {
            return ValidationResult.Fail($"Config validation error: {ex.Message}");
        }
    }

    public ValidationResult ValidateLog(ActivityLogEntity log)
    {
        try
        {
            var config = log.Participant.Activity.Config;
            if (config == null)
            {
                return ValidationResult.Fail("Activity config không được cấu hình");
            }

            var root = config.RootElement;

            // Get config values with defaults
            var minPace = GetDecimalValue(root, "min_pace", 4.0m);
            var maxPace = GetDecimalValue(root, "max_pace", 15.0m);
            var minDistance = GetDecimalValue(root, "min_distance_per_log", 1.0m);
            var maxDistancePerDay = GetDecimalValue(root, "max_distance_per_day", 42.0m);

            // Validate distance
            if (log.Distance < minDistance)
            {
                return ValidationResult.Fail($"Khoảng cách tối thiểu là {minDistance} km");
            }

            if (log.Distance > maxDistancePerDay)
            {
                return ValidationResult.Fail($"Khoảng cách tối đa mỗi ngày là {maxDistancePerDay} km");
            }

            // Validate pace (minutes per km)
            if (log.Distance > 0 && log.DurationMinutes > 0)
            {
                var pace = (decimal)log.DurationMinutes / log.Distance;
                if (pace < minPace)
                {
                    return ValidationResult.Fail($"Pace quá nhanh ({pace:F2} phút/km). Pace tối thiểu: {minPace} phút/km");
                }
                if (pace > maxPace)
                {
                    return ValidationResult.Fail($"Pace quá chậm ({pace:F2} phút/km). Pace tối đa: {maxPace} phút/km");
                }
            }

            return ValidationResult.Success();
        }
        catch (Exception ex)
        {
            return ValidationResult.Fail($"Lỗi validation: {ex.Message}");
        }
    }

    public int CalculatePoints(ActivityLogEntity log)
    {
        try
        {
            var config = log.Participant.Activity.Config;
            if (config == null)
            {
                // Fallback: simple calculation
                return (int)Math.Floor(log.Distance * 10);
            }

            var root = config.RootElement;

            var pointsPerKm = GetDecimalValue(root, "points_per_km", 10m);
            var weekendMultiplier = GetDecimalValue(root, "bonus_weekend_multiplier", 1.5m);
            var maxDistancePerDay = GetDecimalValue(root, "max_distance_per_day", 42.0m);

            // Cap distance at max per day
            var cappedDistance = Math.Min(log.Distance, maxDistancePerDay);

            // Calculate base points
            var basePoints = cappedDistance * pointsPerKm;

            // Apply weekend bonus
            if (log.LogDate.DayOfWeek == DayOfWeek.Saturday || log.LogDate.DayOfWeek == DayOfWeek.Sunday)
            {
                basePoints *= weekendMultiplier;
            }

            return (int)Math.Floor(basePoints);
        }
        catch
        {
            // Fallback: simple calculation
            return (int)Math.Floor(log.Distance * 10);
        }
    }

    private static decimal GetDecimalValue(JsonElement element, string propertyName, decimal defaultValue)
    {
        if (element.TryGetProperty(propertyName, out var prop))
        {
            return prop.ValueKind switch
            {
                JsonValueKind.Number => prop.GetDecimal(),
                JsonValueKind.String => decimal.TryParse(prop.GetString(), out var val) ? val : defaultValue,
                _ => defaultValue
            };
        }
        return defaultValue;
    }
}
