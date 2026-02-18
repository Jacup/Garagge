<script lang="ts" setup>
import { getServiceTypes } from '@/api/generated/service-types/service-types'
import { onMounted, ref } from 'vue'

import type { ServiceTypeDto } from '@/api/generated'
import { parseApiError } from '@/utils/error-handler'
import { useNotificationsStore } from '@/stores/notifications'

const {
  getApiVehiclesServiceRecordsTypes,
  postApiVehiclesServiceRecordsTypes,
  putApiVehiclesServiceRecordsTypesId,
  deleteApiVehiclesServiceRecordsTypesId,
} = getServiceTypes()

const notifications = useNotificationsStore()

const serviceTypes = ref<ServiceTypeDto[]>([])
const isLoading = ref(false)

onMounted(async () => {
  await loadServiceTypes()
})

async function loadServiceTypes() {
  try {
    isLoading.value = true
    const response = await getApiVehiclesServiceRecordsTypes()
    serviceTypes.value = response
  } catch (error: unknown) {
    const parsedError = parseApiError(error)

    notifications.show(parsedError.message)
  }
}
</script>

<template>
  <v-list-group value="service-types">
    <template v-slot:activator="{ props }">
      <v-list-item v-bind="props" lines="two" subtitle="Add, edit or delete types" prepend-icon="mdi-form-textbox-password">
        <template #title>
          <div class="d-flex ga-2">
            Service Types
            <!-- <v-chip v-if="isDirty" size="small" density="compact" color="warning" class="suggestion-chip" variant="outlined">
              Unsaved changes
            </v-chip> -->
          </div>
        </template>
      </v-list-item>
    </template>

    <v-list-item v-for="serviceType in serviceTypes" :key="serviceType.id" :title="serviceType.name" lines="one">
      <template #append>
        <v-btn icon="mdi-pencil" variant="text" size="small"></v-btn>
        <v-btn icon="mdi-delete" variant="text" size="small" color="error"></v-btn>
      </template>
    </v-list-item>
  </v-list-group>
</template>
