namespace Application.Vehicles;

public class VehicleDto
{
    public Guid Id { get; set; }
    
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
    
    public required string Brand { get; set; }
    public required string Model { get; set; }
    public required DateOnly ManufacturedYear { get; set; }
    
    public required Guid UserId { get; set; }
}