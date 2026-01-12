using Common.Services.Helper;

using SettingService.Api.Infrastructure.Interface;
using SettingService.Api.Services.Interface;
using SettingService.Api.ViewModels.Request;
using SettingService.Api.ViewModels.Response.UserAccessRole;
using SettingService.Api.ViewModels.Response.UserRole;
//using SettingService.Api.ViewModels.Response;

namespace SettingService.Api.Services.Implementation
{
    public class UserRoleService : IUserRoleService
    {
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly ILogger<UserRoleService> _logger;
        public UserRoleService(IUserRoleRepository userRoleRepository, ILogger<UserRoleService> logger)
        {
            _userRoleRepository = userRoleRepository;
            _logger = logger;
        }

        #region User Role
        public async Task<ApiResponse<UserRoleResponseViewModel>> UserRoleCreateAsync(UserRoleRequestViewModel model)
        {
            //🔹 Repository Call

            var data = await _userRoleRepository.UserRoleCreateAsync(model);

            string Message = "";
            bool Status = false;
            int Code = 0;
            if (data.id == null)
            {
                Code = (int)ApiStatusCode.BadRequest;
                Message = "Invalid data";
            }
            else
            {
                Code = (int)ApiStatusCode.Success;
                Status = true;
                if (model.id == null)
                    Message = "User role is inserted successfully.";
                else
                    Message = "User role is updated successfully.";
            }

            return new ApiResponse<UserRoleResponseViewModel>
            {
                StatusCode = Code,
                Success = Status,
                Message = Message,
                Data = data
            };
        }
        public async Task<ApiResponse<UserRoleListResponseViewModel>> UserRoleListAsync(string? search, bool? Is_Active, int length, int page, string orderColumn, string orderDirection)
        {
            string Message = "";
            bool Status = false;
            int Code = (int)ApiStatusCode.BadRequest;
            //🔹 Validate Input
            //if (string.IsNullOrEmpty(model.name))
            //{

            //    return new ApiResponse<UserRoleResponseViewModel>
            //    {
            //        Success = false,
            //        Message = "name cannot be empty."
            //    };
            //}

            //🔹 Repository Call
            var data = await _userRoleRepository.UserRoleListAsync(search, Is_Active, length, page, orderColumn, orderDirection);

            if (data == null)
            {
                Code = (int)ApiStatusCode.BadRequest;
                Message = "Invalid data.";
            }
            else
            {
                Code = (int)ApiStatusCode.Success;
                Status = true;
                Message = "Success.";
            }

            return new ApiResponse<UserRoleListResponseViewModel>
            {
                StatusCode = Code,
                Success = Status,
                Message = Message,
                Data = data
            };
        }
        public async Task<ApiResponse<UserRoleDeleteResponseViewModel>> UserRoleDeleteAsync(UserRoleDeleteRequestViewModel model)
        {
            //🔹 Repository Call

            var data = await _userRoleRepository.UserRoleDeleteAsync(model);
            int Code = (int)ApiStatusCode.BadRequest;
            string Message = "";
            bool Status = false;

            if (data.id == null)
            {
                Code = (int)ApiStatusCode.BadRequest;
                Message = "Invalid data";
            }
            else
            {
                Code = (int)ApiStatusCode.Success;
                Status = true;
                Message = "User role is deleted successfully.";
            }

            return new ApiResponse<UserRoleDeleteResponseViewModel>
            {
                StatusCode = Code,
                Success = Status,
                Message = Message,
                Data = data
            };
        }
        public async Task<ApiResponse<UserRoleResponseViewModel>> UserRoleByIdAsync(Guid? id)
        {
            string Message = "";
            bool Status = false;
            int Code = (int)ApiStatusCode.BadRequest;
            //🔹 Validate Input
            if (id == null)
            {
                return new ApiResponse<UserRoleResponseViewModel>
                {
                    StatusCode = (int)ApiStatusCode.BadRequest,
                    Success = false,
                    Message = "id cannot be empty."
                };
            }

            //🔹 Repository Call
            var data = await _userRoleRepository.UserRoleByIdAsync(id);

            if (data == null)
            {
                Code = (int)ApiStatusCode.BadRequest;
                Message = "Invalid data.";
            }
            else
            {
                Code = (int)ApiStatusCode.Success;
                Status = true;
                Message = "Success.";
            }

            return new ApiResponse<UserRoleResponseViewModel>
            {
                StatusCode = Code,
                Success = Status,
                Message = Message,
                Data = data
            };
        }

        #endregion
        #region User Access Role
        public async Task<ApiResponse<UserAccessRoleCreateUpdateResponseViewModel>> UserAccessRoleCreateAsync(UserAccessRoleRequestViewModel model)
        {
            //🔹 Repository Call

            var data = await _userRoleRepository.UserAccessRoleCreateAsync(model);

            string Message = "";
            bool Status = false;
            int Code = (int)ApiStatusCode.BadRequest;

            if (data.id == null)
            {
                Code = (int)ApiStatusCode.BadRequest;
                Message = "Invalid data";
            }
            else
            {
                Code = (int)ApiStatusCode.Success;
                Status = true;
                if (model.id == null)
                    Message = "User access role is inserted successfully.";
                else
                    Message = "User access role is updated successfully.";
            }

            return new ApiResponse<UserAccessRoleCreateUpdateResponseViewModel>
            {
                StatusCode = Code,
                Success = Status,
                Message = Message,
                Data = data
            };
        }
        public async Task<ApiResponse<UserAccessRoleListResponseViewModel>> UserAccessRoleListAsync(string? search, bool? Is_Active, int length, int page, string orderColumn, string orderDirection)
        {
            string Message = "";
            bool Status = false;
            int Code = (int)ApiStatusCode.BadRequest;
            //🔹 Repository Call
            var data = await _userRoleRepository.UserAccessRoleListAsync(search, Is_Active, length, page, orderColumn, orderDirection);

            if (data == null)
            {
                Code = (int)ApiStatusCode.BadRequest;
                Message = "Invalid data.";
            }
            else
            {
                Code = (int)ApiStatusCode.Success;
                Status = true;
                Message = "Success.";
            }

            return new ApiResponse<UserAccessRoleListResponseViewModel>
            {
                StatusCode = Code,
                Success = Status,
                Message = Message,
                Data = data
            };
        }
        public async Task<ApiResponse<UserAccessRoleDeleteResponseViewModel>> UserAccessRoleDeleteAsync(UserAccessRoleDeleteRequestViewModel model)
        {
            //🔹 Repository Call

            var data = await _userRoleRepository.UserAccessRoleDeleteAsync(model);

            string Message = "";
            bool Status = false;
            int Code = (int)ApiStatusCode.BadRequest;

            if (data.id == null)
            {
                Code = (int)ApiStatusCode.BadRequest;
                Message = "Invalid data";
            }
            else
            {
                Code = (int)ApiStatusCode.Success;
                Status = true;
                Message = "User access role is deleted successfully.";
            }

            return new ApiResponse<UserAccessRoleDeleteResponseViewModel>
            {
                StatusCode = Code,
                Success = Status,
                Message = Message,
                Data = data
            };
        }
        public async Task<ApiResponse<UserAccessRoleResponseViewModel>> UserAccessRoleByIdAsync(Guid? id)
        {
            string Message = "";
            bool Status = false;
            int Code = (int)ApiStatusCode.BadRequest;

            //🔹 Validate Input
            if (id == null)
            {
                return new ApiResponse<UserAccessRoleResponseViewModel>
                {
                    StatusCode = (int)ApiStatusCode.BadRequest,
                    Success = false,
                    Message = "id cannot be empty."
                };
            }

            //🔹 Repository Call
            var data = await _userRoleRepository.UserAccessRoleByIdAsync(id);

            if (data == null)
            {
                Code = (int)ApiStatusCode.BadRequest;
                Message = "Invalid data.";
            }
            else
            {
                Code = (int)ApiStatusCode.Success;
                Status = true;
                Message = "Success.";
            }

            return new ApiResponse<UserAccessRoleResponseViewModel>
            {
                StatusCode = Code,
                Success = Status,
                Message = Message,
                Data = data
            };
        }
        #endregion
    }
}