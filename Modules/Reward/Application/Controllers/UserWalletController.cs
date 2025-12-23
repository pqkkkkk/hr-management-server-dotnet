using Microsoft.AspNetCore.Mvc;
using HrManagement.Api.Modules.Reward.Application.DTOs;
using HrManagement.Api.Shared.DTOs;
using HrManagement.Api.Modules.Reward.Domain.Services.UserWalletServices;

namespace HrManagement.Api.Modules.Reward.Application.Controllers;

/// <summary>
/// API endpoints for managing user wallets in reward programs.
/// </summary>
[ApiController]
[Route("api/rewards/wallets")]
[Produces("application/json")]
[Microsoft.AspNetCore.Http.Tags("User Wallets")]
public class UserWalletController : ControllerBase
{
    private readonly IUserWalletQueryService _userWalletQueryService;

    public UserWalletController(IUserWalletQueryService userWalletQueryService)
    {
        _userWalletQueryService = userWalletQueryService;
    }
    /// <summary>
    /// Gets a specific wallet for a user in a reward program.
    /// </summary>
    /// <remarks>
    /// Retrieves the wallet information for a specific user in a specific reward program,
    /// including personal point balance and giving budget (for managers).
    /// </remarks>
    /// <param name="userId">The user ID.</param>
    /// <param name="programId">The reward program ID.</param>
    /// <returns>The user's wallet in the specified program.</returns>
    /// <response code="200">Returns the wallet information.</response>
    /// <response code="404">If the wallet is not found.</response>
    [HttpGet("user/{userId}/program/{programId}")]
    [ProducesResponseType(typeof(ApiResponse<UserWalletResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWalletByUserAndProgram(
        [FromRoute] string userId,
        [FromRoute] string programId)
    {
        var userWallet = await _userWalletQueryService.GetWalletByUserAndProgramAsync(userId, programId);

        var response = UserWalletResponse.FromEntity(userWallet == null ?
            throw new KeyNotFoundException("Wallet not found.")
            : userWallet);

        return Ok(ApiResponse<UserWalletResponse>.Ok(response));
    }
}
