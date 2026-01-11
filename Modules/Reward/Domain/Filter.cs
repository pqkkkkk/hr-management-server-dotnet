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
    public string? EmployeeId { get; set; }
    public string? SourceWalletId { get; set; }
    public string? DestinationWalletId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public HrManagement.Api.Modules.Reward.Domain.Entities.RewardEnums.TransactionType? TransactionType { get; set; }
}
