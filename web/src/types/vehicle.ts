import type { VehicleCreateRequest, EngineType, NullableOfVehicleType, EnergyType } from '@/api/generated/apiV1.schemas'

export interface Vehicle extends VehicleCreateRequest {
  brand: string
  model: string
  engineType: EngineType
  manufacturedYear: number | null
  type: NullableOfVehicleType
  vin: string | null
  energyTypes: EnergyType[] | null
}
