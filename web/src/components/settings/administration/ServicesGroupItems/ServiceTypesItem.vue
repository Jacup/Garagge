<script lang="ts" setup>
import { getServiceTypes } from '@/api/generated/service-types/service-types'
import { onMounted, ref } from 'vue'

import type { ServiceTypeDto } from '@/api/generated'
import { parseApiError } from '@/utils/error-handler'
import { useNotificationsStore } from '@/stores/notifications'

import DeleteDialog from '@/components/common/DeleteDialog.vue'
import ServiceTypeDialog from '@/components/settings/administration/ServicesGroupItems/ServiceTypeDialog.vue'

const {
  getApiVehiclesServiceRecordsTypes,
  postApiVehiclesServiceRecordsTypes,
  putApiVehiclesServiceRecordsTypesId,
  deleteApiVehiclesServiceRecordsTypesId,
} = getServiceTypes()

const notifications = useNotificationsStore()

const serviceTypes = ref<ServiceTypeDto[]>([])
const isLoading = ref(false)

const isDeleteDialogOpen = ref(false)
const serviceTypeToDelete = ref<ServiceTypeDto | null>(null)

function openDeleteServiceTypeDialog(serviceType: ServiceTypeDto) {
  serviceTypeToDelete.value = serviceType
  isDeleteDialogOpen.value = true
}
function closeDeleteServiceTypeDialog() {
  serviceTypeToDelete.value = null
  isDeleteDialogOpen.value = false
}
async function confirmDeleteServiceType() {
  if (!serviceTypeToDelete.value?.id) {
    return
  }

  try {
    await deleteApiVehiclesServiceRecordsTypesId(serviceTypeToDelete.value.id)
    notifications.show('Service type deleted successfully')
    await loadServiceTypes()
  } catch (error: unknown) {
    const parsedError = parseApiError(error)

    notifications.show(parsedError.message)
  } finally {
    closeDeleteServiceTypeDialog()
  }
}

const isEditDialogOpen = ref(false)
const serviceTypeToEdit = ref<ServiceTypeDto | null>(null)
const serviceTypeApiError = ref<string | null>(null)

function openNewServiceTypeDialog() {
  serviceTypeToEdit.value = null
  serviceTypeApiError.value = null
  isEditDialogOpen.value = true
}

function openEditServiceTypeDialog(serviceType: ServiceTypeDto) {
  serviceTypeToEdit.value = serviceType
  serviceTypeApiError.value = null
  isEditDialogOpen.value = true
}

function clearServiceTypeApiError() {
  serviceTypeApiError.value = null
}

async function handleSaveServiceType(name: string) {
  try {
    isLoading.value = true
    serviceTypeApiError.value = null

    if (serviceTypeToEdit.value) {
      await putApiVehiclesServiceRecordsTypesId(serviceTypeToEdit.value.id, { name })
      notifications.show('Service type updated successfully')
    } else {
      await postApiVehiclesServiceRecordsTypes({ name })
      notifications.show('Service type created successfully')
    }

    await loadServiceTypes()
    isEditDialogOpen.value = false
  } catch (error: unknown) {
    const parsedError = parseApiError(error)
    serviceTypeApiError.value = parsedError.message
  } finally {
    isLoading.value = false
  }
}

async function loadServiceTypes() {
  try {
    isLoading.value = true
    const response = await getApiVehiclesServiceRecordsTypes()
    serviceTypes.value = response
  } catch (error: unknown) {
    const parsedError = parseApiError(error)

    notifications.show(parsedError.message)
  } finally {
    isLoading.value = false
  }
}
onMounted(async () => {
  await loadServiceTypes()
})
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

    <v-list-item>
      <div class="d-flex justify-center">
        <v-list rounded class="material-list-inner" width="400px">
          <v-list-item v-for="serviceType in serviceTypes" :key="serviceType.id" :title="serviceType.name" lines="one">
            <template #append>
              <v-btn icon="mdi-pencil" variant="text" size="small" @click="openEditServiceTypeDialog(serviceType)"></v-btn>
              <v-btn icon="mdi-delete" variant="text" size="small" color="error" @click="openDeleteServiceTypeDialog(serviceType)"></v-btn>
            </template>
          </v-list-item>
          <v-list-item link class="add-new-type-item" color="primary" @click="openNewServiceTypeDialog">
            <div class="d-flex justify-center w-100">
              <v-icon icon="mdi-plus" color="primary"></v-icon>
            </div>
          </v-list-item>
        </v-list>
      </div>
    </v-list-item>
  </v-list-group>

  <DeleteDialog
    :is-open="isDeleteDialogOpen"
    :item-to-delete="serviceTypeToDelete?.name || 'service type'"
    :on-cancel="closeDeleteServiceTypeDialog"
    :on-confirm="confirmDeleteServiceType"
  />
  <ServiceTypeDialog
    v-model="isEditDialogOpen"
    :item="serviceTypeToEdit"
    :is-loading="isLoading"
    :api-error="serviceTypeApiError"
    @save="handleSaveServiceType"
    @clear-error="clearServiceTypeApiError"
  />
</template>

<style lang="scss" scoped></style>
