<script lang="ts" setup>
import { ref, onMounted, onUnmounted, watch, computed } from 'vue'
import { useRouter } from 'vue-router'
import type { VehicleDto } from '@/api/generated/apiV1.schemas'
import { getVehicles } from '@/api/generated/vehicles/vehicles'
import { useLayoutFab } from '@/composables/useLayoutFab'
import SearchTable from '@/components/vehicles/topbar/SearchTable.vue'
import ConnectedButtonGroup from '@/components/common/ConnectedButtonGroup.vue'
import VehicleListView from '@/components/vehicles/views/VehicleListView.vue'
import VehicleDetailedListView from '@/components/vehicles/views/VehicleDetailedListView.vue'
import VehicleCardsView from '@/components/vehicles/views/VehicleCardsView.vue'

const { getApiVehicles, deleteApiVehiclesId } = getVehicles()
const router = useRouter()
const { registerFab, unregisterFab } = useLayoutFab()

const page = ref(1)
const itemsPerPage = ref(10)
const search = ref('')
const sortBy = ref<{ key: string; order: 'asc' | 'desc' }[]>([])
const serverItems = ref([] as VehicleDto[])
const loading = ref(true)
const totalItems = ref(0)
const viewMode = ref<'list' | 'detailed-list' | 'cards'>('cards')

const viewModeOptions = [
  { value: 'cards' as const, icon: 'mdi-view-grid-outline', selectedIcon: 'mdi-view-grid', tooltip: 'Card View' },
  { value: 'list' as const, icon: 'mdi-view-agenda-outline', selectedIcon: 'mdi-view-agenda', tooltip: 'List View' },
  { value: 'detailed-list' as const, icon: 'mdi-view-list-outline', selectedIcon: 'mdi-view-list', tooltip: 'Detailed List View' },
]

const itemsPerPageOptions = [
  { value: 5, title: '5' },
  { value: 10, title: '10' },
  { value: 25, title: '25' },
  { value: 50, title: '50' },
  { value: 100, title: '100' },
]

const pageCount = computed(() => {
  return Math.ceil(totalItems.value / itemsPerPage.value)
})

let debounceTimeout: ReturnType<typeof setTimeout> | null = null

function onSearchChange() {
  if (debounceTimeout) clearTimeout(debounceTimeout)
  debounceTimeout = setTimeout(() => {
    page.value = 1
    loadItems()
  }, 500)
}

watch(search, onSearchChange)

function updatePage(newPage: number) {
  page.value = newPage
  loadItems()
}

function updateItemsPerPage(newItemsPerPage: number) {
  itemsPerPage.value = newItemsPerPage
  page.value = 1 // Reset to first page
  loadItems()
}

function updateSortBy(newSortBy: { key: string; order: 'asc' | 'desc' }[]) {
  sortBy.value = newSortBy
  loadItems()
}

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

function goToAddVehicle() {
  router.push('/vehicles/add')
}

onMounted(() => {
  registerFab({
    icon: 'mdi-plus',
    text: 'Add',
    action: goToAddVehicle,
  })
  loadItems()
})

onUnmounted(() => {
  unregisterFab()
})
</script>

<template>
  <div class="vehicles-topbar">
    <SearchTable v-model="search" />
    <ConnectedButtonGroup v-model="viewMode" :options="viewModeOptions" mandatory />
  </div>

  <div class="vehicles-container">
    <div class="vehicles-content">
      <VehicleListView
        v-if="viewMode === 'list'"
        :items="serverItems"
        :loading="loading"
        @edit="edit"
        @delete="remove"
        @view="viewOverview"
      />

      <VehicleDetailedListView
        v-else-if="viewMode === 'detailed-list'"
        :items="serverItems"
        :loading="loading"
        :sort-by="sortBy"
        @edit="edit"
        @delete="remove"
        @view="viewOverview"
        @update:sort-by="updateSortBy"
      />

      <VehicleCardsView v-else :items="serverItems" :loading="loading" @edit="edit" @delete="remove" @view="viewOverview" />
    </div>

    <div class="vehicles-pagination">
      <div class="pagination-left">
        <v-select
          :model-value="itemsPerPage"
          :items="itemsPerPageOptions"
          label="Items per page"
          density="compact"
          hide-details
          class="items-per-page-select"
          @update:model-value="updateItemsPerPage"
        />

        <div class="pagination-info">
          {{ Math.min((page - 1) * itemsPerPage + 1, totalItems) }}-{{ Math.min(page * itemsPerPage, totalItems) }} of
          {{ totalItems }} items
        </div>
      </div>

      <div class="pagination-right">
        <v-pagination :model-value="page" :length="pageCount" :total-visible="7" density="comfortable" @update:model-value="updatePage" />
      </div>
    </div>
  </div>
</template>

<style lang="scss" scoped>
.vehicles-topbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1rem;
  background-color: rgba(var(--v-theme-primary), 0.08);
  border-radius: 16px;
  margin-bottom: 16px;
}

.vehicles-container {
  display: flex;
  flex-direction: column;
  background-color: rgba(var(--v-theme-primary), 0.08);
  padding: 1rem;
  border-radius: 16px;
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

    .pagination-right {
      flex-shrink: 0;
    }
  }
}

.vehicles-header {
  flex-shrink: 0;
  padding: 16px 24px;
  border-bottom: 1px solid rgb(var(--v-theme-outline-variant));
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 16px;

  .vehicles-title {
    font-size: 1.5rem;
    font-weight: 500;
    color: rgb(var(--v-theme-on-surface));
  }
}

@media (max-width: 768px) {
  .vehicles-header {
    padding: 12px 16px;
    flex-direction: column;
    align-items: stretch;
    gap: 12px;

    .vehicles-title {
      text-align: center;
    }
  }

  .vehicle-pagination {
    padding: 12px 16px;
    flex-direction: column;
    gap: 12px;

    .pagination-left {
      justify-content: center;
      flex-wrap: wrap;
      gap: 12px;

      .pagination-info {
        font-size: 0.8125rem;
      }
    }
  }
}
</style>
