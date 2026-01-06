using HrManagement.Api.Modules.Activity.Domain.Dao;

namespace HrManagement.Api.Modules.Activity.Domain.Services.Participant;

/// <summary>
/// Query service implementation for Participant operations.
/// </summary>
public class ParticipantQueryServiceImpl : IParticipantQueryService
{
    private readonly IParticipantDao _participantDao;

    public ParticipantQueryServiceImpl(IParticipantDao participantDao)
    {
        _participantDao = participantDao;
    }

    public async Task<Entities.Participant?> GetParticipantStatsAsync(string activityId, string employeeId)
    {
        return await _participantDao.GetByActivityAndEmployeeWithStatsAsync(activityId, employeeId);
    }
}
