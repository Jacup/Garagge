<script lang="ts" setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useRouter } from 'vue-router'

import { useResponsiveLayout } from '@/composables/useResponsiveLayout'
import { useLayoutFab } from '@/composables/useLayoutFab'
import { useVehicles } from '@/composables/vehicles/useVehicles'
import { useVehiclesSelection } from '@/composables/vehicles/useVehiclesSelection'
import { useSettingsStore, type VehicleViewType } from '@/stores/settings'
import { useNotificationsStore } from '@/stores/notifications'

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
const settingsStore = useSettingsStore()
const notifications = useNotificationsStore()

const vehicles = useVehicles({
  initialPageSize: 10,
  isMobile,
  onError: (error, operation) => {
    notifications.show(`Failed to ${operation} vehicle(s).`)
  },
})

const {
  vehiclesList,
  vehiclesLoading,
  vehiclesTotal,
  pageCount,
  sortBy,
  page,
  itemsPerPage,
  loadVehicles,
  loadMore,
  fetchVehicleById,
  createVehicle,
  updateVehicle,
  deleteVehicle,
  deleteMultipleVehicles,
  updateSortBy,
  cleanup: cleanupVehicles,
} = vehicles

const selection = useVehiclesSelection({
  enableHistoryIntegration: true,
})

const { selectedIds, hasSelection, selectedCount, clear: clearSelection } = selection

const viewMode = computed({
  get: () => settingsStore.currentVehicleViewMode,
  set: (val: VehicleViewType) => settingsStore.setVehicleViewMode(val),
})

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

const vehicleToDeleteName = computed(() => {
  if (!vehicleIdToDelete.value) return 'vehicle'
  const vehicle = vehiclesList.value.find((v) => v.id === vehicleIdToDelete.value)
  return vehicle ? `${vehicle.brand} ${vehicle.model}` : 'vehicle'
})

function openAddVehicleDialog() {
  editingVehicle.value = null
  formDialogKey.value++
  showVehicleDialog.value = true
}

async function openEditVehicleDialog(id: string | undefined) {
  if (!id) return

  const vehicle = await fetchVehicleById(id)
  if (vehicle) {
    editingVehicle.value = vehicle
    formDialogKey.value++
    showVehicleDialog.value = true
  }
}

async function confirmSave(vehicleData: VehicleCreateRequest | VehicleUpdateRequest) {
  savingVehicle.value = true

  try {
    let success = false

    if (editingVehicle.value) {
      success = await updateVehicle(editingVehicle.value.id!, vehicleData as VehicleUpdateRequest)
    } else {
      success = await createVehicle(vehicleData as VehicleCreateRequest)
    }

    if (success) {
      showVehicleDialog.value = false
      editingVehicle.value = null
      notifications.show('Vehicle saved successfully.')
    }
  } finally {
    savingVehicle.value = false
  }
}

function openSingleDeleteDialog(id: string | undefined) {
  if (id) {
    vehicleIdToDelete.value = id
    showSingleDeleteDialog.value = true
  }
}

async function confirmSingleDelete() {
  if (!vehicleIdToDelete.value) return

  const idToDelete = vehicleIdToDelete.value

  showSingleDeleteDialog.value = false
  vehicleIdToDelete.value = null

  selection.remove(idToDelete)

  const success = await deleteVehicle(idToDelete)
  if (success) {
    notifications.show('Vehicle deleted successfully.')
  }
}

async function confirmMultipleDelete() {
  if (selectedIds.value.length === 0) return

  isBulkDeleting.value = true

  try {
    const idsToDelete = [...selectedIds.value]

    clearSelection()
    showBulkDeleteDialog.value = false

    const success = await deleteMultipleVehicles(idsToDelete)
    if (success) {
      notifications.show('Vehicles deleted successfully.')
    }
  } finally {
    isBulkDeleting.value = false
  }
}

function goToVehicle(id: string | undefined) {
  if (id) router.push(`/vehicles/${id}`)
}

onMounted(() => {
  registerFab({ icon: 'mdi-plus', text: 'Add', action: openAddVehicleDialog })
  loadVehicles()
})

onUnmounted(() => {
  unregisterFab()
  cleanupVehicles()
})
</script>

<template>
  <div class="topbar-container">
    <v-fade-transition mode="out-in" duration="200">
      <div v-if="hasSelection" class="context-bar d-flex align-center w-100 rounded-pill px-2 py-1" key="context-bar">
        <v-tooltip text="Clear Selection" location="bottom" open-delay="200" close-delay="500">
          <template #activator="{ props }">
            <v-btn v-bind="props" icon="mdi-close" variant="text" @click="clearSelection"></v-btn>
          </template>
        </v-tooltip>

        <div class="text-subtitle-1 font-weight-medium ml-2">{{ selectedCount }} vehicle(s) selected</div>

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
        <VehiclesCards
          v-model="selectedIds"
          :items="vehiclesList"
          :loading="vehiclesLoading"
          @edit="openEditVehicleDialog"
          @delete="openSingleDeleteDialog"
          @view="goToVehicle"
        />
      </template>

      <template v-else>
        <VehiclesList
          v-model="selectedIds"
          :items="vehiclesList"
          :showDetails="viewMode === 'detailed-list'"
          @select="goToVehicle"
          @delete="openSingleDeleteDialog"
        />
      </template>
    </v-infinite-scroll>
  </template>

  <template v-else>
    <div class="vehicles-container">
      <div class="vehicles-content">
        <VehiclesList
          v-if="viewMode === 'list'"
          v-model="selectedIds"
          :items="vehiclesList"
          :showDetails="true"
          @select="goToVehicle"
          @delete="openSingleDeleteDialog"
        />

        <VehiclesTable
          v-else-if="viewMode === 'detailed-list'"
          v-model="selectedIds"
          :items="vehiclesList"
          :loading="vehiclesLoading"
          :sort-by="sortBy"
          @edit="openEditVehicleDialog"
          @delete="openSingleDeleteDialog"
          @view="goToVehicle"
          @update:sort-by="updateSortBy"
        />

        <VehiclesCards
          v-else
          v-model="selectedIds"
          :items="vehiclesList"
          :loading="vehiclesLoading"
          @edit="openEditVehicleDialog"
          @delete="openSingleDeleteDialog"
          @view="goToVehicle"
        />
      </div>

      <div class="vehicles-pagination">
        <div class="pagination-left">
          <v-select
            v-model="itemsPerPage"
            :items="itemsPerPageOptions"
            label="Items per page"
            density="compact"
            hide-details
            class="items-per-page-select"
          />
          <div class="pagination-info">
            {{ Math.min((page - 1) * itemsPerPage + 1, vehiclesTotal) }}-{{ Math.min(page * itemsPerPage, vehiclesTotal) }} of
            {{ vehiclesTotal }} items
          </div>
        </div>

        <div class="pagination-right">
          <v-pagination v-model="page" :length="pageCount" :total-visible="7" density="comfortable" />
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
    @save="confirmSave"
  />

  <DeleteDialog
    :item-to-delete="`${selectedCount} vehicle(s)`"
    :is-open="showBulkDeleteDialog"
    :on-confirm="confirmMultipleDelete"
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
