import { useUserStore } from '@/stores/userStore'
import type { Vehicle } from '@/types/vehicle'

const BASE_URL = import.meta.env.VITE_API_URL || ''

interface VehicleApiResponse {
  id: string
  createdDate: string
  updatedDate: string
  brand: string
  model: string
  manufacturedYear: string
  userId: string
}

function convertApiResponseToVehicle(apiVehicle: VehicleApiResponse): Vehicle & { id: string } {
  return {
    id: apiVehicle.id,
    brand: apiVehicle.brand,
    model: apiVehicle.model,
    manufacturedYear: new Date(apiVehicle.manufacturedYear).getFullYear(),
  }
}

function convertVehicleToApiRequest(vehicle: Vehicle): Omit<VehicleApiResponse, 'id' | 'createdDate' | 'updatedDate' | 'userId'> {
  return {
    brand: vehicle.brand,
    model: vehicle.model,
    manufacturedYear: `${vehicle.manufacturedYear}-01-01`, // RFC 3339 date format
  }
}

export async function getMyVehicles(): Promise<(Vehicle & { id: string })[]> {
  const userStore = useUserStore()

  if (!userStore.accessToken) {
    throw new Error('No access token available')
  }

  const res = await fetch(`${BASE_URL}/vehicles/my`, {
    headers: {
      Authorization: `Bearer ${userStore.accessToken}`,
      'Content-Type': 'application/json',
    },
  })

  if (!res.ok) {
    const errorText = await res.text()
    throw new Error(`Failed to fetch vehicles (${res.status}): ${errorText}`)
  }

  const result: VehicleApiResponse[] = await res.json()
  return result.map(convertApiResponseToVehicle)
}

export async function addNewVehicle(params: Vehicle): Promise<Vehicle & { id: string }> {
  const userStore = useUserStore()

  if (!userStore.accessToken) {
    throw new Error('No access token available')
  }

  const requestBody = convertVehicleToApiRequest(params)

  const res = await fetch(`${BASE_URL}/vehicles/my`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      Authorization: `Bearer ${userStore.accessToken}`,
    },
    body: JSON.stringify(requestBody),
  })

  if (!res.ok) {
    const errorText = await res.text()
    throw new Error(`Failed to add vehicle (${res.status}): ${errorText}`)
  }

  const result: VehicleApiResponse = await res.json()
  return convertApiResponseToVehicle(result)
}
