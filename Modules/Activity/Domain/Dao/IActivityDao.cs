using HrManagement.Api.Modules.Activity.Domain.Filter;
using HrManagement.Api.Shared.DTOs;

namespace HrManagement.Api.Modules.Activity.Domain.Dao;

/// <summary>
/// Data access interface for Activity entity.
/// </summary>
public interface IActivityDao
{
    /// <summary>
    /// Gets an activity by ID with related entities (Participants).
    /// </summary>
    Task<Entities.Activity?> GetByIdAsync(string id);

    /// <summary>
    /// Gets all activities with filtering and pagination.
    /// </summary>
    Task<PagedResult<Entities.Activity>> GetAllAsync(ActivityFilter filter);

    /// <summary>
    /// Gets activities that an employee has joined.
    /// </summary>
    Task<PagedResult<Entities.Activity>> GetByEmployeeIdAsync(string employeeId, ActivityFilter filter);

    /// <summary>
    /// Creates a new activity.
    /// </summary>
    Task<Entities.Activity> CreateAsync(Entities.Activity activity);

    /// <summary>
    /// Updates an existing activity.
    /// </summary>
    Task UpdateAsync(Entities.Activity activity);

    /// <summary>
    /// Deletes an activity by ID.
    /// </summary>
    Task DeleteAsync(string id);
}
