import { NullableOfVehicleType } from '@/api/generated/apiV1.schemas'

export const getVehicleIcon = (type?: string | null): string => {
  switch (type) {
    case NullableOfVehicleType.Bus:
      return 'mdi-bus'
    case NullableOfVehicleType.Motorbike:
      return 'mdi-motorbike'
    case NullableOfVehicleType.Truck:
      return 'mdi-truck'
    case NullableOfVehicleType.Car:
    default:
      return 'mdi-car-hatchback'
  }
}
