import { ref, computed, watch, type Ref } from 'vue'
import { getVehicles } from '@/api/generated/vehicles/vehicles'
import type { VehicleDto, VehicleCreateRequest, VehicleUpdateRequest } from '@/api/generated/apiV1.schemas'

const DEFAULT_PAGE_SIZE = 10
const SEARCH_DEBOUNCE_MS = 500

interface UseVehiclesOptions {
  initialPageSize?: number
  isMobile?: Ref<boolean>
  onError?: (error: Error, operation: string) => void
}

interface LoadVehiclesOptions {
  isBackgroundReload?: boolean
  resetList?: boolean
}

export function useVehicles(options: UseVehiclesOptions = {}) {
  const { initialPageSize = DEFAULT_PAGE_SIZE, isMobile = ref(false), onError } = options

  const api = getVehicles()
  const { getApiVehicles, getApiVehiclesId, postApiVehicles, putApiVehiclesId, deleteApiVehiclesId } = api

  const vehiclesList = ref<VehicleDto[]>([])
  const vehiclesLoading = ref(false)
  const vehiclesPage = ref(1)
  const vehiclesPageSize = ref(initialPageSize)
  const vehiclesTotal = ref(0)
  const error = ref<string | null>(null)
  const hasMoreRecords = ref(true)

  const search = ref('')
  const sortBy = ref<{ key: string; order: 'asc' | 'desc' }[]>([])

  const pageCount = computed(() => Math.ceil(vehiclesTotal.value / vehiclesPageSize.value))

  let debounceTimeout: ReturnType<typeof setTimeout> | null = null

  let abortController: AbortController | null = null

  async function loadVehicles(options: LoadVehiclesOptions = {}) {
    const { isBackgroundReload = false, resetList = false } = options

    if (abortController) {
      abortController.abort()
    }
    abortController = new AbortController()

    if (!isBackgroundReload) {
      vehiclesLoading.value = true
    }
    error.value = null

    if (resetList) {
      vehiclesPage.value = 1
      if (isMobile.value) {
        vehiclesList.value = []
      }
    }

    try {
      const response = await getApiVehicles({
        searchTerm: search.value || undefined,
        pageSize: vehiclesPageSize.value,
        page: vehiclesPage.value,
        // sortBy: sortBy.value.map(s => `${s.key}:${s.order}`).join(',')
      })

      const fetchedItems = response.items ?? []
      const totalCount = response.totalCount ?? 0

      if (isMobile.value && vehiclesPage.value > 1 && !resetList) {
        const existingIds = new Set(vehiclesList.value.map((v) => v.id))
        const uniqueNewItems = fetchedItems.filter((v) => !existingIds.has(v.id))
        vehiclesList.value = [...vehiclesList.value, ...uniqueNewItems]
      } else {
        vehiclesList.value = fetchedItems
      }

      vehiclesTotal.value = totalCount
      hasMoreRecords.value = vehiclesList.value.length < vehiclesTotal.value

      abortController = null
    } catch (err: unknown) {
      if (err instanceof Error && err.name === 'AbortError') {
        return
      }

      console.error('Failed to load vehicles:', err)
      const errorMessage = 'Failed to load vehicles. Please try again.'
      error.value = errorMessage

      if (onError) {
        onError(err as Error, 'loadVehicles')
      }

      if (!isBackgroundReload) {
        vehiclesList.value = []
        vehiclesTotal.value = 0
      }
    } finally {
      vehiclesLoading.value = false
    }
  }

  async function loadMore(options: { side: 'start' | 'end' | 'both'; done: (status: 'ok' | 'empty' | 'error') => void }) {
    const { done } = options

    if (!hasMoreRecords.value) {
      done('empty')
      return
    }

    vehiclesPage.value++

    try {
      await loadVehicles()
      done(hasMoreRecords.value ? 'ok' : 'empty')
    } catch {
      done('error')
    }
  }

  async function fetchVehicleById(id: string): Promise<VehicleDto | null> {
    try {
      const vehicle = await getApiVehiclesId(id)
      return vehicle
    } catch (err) {
      console.error('Failed to fetch vehicle:', err)
      error.value = 'Failed to fetch vehicle details.'

      if (onError) {
        onError(err as Error, 'fetchVehicleById')
      }

      return null
    }
  }

  async function createVehicle(vehicleData: VehicleCreateRequest): Promise<boolean> {
    try {
      await postApiVehicles(vehicleData)

      await loadVehicles({ resetList: true })

      return true
    } catch (err) {
      console.error('Failed to create vehicle:', err)
      error.value = 'Failed to create vehicle.'

      if (onError) {
        onError(err as Error, 'createVehicle')
      }

      return false
    }
  }

  async function updateVehicle(id: string, vehicleData: VehicleUpdateRequest): Promise<boolean> {
    try {
      await putApiVehiclesId(id, vehicleData)

      await loadVehicles({ isBackgroundReload: true })

      return true
    } catch (err) {
      console.error('Failed to update vehicle:', err)
      error.value = 'Failed to update vehicle.'

      if (onError) {
        onError(err as Error, 'updateVehicle')
      }

      return false
    }
  }

  async function deleteVehicle(id: string): Promise<boolean> {
    const vehicleIndex = vehiclesList.value.findIndex((v) => v.id === id)
    const deletedVehicle = vehicleIndex >= 0 ? vehiclesList.value[vehicleIndex] : null

    if (vehicleIndex >= 0) {
      vehiclesList.value = vehiclesList.value.filter((v) => v.id !== id)
      vehiclesTotal.value = Math.max(0, vehiclesTotal.value - 1)
    }

    try {
      await deleteApiVehiclesId(id)

      await loadVehicles({ isBackgroundReload: true })

      return true
    } catch (err) {
      console.error('Failed to delete vehicle:', err)
      error.value = 'Failed to delete vehicle.'

      if (deletedVehicle && vehicleIndex >= 0) {
        vehiclesList.value.splice(vehicleIndex, 0, deletedVehicle)
        vehiclesTotal.value++
      }

      if (onError) {
        onError(err as Error, 'deleteVehicle')
      }

      return false
    }
  }

  async function deleteMultipleVehicles(ids: string[]): Promise<boolean> {
    if (ids.length === 0) return true

    const deletedVehicles = vehiclesList.value.filter((v) => ids.includes(v.id!))
    const deletedIndices = new Map(deletedVehicles.map((v) => [v.id!, vehiclesList.value.indexOf(v)]))

    vehiclesList.value = vehiclesList.value.filter((v) => !ids.includes(v.id!))
    vehiclesTotal.value = Math.max(0, vehiclesTotal.value - ids.length)

    try {
      await Promise.all(ids.map((id) => deleteApiVehiclesId(id)))

      await loadVehicles({ isBackgroundReload: true })

      return true
    } catch (err) {
      console.error('Failed to delete vehicles:', err)
      error.value = 'Failed to delete some vehicles.'

      deletedVehicles.forEach((vehicle) => {
        const originalIndex = deletedIndices.get(vehicle.id!)
        if (originalIndex !== undefined) {
          vehiclesList.value.splice(originalIndex, 0, vehicle)
        }
      })
      vehiclesTotal.value += deletedVehicles.length

      if (onError) {
        onError(err as Error, 'deleteMultipleVehicles')
      }

      return false
    }
  }

  function updatePage(newPage: number) {
    vehiclesPage.value = newPage
    loadVehicles()
  }

  function updateItemsPerPage(newItemsPerPage: number) {
    vehiclesPageSize.value = newItemsPerPage
    vehiclesPage.value = 1
    loadVehicles()
  }

  function updateSortBy(newSortBy: { key: string; order: 'asc' | 'desc' }[]) {
    sortBy.value = newSortBy
    vehiclesPage.value = 1
    loadVehicles()
  }

  function clearFilters() {
    search.value = ''
    sortBy.value = []
    vehiclesPage.value = 1
    loadVehicles()
  }

  function refresh() {
    loadVehicles({ resetList: true })
  }

  watch(search, () => {
    if (debounceTimeout) {
      clearTimeout(debounceTimeout)
    }

    debounceTimeout = setTimeout(() => {
      vehiclesPage.value = 1
      loadVehicles()
    }, SEARCH_DEBOUNCE_MS)
  })

  function cleanup() {
    if (debounceTimeout) {
      clearTimeout(debounceTimeout)
      debounceTimeout = null
    }
    if (abortController) {
      abortController.abort()
      abortController = null
    }
  }

  return {
    vehiclesList: computed(() => vehiclesList.value),
    vehiclesLoading: computed(() => vehiclesLoading.value),
    vehiclesTotal: computed(() => vehiclesTotal.value),
    error: computed(() => error.value),
    hasMoreRecords: computed(() => hasMoreRecords.value),

    pageCount,

    search,
    sortBy,

    page: computed({
      get: () => vehiclesPage.value,
      set: (val: number) => updatePage(val),
    }),
    itemsPerPage: computed({
      get: () => vehiclesPageSize.value,
      set: (val: number) => updateItemsPerPage(val),
    }),

    loadVehicles,
    loadMore,
    fetchVehicleById,
    createVehicle,
    updateVehicle,
    deleteVehicle,
    deleteMultipleVehicles,
    updatePage,
    updateItemsPerPage,
    updateSortBy,
    clearFilters,
    refresh,
    cleanup,
  }
}

export type UseVehiclesReturn = ReturnType<typeof useVehicles>
