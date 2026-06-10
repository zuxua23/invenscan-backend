namespace InvenScan.DTO.Request;

public class UserUpdateRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = "OPERATOR";
    public bool IsActive { get; set; } = true;
    public string? NewPassword { get; set; }
}
