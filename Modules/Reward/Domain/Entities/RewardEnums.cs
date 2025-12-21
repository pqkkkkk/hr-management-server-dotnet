namespace HrManagement.Api.Modules.Reward.Domain.Entities;

/// <summary>
/// Enums for Reward module
/// </summary>
public static class RewardEnums
{
    /// <summary>
    /// Types of automatic point distribution policies
    /// </summary>
    public enum PolicyType
    {
        /// <summary>
        /// Reward for days with no late check-in (lateMinutes == 0)
        /// </summary>
        NOT_LATE,

        /// <summary>
        /// Reward for overtime work (based on overtimeMinutes)
        /// </summary>
        OVERTIME,

        /// <summary>
        /// Reward for full attendance (morning + afternoon both PRESENT)
        /// </summary>
        FULL_ATTENDANCE
    }

    /// <summary>
    /// Calculation period for policy evaluation
    /// Currently fixed to WEEKLY for simplicity
    /// </summary>
    public enum CalculationPeriod
    {
        WEEKLY
    }

    /// <summary>
    /// Types of point transactions
    /// </summary>
    public enum TransactionType
    {
        /// <summary>
        /// Manager gifts points to employee (uses giving_budget)
        /// </summary>
        GIFT,

        /// <summary>
        /// Employee exchanges points for reward items
        /// </summary>
        EXCHANGE,

        /// <summary>
        /// System auto-distributes points based on policy
        /// </summary>
        POLICY_REWARD
    }

    /// <summary>
    /// Status of reward program
    /// </summary>
    public enum ProgramStatus
    {
        ACTIVE,
        INACTIVE
    }
}
