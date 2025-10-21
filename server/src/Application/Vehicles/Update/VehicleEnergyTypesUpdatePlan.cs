using Domain.Enums;

namespace Application.Vehicles.Update;

public sealed record VehicleEnergyTypesUpdatePlan(
    IReadOnlyList<EnergyType> TypesToAdd,
    IReadOnlyList<EnergyType> TypesToRemove,
    IReadOnlyList<EnergyType> ConflictingEnergyTypes)
{
    public bool HasChanges => TypesToAdd.Count > 0 || TypesToRemove.Count > 0;

    public static VehicleEnergyTypesUpdatePlan NoChanges() => new([], [], []);
}