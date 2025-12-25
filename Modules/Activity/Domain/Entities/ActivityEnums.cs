namespace HrManagement.Api.Modules.Activity.Domain.Entities;

/// <summary>
/// Enums for Activity module
/// </summary>
public static class ActivityEnums
{
    /// <summary>
    /// Types of activities supported
    /// </summary>
    public enum ActivityType
    {
        RUNNING
    }

    /// <summary>
    /// Status of an activity
    /// </summary>
    public enum ActivityStatus
    {
        /// <summary>
        /// Activity is being configured, not visible to employees
        /// </summary>
        DRAFT,

        /// <summary>
        /// Activity is open for registration
        /// </summary>
        OPEN,

        /// <summary>
        /// Activity has started, participants can submit logs
        /// </summary>
        IN_PROGRESS,

        /// <summary>
        /// Activity registration/submission is closed
        /// </summary>
        CLOSED,

        /// <summary>
        /// Activity is completed and finalized
        /// </summary>
        COMPLETED
    }

    /// <summary>
    /// Status of a participant in an activity
    /// </summary>
    public enum ParticipantStatus
    {
        /// <summary>
        /// Waiting for approval to join
        /// </summary>
        PENDING,

        /// <summary>
        /// Active participant
        /// </summary>
        ACTIVE,

        /// <summary>
        /// Removed from activity
        /// </summary>
        DISQUALIFIED
    }

    /// <summary>
    /// Status of an activity log submission
    /// </summary>
    public enum ActivityLogStatus
    {
        /// <summary>
        /// Awaiting manager review
        /// </summary>
        PENDING,

        /// <summary>
        /// Approved by manager
        /// </summary>
        APPROVED,

        /// <summary>
        /// Rejected by manager
        /// </summary>
        REJECTED
    }
}
