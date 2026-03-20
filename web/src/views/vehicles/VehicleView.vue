<!-- VehicleView.vue -->
<script setup lang="ts">
import { ref, onMounted, onUnmounted, watch, nextTick, onBeforeUnmount } from 'vue'
import { useRoute } from 'vue-router'
import { getVehicles } from '@/api/generated/vehicles/vehicles'
import type { VehicleDto, VehicleUpdateRequest } from '@/api/generated/apiV1.schemas'

import VehicleOverviewTab from '@/views/vehicles/tabs/VehicleOverviewTab.vue'
import VehicleFuelTab from '@/views/vehicles/tabs/VehicleFuelTab.vue'
import VehicleServiceTab from '@/views/vehicles/tabs/VehicleServiceTab.vue'
import VehicleFormDialog from '@/components/vehicles/VehicleFormDialog.vue'
import { useLayoutFab } from '@/composables/useLayoutFab'
import { useAppBar } from '@/composables/useAppBar'
import { useServiceDetailsState } from '@/composables/vehicles/useServiceDetailsState'
import { useEnergyEntriesState } from '@/composables/vehicles/useEnergyEntriesState'
import { useNotificationsStore } from '@/stores/notifications'

const notifications = useNotificationsStore()
const route = useRoute()

const { getApiVehiclesId, putApiVehiclesId } = getVehicles()
const { registerFab, registerFabMenu, unregisterFab } = useLayoutFab()
const { close: closeServiceDetailsSheet } = useServiceDetailsState()
const { setContextBar, resetToSearch } = useAppBar()

const energyEntriesState = useEnergyEntriesState()
const detailsState = useServiceDetailsState()

const vehicleId = ref(route.params.id as string)
const vehicle = ref<VehicleDto | null>(null)
const loading = ref(true)
const error = ref<string | null>(null)
const activeTab = ref('overview')
const editVehicleDialog = ref(false)

async function loadVehicle() {
  if (!vehicleId.value) {
    error.value = 'No vehicle ID provided'
    loading.value = false
    return
  }

  try {
    loading.value = true
    error.value = null
    const response = await getApiVehiclesId(vehicleId.value)
    vehicle.value = response
    setContextBar(`${response.brand} ${response.model}`, [
      {
        icon: 'mdi-pencil-outline',
        label: 'Edit',
        action: () => { editVehicleDialog.value = true },
      },
    ])
  } catch (err) {
    console.error('Failed to load vehicle:', err)
    error.value = 'Failed to load vehicle data'
  } finally {
    loading.value = false
  }
}

function openEditVehicleDialog() {
  editVehicleDialog.value = true
}

function closeEditVehicleDialog() {
  editVehicleDialog.value = false
}

async function handleVehicleUpdated(vehicleData: VehicleUpdateRequest) {
  if (!vehicleId.value) return

  try {
    await putApiVehiclesId(vehicleId.value, vehicleData)
    notifications.show('Vehicle updated successfully.')
    closeEditVehicleDialog()
    await loadVehicle()
  } catch (err) {
    console.error('Failed to update vehicle:', err)
  }
}

const updateFabForTab = () => {
  if (activeTab.value === 'overview') {
    registerFabMenu({
      icon: 'mdi-plus',
      text: 'Add',
      menuItems: [
        {
          key: 'fuel',
          icon: 'mdi-gas-station',
          text: 'Add Fuel',
          action: () => {
            activeTab.value = 'fuel'
            nextTick(() => energyEntriesState.openCreateDialog())
          },
        },
        {
          key: 'service',
          icon: 'mdi-wrench',
          text: 'Add Service',
          color: 'secondary',
          action: () => {
            activeTab.value = 'service'
            nextTick(() => detailsState.create())
          },
        },
      ],
    })
  } else if (activeTab.value === 'fuel') {
    registerFab({
      icon: 'mdi-gas-station',
      text: 'Add Fuel',
      action: () => energyEntriesState.openCreateDialog(),
    })
  } else if (activeTab.value === 'service') {
    registerFab({
      icon: 'mdi-plus',
      text: 'Add Service',
      action: () => detailsState.create(),
    })
  }
}

onMounted(async () => {
  await loadVehicle()
  updateFabForTab()
})

onBeforeUnmount(() => resetToSearch())
onUnmounted(() => unregisterFab())

watch(activeTab, () => {
  updateFabForTab()
  closeServiceDetailsSheet()
})
</script>

<template>
  <div v-if="loading" class="page-content">
    <div class="d-flex align-center mb-6">
      <v-skeleton-loader type="button" width="40" height="40" class="mr-4" />
      <div class="flex-grow-1">
        <v-skeleton-loader type="heading" width="200" class="mb-2" />
        <v-skeleton-loader type="text" width="300" />
      </div>
    </div>
    <section class="summary-section mb-6">
      <v-row>
        <v-col v-for="n in 4" :key="n" cols="12" sm="6" md="3">
          <v-card class="card-background summary-card" height="120">
            <v-card-text>
              <v-skeleton-loader type="text" width="60%" class="mb-2" />
              <v-skeleton-loader type="heading" width="80%" />
            </v-card-text>
          </v-card>
        </v-col>
      </v-row>
    </section>
  </div>

  <div v-else-if="error" class="d-flex justify-center align-center flex-column" style="height: 400px">
    <v-icon size="64" color="error" class="mb-4">mdi-alert-circle</v-icon>
    <h3 class="text-h5 mb-2 text-error">{{ error }}</h3>
    <v-btn @click="loadVehicle" color="primary" variant="outlined">
      <v-icon start>mdi-refresh</v-icon>
      Retry
    </v-btn>
  </div>

  <div v-else-if="vehicle" class="page-content">
    <v-tabs
      v-model="activeTab"
      align-tabs="center"
      hide-slider
      mandatory
      grow
      selected-class="selected-tab"
      height="64"
      class="mb-4 tabs-container"
    >
      <v-tab value="overview" rounded="pill">
        <v-icon :icon="activeTab === 'overview' ? 'mdi-information' : 'mdi-information-outline'" start size="24" />
        <span class="tab-text">Overview</span>
      </v-tab>
      <v-tab value="fuel" rounded="pill">
        <v-icon :icon="activeTab === 'fuel' ? 'mdi-gas-station' : 'mdi-gas-station-outline'" start size="24" />
        <span class="tab-text">Fuel</span>
      </v-tab>
      <v-tab value="service" rounded="pill">
        <v-icon :icon="activeTab === 'service' ? 'mdi-wrench' : 'mdi-wrench-outline'" start size="24" />
        <span class="tab-text">Service</span>
      </v-tab>
    </v-tabs>

    <v-window v-model="activeTab" :continuous="false" :touch="false">
      <v-window-item value="overview">
        <VehicleOverviewTab
          :vehicle="vehicle"
          :last-entered-mileage="1234"
          @edit="openEditVehicleDialog"
        />
      </v-window-item>

      <v-window-item value="fuel">
        <VehicleFuelTab
          :vehicle-id="vehicle.id!"
          :allowed-energy-types="vehicle.allowedEnergyTypes"
          @entry-changed="loadVehicle"
        />
      </v-window-item>

      <v-window-item value="service">
        <VehicleServiceTab :vehicle-id="vehicle.id!" />
      </v-window-item>
    </v-window>
  </div>

  <div v-else class="d-flex justify-center align-center flex-column" style="height: 400px">
    <v-icon size="64" color="warning" class="mb-4">mdi-car-off</v-icon>
    <h3 class="text-h5 mb-2">No vehicle found</h3>
    <p class="text-body-1 text-medium-emphasis">The requested vehicle could not be found.</p>
  </div>

  <VehicleFormDialog
    :is-open="editVehicleDialog"
    :vehicle="vehicle"
    @update:is-open="editVehicleDialog = $event"
    @save="handleVehicleUpdated"
  />
</template>

<style scoped>
.tabs-container {
  background-color: rgba(var(--v-theme-primary), 0.08) !important;
  border-radius: 9999px;
}

.selected-tab {
  background-color: rgb(var(--v-theme-secondary-container));
}

.card-background {
  background-color: rgba(var(--v-theme-primary), 0.08) !important;
}

.summary-card {
  border-radius: 16px;
}

.summary-section {
  scroll-margin-top: 80px;
  margin-bottom: 24px;
}

.tab-text {
  font-family: 'Roboto', sans-serif;
  font-size: 16px;
  font-weight: 500;
  line-height: 24px;
  height: 24px;
  letter-spacing: normal;
}

@media (max-width: 599px) {
  .summary-card { margin-bottom: 8px; }
}

@media (min-width: 600px) and (max-width: 959px) {
  .summary-card { margin-bottom: 12px; }
}
</style>