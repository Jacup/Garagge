using Domain.Enums;

namespace Domain.Entities.Vehicles;

public sealed record VehicleEnergyTypesUpdateResult(
    IReadOnlyList<EnergyType> TypesToAdd,
    IReadOnlyList<EnergyType> TypesToRemove,
    IReadOnlyList<object> ConflictingEntries)
{
    public static VehicleEnergyTypesUpdateResult NoChanges() => new([], [], []);

    public bool HasChanges => TypesToAdd.Count > 0 || TypesToRemove.Count > 0;
}