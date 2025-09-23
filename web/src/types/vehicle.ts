import type { CreateVehicleCommand, EngineType, NullableOfVehicleType2 } from '@/api/generated/apiV1.schemas'

export interface Vehicle extends CreateVehicleCommand {
  brand: string
  model: string
  engineType: EngineType
  manufacturedYear?: number | null
  type?: NullableOfVehicleType2
  vin?: string | null
}
