using Application.Abstractions.Messaging;
using Domain.Enums;

namespace Application.Features.VehicleEnergyTypes.GetSupportedForEngine;

public sealed record GetSupportedEnergyTypeForEngineQuery(EngineType EngineType) : IQuery<ICollection<EnergyType>>;