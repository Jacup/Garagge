export function serviceUtils() {
  const getIconForServiceType = (type: string | undefined): string => {
    switch (type) {
      case 'General':
        return 'mdi-cog'
      case 'OilChange':
        return 'mdi-oil'
      case 'Brakes':
        return 'mdi-car-brake-abs'
      case 'Tires':
        return 'mdi-tire'
      case 'Engine':
        return 'mdi-engine'
      case 'Transmission':
        return 'mdi-car-shift-pattern'
      case 'Suspension':
        return 'mdi-car-esp'
      case 'Electrical':
        return 'mdi-flash'
      case 'Bodywork':
        return 'mdi-hammer-wrench'
      case 'Interior':
        return 'mdi-car-seat'
      case 'Inspection':
        return 'mdi-clipboard-check-outline'
      case 'Emergency':
        return 'mdi-alert-decagram'
      case 'Other':
        return 'mdi-help-circle-outline'
      default:
        return 'mdi-tools'
    }
  }

  return {
    getIconForServiceType,
  }
}
