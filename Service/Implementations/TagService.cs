using InvenScan.Database;
using InvenScan.DTO.Request;
using InvenScan.DTO.Response;
using InvenScan.Entity;
using InvenScan.Service.Interfaces;
using InvenScan.Utility;
using Microsoft.EntityFrameworkCore;

namespace InvenScan.Service.Implementations;

public class TagService : ITagService
{
    private readonly AppDbContext _context;

    public TagService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<TagResponse>> GetTagByIdentifierAsync(string identifier)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(identifier))
                return ApiResponse<TagResponse>.Fail("Identifier is required.");

            var tag = await _context.Tags
                .Include(t => t.Item)
                .Include(t => t.Location)
                .FirstOrDefaultAsync(t => t.TagId == identifier || t.EpcTag == identifier);

            if (tag == null)
                return ApiResponse<TagResponse>.Fail("Tag not found.");

            return ApiResponse<TagResponse>.Ok(MapToResponse(tag));
        }
        catch (Exception)
        {
            return ApiResponse<TagResponse>.Fail("Failed to retrieve tag.");
        }
    }

    public async Task<ApiResponse<List<TagResponse>>> RegisterTagsAsync(TagRegisterRequest request)
    {
        try
        {
            if (request.Tags == null || request.Tags.Count == 0)
                return ApiResponse<List<TagResponse>>.Fail("No tags provided.");

            var newTags = new List<Tag>();

            foreach (var tagItem in request.Tags)
            {
                if (string.IsNullOrWhiteSpace(tagItem.TagId) || string.IsNullOrWhiteSpace(tagItem.EpcTag))
                    return ApiResponse<List<TagResponse>>.Fail($"TagId and EpcTag are required for all tags.");

                var exists = await _context.Tags.AnyAsync(t => t.TagId == tagItem.TagId || t.EpcTag == tagItem.EpcTag);
                if (exists)
                    return ApiResponse<List<TagResponse>>.Fail($"Tag '{tagItem.TagId}' or EPC '{tagItem.EpcTag}' already exists.");

                var itemExists = await _context.Items.AnyAsync(i => i.Id == tagItem.ItemId && !i.IsDelete);
                if (!itemExists)
                    return ApiResponse<List<TagResponse>>.Fail($"Item ID {tagItem.ItemId} not found.");

                var locationExists = await _context.Locations.AnyAsync(l => l.Id == tagItem.LocationId && !l.IsDelete);
                if (!locationExists)
                    return ApiResponse<List<TagResponse>>.Fail($"Location ID {tagItem.LocationId} not found.");

                newTags.Add(new Tag
                {
                    TagId = tagItem.TagId,
                    EpcTag = tagItem.EpcTag,
                    ItemId = tagItem.ItemId,
                    LocationId = tagItem.LocationId,
                    Status = AppConstants.TagStatus.InStock
                });
            }

            await _context.Tags.AddRangeAsync(newTags);
            await _context.SaveChangesAsync();

            var ids = newTags.Select(t => t.Id).ToList();
            var saved = await _context.Tags
                .Include(t => t.Item)
                .Include(t => t.Location)
                .Where(t => ids.Contains(t.Id))
                .ToListAsync();

            return ApiResponse<List<TagResponse>>.Ok(saved.Select(MapToResponse).ToList(), "Tags registered successfully.");
        }
        catch (Exception)
        {
            return ApiResponse<List<TagResponse>>.Fail("Failed to register tags.");
        }
    }

    private static TagResponse MapToResponse(Tag tag) => new()
    {
        Id = tag.Id,
        TagId = tag.TagId,
        EpcTag = tag.EpcTag,
        Status = tag.Status,
        CreatedAt = tag.CreatedAt,
        Item = new ItemResponse
        {
            Id = tag.Item.Id,
            ItemCode = tag.Item.ItemCode,
            ItemName = tag.Item.ItemName,
            Unit = tag.Item.Unit
        },
        Location = new LocationResponse
        {
            Id = tag.Location.Id,
            LocationCode = tag.Location.LocationCode,
            LocationName = tag.Location.LocationName
        }
    };
}
