using InvenScan.Database;
using InvenScan.DTO.Request;
using InvenScan.DTO.Response;
using InvenScan.Entity;
using InvenScan.Service.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InvenScan.Service.Implementations;

public class UserService : IUserService
{
    private readonly AppDbContext _context;

    public UserService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<List<UserResponse>>> GetAllUsersAsync()
    {
        try
        {
            var users = await _context.Users
                .OrderBy(u => u.FullName)
                .Select(u => MapToResponse(u))
                .ToListAsync();

            return ApiResponse<List<UserResponse>>.Ok(users);
        }
        catch (Exception)
        {
            return ApiResponse<List<UserResponse>>.Fail("Failed to retrieve users.");
        }
    }

    public async Task<ApiResponse<UserResponse>> GetByUserIdAsync(string userId)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
                return ApiResponse<UserResponse>.Fail("User not found.");

            return ApiResponse<UserResponse>.Ok(MapToResponse(user));
        }
        catch (Exception)
        {
            return ApiResponse<UserResponse>.Fail("Failed to retrieve user.");
        }
    }

    public async Task<ApiResponse<UserResponse>> CreateUserAsync(UserCreateRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.UserId) || string.IsNullOrWhiteSpace(request.Password))
                return ApiResponse<UserResponse>.Fail("UserId and password are required.");

            var exists = await _context.Users.AnyAsync(u => u.UserId == request.UserId);
            if (exists)
                return ApiResponse<UserResponse>.Fail($"User '{request.UserId}' already exists.");

            var user = new User
            {
                UserId = request.UserId,
                FullName = request.FullName,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = request.Role,
                IsActive = true
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return ApiResponse<UserResponse>.Ok(MapToResponse(user), "User created successfully.");
        }
        catch (Exception)
        {
            return ApiResponse<UserResponse>.Fail("Failed to create user.");
        }
    }

    public async Task<ApiResponse<UserResponse>> UpdateUserAsync(string userId, UserUpdateRequest request)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
                return ApiResponse<UserResponse>.Fail("User not found.");

            user.FullName = request.FullName;
            user.Role = request.Role;
            user.IsActive = request.IsActive;

            if (!string.IsNullOrWhiteSpace(request.NewPassword))
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

            await _context.SaveChangesAsync();

            return ApiResponse<UserResponse>.Ok(MapToResponse(user), "User updated successfully.");
        }
        catch (Exception)
        {
            return ApiResponse<UserResponse>.Fail("Failed to update user.");
        }
    }

    private static UserResponse MapToResponse(User u) => new()
    {
        Id = u.Id,
        UserId = u.UserId,
        FullName = u.FullName,
        Role = u.Role,
        IsActive = u.IsActive,
        CreatedAt = u.CreatedAt
    };
}
