import { useUserStore } from '@/stores/userStore'

const BASE_URL = import.meta.env.VITE_API_URL || ''
const VEHICLES_URL = `${BASE_URL}/vehicles/my`

export async function getMyVehicles() {
  const userStore = useUserStore()
  const res = await fetch(`${VEHICLES_URL}`, {
    headers: { Authorization: `Bearer ${userStore.accessToken}` },
  })

  if (!res.ok) throw new Error('Fetch vehicles failed')

  return res.json()
}
