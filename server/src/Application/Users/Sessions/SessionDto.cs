namespace Application.Users.Sessions;

public record SessionDto
{
    public Guid Id { get; set; }

    public DateTime CreatedDate { get; set; }
    public bool IsCurrent { get; set; }
    
    public string? DeviceName { get; set; }
    public string? DeviceOs { get; set; }
    public string? DeviceBrowser { get; set; }
    public string? DeviceType { get; set; }
    
    public string? IpAddress { get; set; }
    public string? Location { get; set; }
}