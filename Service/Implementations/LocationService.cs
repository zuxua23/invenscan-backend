using InvenScan.Database;
using InvenScan.DTO.Response;
using InvenScan.Service.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InvenScan.Service.Implementations;

public class LocationService : ILocationService
{
    private readonly AppDbContext _context;

    public LocationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<List<LocationResponse>>> GetAllLocationsAsync()
    {
        try
        {
            var locations = await _context.Locations
                .Where(l => !l.IsDelete)
                .OrderBy(l => l.LocationCode)
                .Select(l => new LocationResponse
                {
                    Id = l.Id,
                    LocationCode = l.LocationCode,
                    LocationName = l.LocationName,
                    Description = l.Description,
                    CreatedAt = l.CreatedAt
                })
                .ToListAsync();

            return ApiResponse<List<LocationResponse>>.Ok(locations);
        }
        catch (Exception)
        {
            return ApiResponse<List<LocationResponse>>.Fail("Failed to retrieve locations.");
        }
    }
}
