using InvenScan.DTO.Response;

namespace InvenScan.Service.Interfaces;

public interface ILocationService
{
    Task<ApiResponse<List<LocationResponse>>> GetAllLocationsAsync();
}
