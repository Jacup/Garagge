import type { CreateMyVehicleCommand, EngineType, NullableOfVehicleType2 } from '@/api/generated/apiV1.schemas'

export interface Vehicle extends CreateMyVehicleCommand {
  brand: string
  model: string
  engineType: EngineType
  manufacturedYear?: number | null
  type?: NullableOfVehicleType2
  vin?: string | null
}
