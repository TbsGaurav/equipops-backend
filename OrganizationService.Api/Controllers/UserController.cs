using Microsoft.AspNetCore.Mvc;

using OrganizationService.Api.Helpers.ResponseHelpers.Enums;
using OrganizationService.Api.Helpers.ResponseHelpers.Handlers;
using OrganizationService.Api.Services.Interface;
using OrganizationService.Api.ViewModels.Request.User;
using OrganizationService.Api.ViewModels.Response.User;

namespace OrganizationService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController(ILogger<UserController> _logger, IUserService _userService) : ControllerBase
    {
        [HttpPost("createUpdate")]
        public async Task<IActionResult> CreateOrUpdateUser(UserCreateUpdateRequestViewModel request)
        {
            _logger.LogInformation("API hit: CreateOrUpdateUser. Email={Email}", request.Email);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                _logger.LogWarning("Validation failed for Email={Email}", request.Email);
                return BadRequest(ResponseHelper<string>.Error(
                    "Validation failed",
                    errors: errors,
                    statusCode: StatusCodeEnum.BAD_REQUEST
                ));
            }

            var result = await _userService.CreateUpdateUserAsync(request);
            _logger.LogInformation(
                "Service response for Email={Email}: Success={Success}",
                request.Email,
                result.Success
            );

            if (!result.Success)
            {
                return Conflict(ResponseHelper<string>.Error(
                    result.Message ?? "User create/update failed.",
                    statusCode: StatusCodeEnum.CONFLICT_OCCURS
                ));
            }

            if (result.Data == null)
            {
                return Conflict(ResponseHelper<string>.Error(
                    "User create/update failed.",
                    statusCode: StatusCodeEnum.CONFLICT_OCCURS
                ));
            }
            return Ok(ResponseHelper<UserCreateUpdateResponseViewModel>.Success(
                result.Message ?? "User created successfully.",
                result.Data
            ));

        }
        [HttpGet("getById")]
        public async Task<IActionResult> GetUserById(Guid? Id)
        {
            _logger.LogInformation(
                "API hit: GetUserById. Id={Id}", Id);
            var result = await _userService.GetUserByIdAsync(Id);

            if (!result.Success || result.Data == null)
            {
                return NotFound(ResponseHelper<string>.Error(
                    result.Message ?? "No user found.",
                    statusCode: StatusCodeEnum.NOT_FOUND
                ));
            }
            return Ok(ResponseHelper<UserData>.Success(
                "User fetched successfully.",
                result.Data
            ));
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetUserList(string? search, int length = 10, int page = 1, string orderColumn = "name", string orderDirection = "Asc", string role = "", bool? is_active = null)
        {
            _logger.LogInformation(
                "API hit: GetUserList. Search={Search}, Page={Page}, Length={Length}",
                search, page, length);
            var result = await _userService.GetUserListAsync(
                search, length, page, orderColumn, orderDirection, role, is_active);

            if (!result.Success || result.Data == null)
            {
                return NotFound(ResponseHelper<string>.Error(
                    result.Message ?? "No users found.",
                    statusCode: StatusCodeEnum.NOT_FOUND
                ));
            }
            return Ok(ResponseHelper<UserListResponseViewModel>.Success(
                "User list fetched successfully.",
                result.Data
            ));
        }

        [HttpPost("delete")]
        public async Task<IActionResult> DeleteUser([FromBody] UserDeleteRequestViewModel request)
        {
            _logger.LogInformation("API hit: DeleteUser. UserId={UserId}", request.Id);

            if (request.Id == Guid.Empty)
            {
                _logger.LogWarning("Validation failed: UserId is required");
                return BadRequest(ResponseHelper<string>.Error(
                    "UserId is required",
                    statusCode: StatusCodeEnum.BAD_REQUEST
                ));
            }

            var result = await _userService.DeleteUserAsync(request);

            if (!result.Success || result.Data == null)
            {
                return NotFound(ResponseHelper<string>.Error(
                    result.Message ?? "User deletion failed or user not found.",
                    statusCode: StatusCodeEnum.NOT_FOUND
                ));
            }

            return Ok(ResponseHelper<UserListResponseViewModel>.Success(
                result.Message ?? "User deleted successfully.",
                result.Data
            ));
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetUserProfile([FromQuery] string? id)
        {
            _logger.LogInformation("API hit: GetUserProfile | Id={Id}", id);

            if (string.IsNullOrWhiteSpace(id))
            {
                _logger.LogWarning("GetUserProfile: Invalid or missing user Id.");

                return BadRequest(ResponseHelper<string>.Error(
                    "user Id is required.",
                    statusCode: StatusCodeEnum.BAD_REQUEST
                ));
            }
            var result = await _userService.GetUserProfile(id);

            if (!result.Success)
            {
                _logger.LogWarning("GetUserProfile failed for Id={Id} | Message={Message}", id, result.Message);

                return BadRequest(ResponseHelper<string>.Error(
                    result.Message ?? "Failed to retrieve user profile.",
                    statusCode: StatusCodeEnum.BAD_REQUEST
                ));
            }

            if (result.Data == null)
            {
                _logger.LogWarning("GetUserProfile: No user profile found for Id={Id}", id);

                return NotFound(ResponseHelper<string>.Error(
                    "user profile not found.",
                    statusCode: StatusCodeEnum.NOT_FOUND
                ));
            }

            _logger.LogInformation("GetUserProfile success for Id={Id}", id);

            return Ok(ResponseHelper<UserData>.Success(
                result.Message ?? "user profile retrieved successfully.",
                result.Data
            ));

        }
    }

}
