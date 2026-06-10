using InvenScan.Database;
using InvenScan.DTO.Request;
using InvenScan.DTO.Response;
using InvenScan.Service.Interfaces;
using InvenScan.Utility;
using Microsoft.EntityFrameworkCore;

namespace InvenScan.Service.Implementations;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly JwtTokenHelper _jwtTokenHelper;
    private readonly IConfiguration _configuration;

    public AuthService(AppDbContext context, JwtTokenHelper jwtTokenHelper, IConfiguration configuration)
    {
        _context = context;
        _jwtTokenHelper = jwtTokenHelper;
        _configuration = configuration;
    }

    public async Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.UserId) || string.IsNullOrWhiteSpace(request.Password))
                return ApiResponse<LoginResponse>.Fail("UserId and password are required.");

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == request.UserId && u.IsActive);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return ApiResponse<LoginResponse>.Fail("Invalid credentials.");

            var expiryHours = int.Parse(_configuration["JwtSettings:ExpiryHours"] ?? "24");
            var token = _jwtTokenHelper.GenerateToken(user.UserId, user.Role);

            return ApiResponse<LoginResponse>.Ok(new LoginResponse
            {
                Token = token,
                UserId = user.UserId,
                FullName = user.FullName,
                Role = user.Role,
                ExpiresAt = DateTime.UtcNow.AddHours(expiryHours)
            });
        }
        catch (Exception)
        {
            return ApiResponse<LoginResponse>.Fail("Login failed. Please try again.");
        }
    }
}
