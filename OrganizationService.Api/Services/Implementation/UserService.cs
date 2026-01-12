
using Common.Services.Services.Interface;

using OrganizationService.Api.Infrastructure.Interface;
using OrganizationService.Api.Services.Interface;
using OrganizationService.Api.ViewModels.Request.User;
using OrganizationService.Api.ViewModels.Response;
using OrganizationService.Api.ViewModels.Response.User;

namespace OrganizationService.Api.Services.Implementation
{
    public class UserService(ILogger<UserService> _logger, IUserRepository _userRepository, IEmailService _emailService) : IUserService
    {
        public async Task<ApiResponse<UserCreateUpdateResponseViewModel>> CreateUpdateUserAsync(
            UserCreateUpdateRequestViewModel model)
        {
            _logger.LogInformation("UserService: CreateUpdate START. Email={Email}", model.Email);
            if (string.IsNullOrWhiteSpace(model.Email) ||
                string.IsNullOrWhiteSpace(model.First_Name))
            {
                _logger.LogWarning(
                    "Validation failed: Required fields missing. Email={Email}",
                    model.Email);

                return new ApiResponse<UserCreateUpdateResponseViewModel>
                {
                    Success = false,
                    Message = "Email and First Name are required."
                };
            }

            _logger.LogInformation("Calling UserRepository.CreateUpdate for Email={Email}", model.Email);
            var data = await _userRepository.CreateUpdateUserAsync(model);
            if (data?.Id == Guid.Empty)
            {
                _logger.LogWarning(
                    "Create/Update failed. No User returned. Email={Email}",
                    model.Email);
                return new ApiResponse<UserCreateUpdateResponseViewModel>
                {
                    Success = false,
                    Message = "User create/update failed.",
                    Data = data
                };
            }

            _logger.LogInformation(
                "User create/update successful. UserId={UserId}, Email={Email}",
                data.Id, data.Email);
            if (!model.Id.HasValue || model.Id == Guid.Empty)  // New user
            {
                await _emailService.SendUserWelcomeEmailAsync(data.Email, model.First_Name, data.Password);
            }

            return new ApiResponse<UserCreateUpdateResponseViewModel>
            {
                Success = true,
                Message = model.Id == Guid.Empty
                    ? "User created successfully."
                    : "User updated successfully.",
                Data = data
            };
        }

        public async Task<ApiResponse<UserListResponseViewModel>> DeleteUserAsync(UserDeleteRequestViewModel request)
        {

            _logger.LogInformation("UserService: Deleting user with UserId={UserId}", request.Id);

            // Call repository to delete the user
            await _userRepository.DeleteUserAsync(request);

            // fetch updated list after deletion
            var updatedList = await _userRepository.GetUserListAsync(
                Search: null,
                Length: 10,
                Page: 1,
                OrderColumn: "first_name",
                OrderDirection: "Asc"
            );
            return new ApiResponse<UserListResponseViewModel>
            {
                Success = true,
                Message = "User deleted successfully.",
                Data = updatedList
            };
        }

        public async Task<ApiResponse<UserData>> GetUserByIdAsync(Guid? Id)
        {
            _logger.LogInformation(
                "UserService: Fetching user. Id={Id}", Id);

            var data = await _userRepository.GetUserByIdAsync(Id);

            return new ApiResponse<UserData>
            {
                Success = true,
                Message = "User fetched successfully.",
                Data = data
            };
        }

        public async Task<ApiResponse<UserListResponseViewModel>> GetUserListAsync(string? Search, int Length, int Page, string OrderColumn, string OrderDirection = "Asc", string role = "", bool? isActive = null)
        {

            _logger.LogInformation(
                "UserService: Fetching user list. Search={Search}, Page={Page}, Length={Length}",
                Search, Page, Length);

            var data = await _userRepository.GetUserListAsync(Search, Length, Page, OrderColumn, OrderDirection, role, isActive);

            return new ApiResponse<UserListResponseViewModel>
            {
                Success = true,
                Message = "User list fetched successfully.",
                Data = data
            };
        }

        public async Task<ApiResponse<UserData>> GetUserProfile(string id)
        {
            _logger.LogInformation("Service: GetUserProfile START for Id={Id}", id);

            var data = await _userRepository.GetUserProfile(id);

            if (data == null)
            {
                _logger.LogWarning("Service: No user found for Id={Id}", id);

                return new ApiResponse<UserData>
                {
                    Success = false,
                    Message = "user not found."
                };
            }

            _logger.LogInformation("Service: user profile fetched successfully for Id={Id}", id);

            return new ApiResponse<UserData>
            {
                Success = true,
                Message = "user profile fetched successfully.",
                Data = data
            };
        }

    }
}
