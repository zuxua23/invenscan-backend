using InvenScan.DTO.Request;
using InvenScan.DTO.Response;

namespace InvenScan.Service.Interfaces;

public interface IUserService
{
    Task<ApiResponse<List<UserResponse>>> GetAllUsersAsync();
    Task<ApiResponse<UserResponse>> GetByUserIdAsync(string userId);
    Task<ApiResponse<UserResponse>> CreateUserAsync(UserCreateRequest request);
    Task<ApiResponse<UserResponse>> UpdateUserAsync(string userId, UserUpdateRequest request);
}
