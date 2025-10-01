<script lang="ts" setup>
import { ref, onMounted, onUnmounted, watch } from 'vue'
import { useRouter } from 'vue-router'
import type { VehicleDto } from '@/api/generated/apiV1.schemas'
import { getVehicles } from '@/api/generated/vehicles/vehicles'
import { useLayoutFab } from '@/composables/useLayoutFab'
import ActionItems from '@/components/vehicles/topbar/ActionItems.vue'
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
  { value: 'cards' as const, icon: 'mdi-view-grid', tooltip: 'Card View' },
  { value: 'list' as const, icon: 'mdi-view-agenda', tooltip: 'List View' },
  { value: 'detailed-list' as const, icon: 'mdi-view-list', tooltip: 'Detailed List View' },
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
function goToAddVehicle() {
  router.push('/vehicles/add')
}

onMounted(() => {
  registerFab({
    icon: 'mdi-plus',
    text: 'Add',
    action: goToAddVehicle
  })
})

// Cleanup FAB when leaving route
onUnmounted(() => {
  unregisterFab()
})
</script>

<template>
  <div class="page-content">
    <div class="vehicles-topbar">
      <SearchTable v-model="search" />

      <ConnectedButtonGroup v-model="viewMode" :options="viewModeOptions" mandatory />

      <ActionItems />
    </div>

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
        :page="page"
        :items-per-page="itemsPerPage"
        :total-items="totalItems"
        :sort-by="sortBy"
        @edit="edit"
        @delete="remove"
        @view="viewOverview"
        @update:options="onTableOptionsChange"
        @update:sort-by="sortBy = $event"
      />

      <VehicleCardsView v-else :items="serverItems" :loading="loading" @edit="edit" @delete="remove" @view="viewOverview" />
    </div>
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
  background-color: rgba(var(--v-theme-primary), 0.08);
  border-radius: 16px;
  margin-bottom: 0;
  border-bottom: 1px solid var(--color-border);
}

.vehicles-content {
  padding: 16px 0px;
}
</style>
