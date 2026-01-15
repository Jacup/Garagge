import { EnergyUnit } from '@/api/generated/apiV1.schemas'

export function formattingUtils() {
  const formatEnergyUnit = (unit: EnergyUnit): string => {
    switch (unit) {
      case EnergyUnit.Liter:
        return 'L'
      case EnergyUnit.Gallon:
        return 'gal'
      case EnergyUnit.CubicMeter:
        return 'm³'
      case EnergyUnit.kWh:
        return 'kWh'
      default:
        return unit
    }
  }

  return {
    formatEnergyUnit,
  }
}
