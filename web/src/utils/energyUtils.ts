import { EnergyType } from '@/api/generated/apiV1.schemas'
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

  return {
    getFuelIcon,
    getFuelColor,
  }
}
