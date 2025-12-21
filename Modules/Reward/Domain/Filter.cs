using System;

namespace HrManagement.Api.Modules.Reward.Domain.Filter;



public class RewardProgramFilter
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? NameContains { get; set; }
    public bool? IsActive { get; set; }
}

public class PointTransactionFilter
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public Guid? EmployeeId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
