<script lang="ts" setup>
import { ref, onMounted, watch } from 'vue'
import { useRouter } from 'vue-router'
import type { VehicleDto } from '@/api/apiV1.schemas'
import { getVehicles } from '@/api/generated/vehicles/vehicles'
import ActionItems from '@/components/vehicles/topbar/ActionItems.vue'
import SearchTable from '@/components/vehicles/topbar/SearchTable.vue'

const { getVehiclesMy, deleteVehiclesMyId } = getVehicles()
const router = useRouter()

const page = ref(1)
const itemsPerPage = ref(10)
const search = ref('')
const sortBy = ref<{ key: string; order: 'asc' | 'desc' }[]>([])
const serverItems = ref([] as VehicleDto[])
const loading = ref(true)
const totalItems = ref(0)

const headers = [
  { title: 'Brand', key: 'brand', sortable: false },
  { title: 'Model', key: 'model', sortable: false },
  { title: 'Power Type', key: 'powerType', sortable: false },
  { title: 'Year', key: 'manufacturedYear', sortable: false },
  { title: 'Type', key: 'type', sortable: false },
  { title: 'VIN', key: 'vin', sortable: false },
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
    const res = await getVehiclesMy({
      page: page.value,
      pageSize: itemsPerPage.value,
      searchTerm: search.value || undefined,
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
  const res = await deleteVehiclesMyId(id ?? '')
  res.status === 204 ? loadItems() : console.error('Failed to delete vehicle:', res)
}

function edit(id: string | undefined) {
  if (id) {
    router.push(`/vehicles/edit/${id}`)
  }
}
</script>

<template>
  <div class="vehicles-view">
    <div class="vehicles-topbar">
      <SearchTable v-model="search" />
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
    >
      <template v-slot:item.actions="{ item }">
        <div class="d-flex ga-2 justify-end">
          <v-icon color="medium-emphasis" icon="mdi-pencil" size="small" @click="edit(item.id)"></v-icon>
          <v-icon color="medium-emphasis" icon="mdi-delete" size="small" @click="remove(item.id)"></v-icon>
        </div>
      </template>
    </v-data-table-server>
  </div>
</template>

<style scoped>
.vehicles-view {
  height: 70vh;
}
.vehicles-table :deep() .v-data-table-header,
.vehicles-table :deep() .v-data-table__th {
  background-color: var(--color-card-contrast);
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

.vehicles-table {
  background: var(--color-card, #1e1e1e);
  color: var(--color-text);
  border-radius: 0px 0px 8px 8px;
  height: 100%;
}
</style>
