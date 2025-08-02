<script lang="ts" setup>
import { ref, onMounted, watch } from 'vue'
import { getMyVehicles } from '../api/vehiclesApi'
import ActionItems from '@/components/vehicles/topbar/ActionItems.vue'
import SearchTable from '@/components/vehicles/topbar/SearchTable.vue'

// Interfejs dla pojazdu
interface Vehicle {
  brand: string
  model: string
  year: number
}

// Stan dla tabeli
const vehicles = ref<Vehicle[]>([])
const filteredVehicles = ref<Vehicle[]>([])
const loading = ref(false)
const totalItems = ref(0)
const itemsPerPage = ref(10)
const page = ref(1)
const search = ref('')

// Definicja kolumn tabeli
const headers = [
  { title: 'Brand', key: 'brand', sortable: true },
  { title: 'Model', key: 'model', sortable: true },
  { title: 'Year', key: 'year', sortable: true },
]

// Funkcja filtrowania pojazdów
const filterVehicles = () => {
  let items = vehicles.value
  if (search.value) {
    const s = search.value.toLowerCase()
    items = items.filter((v) => v.brand.toLowerCase().includes(s) || v.model.toLowerCase().includes(s) || String(v.year).includes(s))
  }
  totalItems.value = items.length
  // paginacja
  const start = (page.value - 1) * itemsPerPage.value
  const end = start + itemsPerPage.value
  filteredVehicles.value = items.slice(start, end)
}

// Funkcja ładowania danych
const loadVehicles = async () => {
  loading.value = true
  try {
    const data = await getMyVehicles()
    vehicles.value = data
    filterVehicles()
  } catch (error) {
    console.error('Błąd podczas ładowania pojazdów:', error)
    vehicles.value = []
    filterVehicles()
  } finally {
    loading.value = false
  }
}

// Obserwowanie zmian w filtrach i paginacji
watch([search, page, itemsPerPage], filterVehicles)

// Ładowanie danych przy inicjalizacji
onMounted(() => {
  loadVehicles()
})

interface TableOptions {
  page: number
  itemsPerPage: number
}

function updateOptions(opts: TableOptions) {
  page.value = opts.page
  itemsPerPage.value = opts.itemsPerPage
}
</script>

<template>
  <div class="vehicles-view">
    <div class="vehicles-topbar">
      <SearchTable v-model="search" />
      <ActionItems />
    </div>
    <v-data-table-server
      class="vehicles-table"
      :headers="headers"
      :items="filteredVehicles"
      :items-length="totalItems"
      :loading="loading"
      item-value="brand"
      v-model:items-per-page="itemsPerPage"
      @update:options="updateOptions"
    />
  </div>
</template>

<style scoped>
.vehicles-view {
  height: 70vh;
}
.vehicles-table :deep() .v-data-table-header,
.vehicles-table :deep() .v-data-table__th {
  background-color: var(--color-card-contrast); /* Twój kolor */
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
