using InvenScan.DTO.Request;
using InvenScan.DTO.Response;

namespace InvenScan.Service.Interfaces;

public interface ITagService
{
    Task<ApiResponse<TagResponse>> GetTagByIdentifierAsync(string identifier);
    Task<ApiResponse<List<TagResponse>>> RegisterTagsAsync(TagRegisterRequest request);
}
