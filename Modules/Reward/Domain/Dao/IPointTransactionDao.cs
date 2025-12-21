using System;

namespace hr_management_dotnet.Modules.Reward.Domain.Dao;

public interface IPointTransactionDao
{
    public Task<PointTransaction> GetPointTransactionAsync(Guid id);
    public Task<PointTransaction> CreatePointTransactionAsync(PointTransaction pointTransaction);
}
