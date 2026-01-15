import { EnergyType, EnergyUnit } from '@/api/generated/apiV1.schemas'
import {} from '@/api/generated/apiV1.schemas'

export function energyUtils() {
  const getFuelIcon = (type: EnergyType) => {
    switch (type) {
      case EnergyType.Electric:
        return 'mdi-ev-station'
      case EnergyType.Hydrogen:
        return 'mdi-hydrogen-station'
      default:
        return 'mdi-gas-station'
    }
  }

  const getFuelColor = (type: EnergyType) => {
    switch (type) {
      case 'Diesel':
        return 'grey-darken-3'
      case 'LPG':
        return 'blue-darken-1'
      case 'Electric':
        return 'green-darken-1'
      default:
        return 'primary'
    }
  }

  const getDefaultUnitForEnergyType = (type: EnergyType) => {
    switch (type) {
      case EnergyType.Gasoline:
        return EnergyUnit.Liter
      case EnergyType.Diesel:
        return EnergyUnit.Liter
      case EnergyType.LPG:
        return EnergyUnit.Liter
      case EnergyType.Ethanol:
        return EnergyUnit.Liter
      case EnergyType.Biofuel:
        return EnergyUnit.Liter
      case EnergyType.CNG:
        return EnergyUnit.CubicMeter
      case EnergyType.Hydrogen:
        return EnergyUnit.CubicMeter
      case EnergyType.Electric:
        return EnergyUnit.kWh
    }
  }

  return {
    getFuelIcon,
    getFuelColor,
    getDefaultUnitForEnergyType,
  }
}
