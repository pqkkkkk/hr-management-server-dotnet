using HrManagement.Api.Modules.Activity.Domain.Filter;
using HrManagement.Api.Shared.DTOs;

namespace HrManagement.Api.Modules.Activity.Domain.Dao;

/// <summary>
/// Data access interface for ActivityLog entity.
/// </summary>
public interface IActivityLogDao
{
    /// <summary>
    /// Gets an activity log by ID with related entities (Participant -> Activity).
    /// </summary>
    Task<Entities.ActivityLog?> GetByIdAsync(string id);

    /// <summary>
    /// Gets activity logs with filtering and pagination.
    /// </summary>
    Task<PagedResult<Entities.ActivityLog>> GetAllAsync(ActivityLogFilter filter);

    /// <summary>
    /// Gets activity logs by participant ID.
    /// </summary>
    Task<IEnumerable<Entities.ActivityLog>> GetByParticipantIdAsync(string participantId);

    /// <summary>
    /// Creates a new activity log.
    /// </summary>
    Task<Entities.ActivityLog> CreateAsync(Entities.ActivityLog log);

    /// <summary>
    /// Updates an existing activity log.
    /// </summary>
    Task UpdateAsync(Entities.ActivityLog log);

    /// <summary>
    /// Deletes an activity log by ID.
    /// </summary>
    Task DeleteAsync(string id);

    /// <summary>
    /// Gets total distance logged by a participant on a specific date.
    /// </summary>
    Task<decimal> GetTotalDistanceByDateAsync(string participantId, DateTime date);
}
