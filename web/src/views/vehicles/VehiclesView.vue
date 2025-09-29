<script lang="ts" setup>
import { ref, onMounted, watch } from 'vue'
import { useRouter } from 'vue-router'
import type { VehicleDto } from '@/api/generated/apiV1.schemas'
import { getVehicles } from '@/api/generated/vehicles/vehicles'
import ActionItems from '@/components/vehicles/topbar/ActionItems.vue'
import SearchTable from '@/components/vehicles/topbar/SearchTable.vue'
import ConnectedButtonGroup from '@/components/common/ConnectedButtonGroup.vue'

const { getApiVehicles, deleteApiVehiclesId } = getVehicles()
const router = useRouter()

const page = ref(1)
const itemsPerPage = ref(10)
const search = ref('')
const sortBy = ref<{ key: string; order: 'asc' | 'desc' }[]>([])
const serverItems = ref([] as VehicleDto[])
const loading = ref(true)
const totalItems = ref(0)
const viewMode = ref<'list' | 'detailed-list' | 'cards'>('cards')

const viewModeOptions = [
  { value: 'cards' as const, icon: 'mdi-view-grid', tooltip: 'Card View' },
  { value: 'list' as const, icon: 'mdi-view-agenda', tooltip: 'List View' },
  { value: 'detailed-list' as const, icon: 'mdi-view-list', tooltip: 'Detailed List View' },
]

const headers = [
  { title: 'Brand', key: 'brand', sortable: true },
  { title: 'Model', key: 'model', sortable: true },
  { title: 'Engine Type', key: 'engineType', sortable: true },
  { title: 'Year', key: 'manufacturedYear', sortable: true },
  { title: 'Type', key: 'type', sortable: true },
  { title: 'VIN', key: 'vin', sortable: true },
  { title: 'Actions', key: 'actions', sortable: false, align: 'end' as const },
]

let debounceTimeout: ReturnType<typeof setTimeout> | null = null

function onSearchChange() {
  if (debounceTimeout) clearTimeout(debounceTimeout)
  debounceTimeout = setTimeout(() => {
    page.value = 1
    loadItems()
  }, 500)
}

watch(search, onSearchChange)

function onTableOptionsChange(options: { page: number; itemsPerPage: number }) {
  page.value = options.page
  itemsPerPage.value = options.itemsPerPage
  loadItems()
}

onMounted(() => {
  loadItems()
})

async function loadItems() {
  loading.value = true
  try {
    const res = await getApiVehicles({
      searchTerm: search.value || undefined,
      pageSize: itemsPerPage.value,
      page: page.value,
    })
    serverItems.value = res.data.items ?? []
    totalItems.value = res.data.totalCount ?? 0
  } catch (error) {
    console.error('Fetching data failed: ', error)
    serverItems.value = []
    totalItems.value = 0
  } finally {
    loading.value = false
  }
}

async function remove(id: string | undefined) {
  const res = await deleteApiVehiclesId(id ?? '')
  if (res.status === 204) {
    loadItems()
  } else {
    console.error('Failed to delete vehicle:', res)
  }
}

function edit(id: string | undefined) {
  if (id) {
    router.push(`/vehicles/edit/${id}`)
  }
}

function viewOverview(id: string | undefined) {
  if (id) {
    router.push(`/vehicles/${id}`)
  }
}
</script>

<template>
  <div class="page-content">
    <div class="vehicles-topbar">
      <SearchTable v-model="search" />

      <ConnectedButtonGroup
        v-model="viewMode"
        :options="viewModeOptions"
        mandatory
      />

      <ActionItems />
    </div>
    <v-data-table-server
      v-model:items-per-page="itemsPerPage"
      :headers="headers"
      :items="serverItems"
      :items-length="totalItems"
      :loading="loading"
      item-value="id"
      :page="page"
      :sort-by="sortBy"
      show-select
      @update:options="onTableOptionsChange"
      @update:sort-by="sortBy = $event"
    >
      <template v-slot:[`item.actions`]="{ item }">
        <div class="d-flex gap-1 justify-end">
          <v-tooltip text="View Details">
            <template #activator="{ props }">
              <v-btn
                v-bind="props"
                @click="viewOverview(item.id)"
                variant="tonal"
                prepend-icon="mdi-eye"
                color="primary"
                text="View"
                size="small"
                class="action-btn-small"
              />
            </template>
          </v-tooltip>
          <v-tooltip text="Edit Vehicle">
            <template #activator="{ props }">
              <v-btn
                v-bind="props"
                @click="edit(item.id)"
                variant="tonal"
                prepend-icon="mdi-pencil"
                color="info"
                text="Edit"
                size="small"
                class="action-btn-small"
              />
            </template>
          </v-tooltip>
          <v-tooltip text="Delete Vehicle">
            <template #activator="{ props }">
              <v-btn
                v-bind="props"
                @click="remove(item.id)"
                variant="tonal"
                prepend-icon="mdi-delete"
                color="error"
                text="Delete"
                size="small"
                class="action-btn-small"
              />
            </template>
          </v-tooltip>
        </div>
      </template>
      <template v-slot:[`item.manufacturedYear`]="{ item }">
        {{ item.manufacturedYear || 'N/A' }}
      </template>
      <template v-slot:[`item.type`]="{ item }">
        <v-chip v-if="item.type" size="small" variant="tonal">{{ item.type }}</v-chip>
        <span v-else class="text-medium-emphasis">N/A</span>
      </template>
      <template v-slot:[`item.vin`]="{ item }">
        {{ item.vin || 'N/A' }}
      </template>
      <template v-slot:[`item.engineType`]="{ item }">
        {{ item.engineType || 'N/A' }}
      </template>
    </v-data-table-server>
  </div>
</template>

<style scoped>
.page-content {
  margin: 0 auto;
}

.vehicles-topbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1rem;
  background-color: var(--color-card, #1e1e1e);
  border-radius: 8px 8px 0px 0px;
  margin-bottom: 0;
  border-bottom: 1px solid var(--color-border);
}

.vehicles-table :deep() .v-data-table-header,
.vehicles-table :deep() .v-data-table__th {
  background-color: var(--color-card-contrast);
}

/* Enhanced action buttons */
.action-btn-small {
  text-transform: none;
  font-weight: 500;
  height: 32px;
  min-width: 70px;
  font-size: 0.75rem;
}

.v-data-table :deep(.v-data-table__td) {
  padding: 8px 16px;
}

.vehicles-table {
  background: var(--color-card, #1e1e1e);
  color: var(--color-text);
  border-radius: 0px 0px 8px 8px;
  height: 100%;
}
</style>
