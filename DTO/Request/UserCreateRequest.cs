namespace InvenScan.DTO.Request;

public class UserCreateRequest
{
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = "OPERATOR";
}
