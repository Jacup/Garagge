namespace Application.Vehicles;

public class VehicleDto
{
    public Guid Id { get; init; }
    
    public DateTime CreatedDate { get; init; }
    public DateTime UpdatedDate { get; init; }
    
    public required string Brand { get; init; }
    public required string Model { get; init; }
    public int? ManufacturedYear { get; init; }
    
    public required Guid UserId { get; init; }
}