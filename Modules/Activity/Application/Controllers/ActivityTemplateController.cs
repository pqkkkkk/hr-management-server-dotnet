using Microsoft.AspNetCore.Mvc;
using HrManagement.Api.Modules.Activity.Application.DTOs;
using HrManagement.Api.Modules.Activity.Domain.Services.Template;
using HrManagement.Api.Shared.DTOs;
using static HrManagement.Api.Modules.Activity.Domain.Entities.ActivityEnums;

namespace HrManagement.Api.Modules.Activity.Application.Controllers;

/// <summary>
/// API endpoints for activity templates.
/// </summary>
[ApiController]
[Route("api/v1/activity-templates")]
[Produces("application/json")]
[Microsoft.AspNetCore.Http.Tags("Activity Templates")]
public class ActivityTemplateController : ControllerBase
{
    private readonly ActivityTemplateRegistry _templateRegistry;

    public ActivityTemplateController(ActivityTemplateRegistry templateRegistry)
    {
        _templateRegistry = templateRegistry;
    }

    /// <summary>
    /// Gets all available activity templates.
    /// </summary>
    /// <remarks>
    /// Returns a list of all registered activity templates.
    /// Each template defines validation rules and point calculation logic.
    /// </remarks>
    /// <param name="type">Optional filter by activity type.</param>
    /// <returns>List of available templates.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<TemplateInfoResponse>>), StatusCodes.Status200OK)]
    public IActionResult GetAllTemplates([FromQuery] ActivityType? type = null)
    {
        var templates = type.HasValue
            ? _templateRegistry.GetTemplatesByType(type.Value)
            : _templateRegistry.GetAllTemplates();

        var response = templates.Select(TemplateInfoResponse.FromEntity).ToList();
        return Ok(ApiResponse<List<TemplateInfoResponse>>.Ok(response));
    }

    /// <summary>
    /// Gets the configuration schema for a template.
    /// </summary>
    /// <remarks>
    /// Returns the configuration schema that describes what fields
    /// are required when creating an activity with this template.
    /// Frontend uses this to render dynamic forms.
    /// </remarks>
    /// <param name="id">The template ID.</param>
    /// <returns>The configuration schema.</returns>
    /// <response code="200">Returns the configuration schema.</response>
    /// <response code="404">If the template is not found.</response>
    [HttpGet("{id}/schema")]
    [ProducesResponseType(typeof(ApiResponse<ConfigSchemaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public IActionResult GetTemplateSchema([FromRoute] string id)
    {
        var provider = _templateRegistry.GetProviderByTemplateId(id)
            ?? throw new KeyNotFoundException($"Template with ID {id} not found.");

        var schema = provider.GetConfigSchema();
        var response = ConfigSchemaResponse.FromEntity(schema);
        return Ok(ApiResponse<ConfigSchemaResponse>.Ok(response));
    }
}
