<script lang="ts" setup>
import { ref, onMounted, onUnmounted, watch, computed } from 'vue'
import { useRouter } from 'vue-router'

import { useResponsiveLayout } from '@/composables/useResponsiveLayout'
import { useLayoutFab } from '@/composables/useLayoutFab'

import { getVehicles } from '@/api/generated/vehicles/vehicles'
import type { VehicleDto, VehicleCreateRequest, VehicleUpdateRequest } from '@/api/generated/apiV1.schemas'

import ConnectedButtonGroup from '@/components/common/ConnectedButtonGroup.vue'
import VehiclesList from '@/components/vehicles/VehiclesList.vue'
import VehiclesTable from '@/components/vehicles/VehiclesTable.vue'
import VehiclesCards from '@/components/vehicles/VehiclesCard.vue'
import VehicleFormDialog from '@/components/vehicles/VehicleFormDialog.vue'
import DeleteDialog from '@/components/common/DeleteDialog.vue'

const router = useRouter()
const { isMobile } = useResponsiveLayout()
const { registerFab, unregisterFab } = useLayoutFab()
const { getApiVehicles, getApiVehiclesId, postApiVehicles, putApiVehiclesId, deleteApiVehiclesId } = getVehicles()

const vehiclesList = ref<VehicleDto[]>([])
const vehiclesLoading = ref(false)
const vehiclesPage = ref(1)
const vehiclesPageSize = ref(10)
const vehiclesTotal = ref(0)
const error = ref<string | null>(null)
const hasMoreRecords = ref(true)

const search = ref('')
const sortBy = ref<{ key: string; order: 'asc' | 'desc' }[]>([])
const viewMode = ref<'list' | 'detailed-list' | 'cards'>('detailed-list')

const selectedVehicleIds = ref<string[]>([])

const showVehicleDialog = ref(false)
const editingVehicle = ref<VehicleDto | null>(null)
const savingVehicle = ref(false)
const formDialogKey = ref(0)

const showBulkDeleteDialog = ref(false)
const isBulkDeleting = ref(false)

const showSingleDeleteDialog = ref(false)
const vehicleIdToDelete = ref<string | null>(null)

const viewModeOptions = [
  { value: 'list' as const, icon: 'mdi-view-agenda-outline', selectedIcon: 'mdi-view-agenda', tooltip: 'List View' },
  { value: 'detailed-list' as const, icon: 'mdi-view-list-outline', selectedIcon: 'mdi-view-list', tooltip: 'Detailed List View' },
  { value: 'cards' as const, icon: 'mdi-view-grid-outline', selectedIcon: 'mdi-view-grid', tooltip: 'Card View' },
]

const itemsPerPageOptions = [
  { value: 5, title: '5' },
  { value: 10, title: '10' },
  { value: 25, title: '25' },
  { value: 50, title: '50' },
  { value: 100, title: '100' },
]

const pageCount = computed(() => Math.ceil(vehiclesTotal.value / vehiclesPageSize.value))

const vehicleToDeleteName = computed(() => {
  if (!vehicleIdToDelete.value) return 'vehicle'
  const vehicle = vehiclesList.value.find((v) => v.id === vehicleIdToDelete.value)
  return vehicle ? `${vehicle.brand} ${vehicle.model}` : 'vehicle'
})

async function loadVehicles(options: { isBackgroundReload?: boolean; resetList?: boolean } = {}) {
  const { isBackgroundReload = false, resetList = false } = options

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
    })

    const fetchedItems = response.items ?? []
    const totalCount = response.totalCount ?? 0

    if (isMobile.value && vehiclesPage.value > 1) {
      const existingIds = new Set(vehiclesList.value.map((v) => v.id))
      const uniqueNewItems = fetchedItems.filter((v) => !existingIds.has(v.id))
      vehiclesList.value = [...vehiclesList.value, ...uniqueNewItems]
    } else {
      vehiclesList.value = fetchedItems
    }

    vehiclesTotal.value = totalCount
    hasMoreRecords.value = vehiclesList.value.length < vehiclesTotal.value
  } catch (err) {
    console.error('Failed to load vehicles:', err)
    error.value = 'Failed to load vehicles. Please try again.'
    if (!isBackgroundReload) {
      vehiclesList.value = []
      vehiclesTotal.value = 0
    }
  } finally {
    vehiclesLoading.value = false
  }
}

async function loadMore({ done }: { done: (status: 'ok' | 'empty' | 'error') => void }) {
  if (!hasMoreRecords.value) {
    done('empty')
    return
  }
  vehiclesPage.value++
  await loadVehicles()
  done(hasMoreRecords.value ? 'ok' : 'empty')
}

let debounceTimeout: ReturnType<typeof setTimeout> | null = null

  watch(search, () => {
  if (debounceTimeout) clearTimeout(debounceTimeout)
  debounceTimeout = setTimeout(() => {
    vehiclesPage.value = 1
    selectedVehicleIds.value = []
    loadVehicles()
  }, 500)
})

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
  loadVehicles()
}

function remove(id: string | undefined) {
  if (id) {
    vehicleIdToDelete.value = id
    showSingleDeleteDialog.value = true
  }
}

async function confirmSingleDelete() {
  if (vehicleIdToDelete.value) {
    const idToDelete = vehicleIdToDelete.value

    showSingleDeleteDialog.value = false
    vehicleIdToDelete.value = null

    try {
      vehiclesList.value = vehiclesList.value.filter((v) => v.id !== idToDelete)
      selectedVehicleIds.value = selectedVehicleIds.value.filter((id) => id !== idToDelete)

      vehiclesTotal.value = Math.max(0, vehiclesTotal.value - 1)

      await deleteApiVehiclesId(idToDelete)

      await loadVehicles({ isBackgroundReload: true })
    } catch (error) {
      console.error('Delete failed', error)
      loadVehicles()
    }
  }
}

function clearSelection() {
  selectedVehicleIds.value = []
}

async function deleteSelected() {
  isBulkDeleting.value = true
  try {
    const deletePromises = selectedVehicleIds.value.map((id) => deleteApiVehiclesId(id))

    vehiclesList.value = vehiclesList.value.filter((v) => !selectedVehicleIds.value.includes(v.id!))
    vehiclesTotal.value = Math.max(0, vehiclesTotal.value - selectedVehicleIds.value.length)

    selectedVehicleIds.value = []
    showBulkDeleteDialog.value = false

    await Promise.all(deletePromises)
    await loadVehicles({ isBackgroundReload: true })
  } catch (error) {
    console.error('Bulk delete failed', error)
    loadVehicles()
  } finally {
    isBulkDeleting.value = false
  }
}

async function edit(id: string | undefined) {
  if (id) {
    try {
      const res = await getApiVehiclesId(id)
      editingVehicle.value = res
      formDialogKey.value++
      showVehicleDialog.value = true
    } catch (error) {
      console.error('Failed to fetch vehicle for editing:', error)
    }
  }
}

function goToVehicle(id: string | undefined) {
  if (id) router.push(`/vehicles/${id}`)
}

function openAddVehicleDialog() {
  editingVehicle.value = null
  formDialogKey.value++
  showVehicleDialog.value = true
}

async function saveVehicle(vehicleData: VehicleCreateRequest | VehicleUpdateRequest) {
  savingVehicle.value = true
  try {
    if (editingVehicle.value) {
      await putApiVehiclesId(editingVehicle.value.id!, vehicleData as VehicleUpdateRequest)
      await loadVehicles({ isBackgroundReload: true })
    } else {
      await postApiVehicles(vehicleData as VehicleCreateRequest)
      await loadVehicles({ resetList: true })
    }
    showVehicleDialog.value = false
    editingVehicle.value = null
  } catch (error: unknown) {
    console.error(error)
  } finally {
    savingVehicle.value = false
  }
}

onMounted(() => {
  registerFab({ icon: 'mdi-plus', text: 'Add', action: openAddVehicleDialog })
  loadVehicles()
})

onUnmounted(() => {
  unregisterFab()
})
</script>

<template>
  <div class="topbar-container">
    <v-fade-transition mode="out-in" duration="200">
      <div v-if="selectedVehicleIds.length > 0" class="context-bar d-flex align-center w-100 rounded-pill px-2 py-1" key="context-bar">
        <v-tooltip text="Clear Selection" location="bottom" open-delay="200" close-delay="500">
          <template #activator="{ props }">
            <v-btn v-bind="props" icon="mdi-close" variant="text" @click="clearSelection"></v-btn>
          </template>
        </v-tooltip>

        <div class="text-subtitle-1 font-weight-medium ml-2">{{ selectedVehicleIds.length }} vehicle(s) selected</div>

        <v-spacer></v-spacer>
        <v-tooltip text="Delete selected vehicles" location="bottom" open-delay="200" close-delay="500">
          <template #activator="{ props }">
            <v-btn v-bind="props" icon="mdi-delete" variant="text" color="error" @click="showBulkDeleteDialog = true"></v-btn>
          </template>
        </v-tooltip>
      </div>

      <div v-else class="d-flex justify-space-between align-center w-100 py-1" key="standard-bar">
        <ConnectedButtonGroup v-model="viewMode" :options="viewModeOptions" mandatory />
        <v-btn icon="mdi-filter-variant" variant="text" disabled></v-btn>
      </div>
    </v-fade-transition>
  </div>

  <template v-if="isMobile">
    <v-infinite-scroll @load="loadMore" :items="vehiclesList">
      <template v-if="viewMode === 'cards'">
        <VehiclesCards :items="vehiclesList" :loading="vehiclesLoading" @edit="edit" @delete="remove" @view="goToVehicle" />
      </template>

      <template v-else>
        <VehiclesList :items="vehiclesList" :showDetails="viewMode === 'detailed-list'" @select="goToVehicle" @delete="remove" />
      </template>
    </v-infinite-scroll>
  </template>

  <template v-else>
    <div class="vehicles-container">
      <div class="vehicles-content" style="overflow: hidden">
        <VehiclesList v-if="viewMode === 'list'" :items="vehiclesList" :showDetails="true" @select="goToVehicle" @delete="remove" />

        <VehiclesTable
          v-else-if="viewMode === 'detailed-list'"
          v-model="selectedVehicleIds"
          :items="vehiclesList"
          :loading="vehiclesLoading"
          :sort-by="sortBy"
          @edit="edit"
          @delete="remove"
          @view="goToVehicle"
          @update:sort-by="updateSortBy"
        />

        <VehiclesCards v-else :items="vehiclesList" :loading="vehiclesLoading" @edit="edit" @delete="remove" @view="goToVehicle" />
      </div>

      <div class="vehicles-pagination">
        <div class="pagination-left">
          <v-select
            :model-value="vehiclesPageSize"
            :items="itemsPerPageOptions"
            label="Items per page"
            density="compact"
            hide-details
            class="items-per-page-select"
            @update:model-value="updateItemsPerPage"
          />
          <div class="pagination-info">
            {{ Math.min((vehiclesPage - 1) * vehiclesPageSize + 1, vehiclesTotal) }}-{{
              Math.min(vehiclesPage * vehiclesPageSize, vehiclesTotal)
            }}
            of {{ vehiclesTotal }} items
          </div>
        </div>

        <div class="pagination-right">
          <v-pagination
            :model-value="vehiclesPage"
            :length="pageCount"
            :total-visible="7"
            density="comfortable"
            @update:model-value="updatePage"
          />
        </div>
      </div>
    </div>
  </template>

  <VehicleFormDialog
    :key="formDialogKey"
    ref="vehicleFormDialogRef"
    :is-open="showVehicleDialog"
    :vehicle="editingVehicle"
    :loading="savingVehicle"
    @update:is-open="showVehicleDialog = $event"
    @save="saveVehicle"
  />

  <DeleteDialog
    :item-to-delete="`${selectedVehicleIds.length} vehicle(s)`"
    :is-open="showBulkDeleteDialog"
    :on-confirm="deleteSelected"
    :on-cancel="() => (showBulkDeleteDialog = false)"
  />

  <DeleteDialog
    :item-to-delete="vehicleToDeleteName"
    :is-open="showSingleDeleteDialog"
    :on-confirm="confirmSingleDelete"
    :on-cancel="() => (showSingleDeleteDialog = false)"
  />
</template>

<style lang="scss" scoped>
.topbar-container {
  display: flex;
  align-items: center;
  min-height: 64px;
  padding: 0;
}

.context-bar {
  background-color: rgb(var(--v-theme-secondary-container));
  color: rgb(var(--v-theme-on-secondary-container));
}

.vehicles-container {
  display: flex;
  flex-direction: column;
  margin-top: 8px;
}

.vehicles-content {
  flex: 1;
  min-height: 0;
  overflow: auto;
  margin-bottom: 16px;
}

.vehicles-pagination {
  flex-shrink: 0;
  padding: 16px 24px;
  border-top: 1px solid rgb(var(--v-theme-outline-variant));
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 16px;

  .pagination-left {
    display: flex;
    align-items: center;
    gap: 16px;

    .items-per-page-select {
      width: 120px;
    }

    .pagination-info {
      font-size: 0.875rem;
      color: rgb(var(--v-theme-on-surface-variant));
      white-space: nowrap;
    }
  }
}
</style>
