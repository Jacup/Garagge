<script setup lang="ts">
import { computed, ref, onMounted } from 'vue'

import type {
  ServiceRecordDto,
  ServiceTypeDto,
  ServiceRecordCreateRequest,
  ServiceRecordUpdateRequest,
} from '@/api/generated/apiV1.schemas'
import { getServiceRecords } from '@/api/generated/service-records/service-records'

import ServiceDetails from './ServiceDetails.vue'
import ServiceRecordForm from './ServiceRecordForm.vue'
import ServiceItemFormDialog from './ServiceItemFormDialog.vue'
import ServiceItemDeleteDialog from './ServiceItemDeleteDialog.vue'

import { useResponsiveLayout } from '@/composables/useResponsiveLayout'
import { useServiceDetailsState } from '@/composables/vehicles/useServiceDetailsState'
import { useNotificationsStore } from '@/stores/notifications'

const props = defineProps<{
  modelValue: boolean
  record: ServiceRecordDto | null
  vehicleId: string
}>()

const emit = defineEmits<{
  'update:modelValue': [value: boolean]
  'refresh-data': []
}>()

const { getApiVehiclesServiceRecordsTypes, postApiVehiclesVehicleIdServiceRecords, putApiVehiclesVehicleIdServiceRecordsServiceRecordId } =
  getServiceRecords()
const { isMobile } = useResponsiveLayout()
const { mode, editMetadata, cancelAction, close: closeSheet } = useServiceDetailsState()
const notifications = useNotificationsStore()

const isOpen = computed({
  get: () => props.modelValue,
  set: (value) => emit('update:modelValue', value),
})

const title = computed(() => {
  switch (mode.value) {
    case 'create':
      return 'New Service Record'
    case 'edit-metadata':
      return 'Edit Service Info'
    case 'view':
    default:
      return 'Service Details'
  }
})

const serviceTypes = ref<ServiceTypeDto[]>([])
const isSaving = ref(false)
const saveError = ref<string | null>(null)

onMounted(async () => {
  try {
    const res = await getApiVehiclesServiceRecordsTypes()
    serviceTypes.value = res ?? []
  } catch (e) {
    console.error('Failed to load service types', e)
  }
})

const handleSubmit = async (payload: ServiceRecordCreateRequest) => {
  isSaving.value = true
  saveError.value = null
  try {
    if (mode.value === 'create') {
      await postApiVehiclesVehicleIdServiceRecords(props.vehicleId, payload)
      emit('refresh-data')
      closeSheet()
      notifications.show('Service record created successfully.')
    } else if (mode.value === 'edit-metadata') {
      if (!props.record?.id) {
        throw new Error('Missing record ID for update')
      }

      const updatePayload: ServiceRecordUpdateRequest = {
        title: payload.title,
        serviceDate: payload.serviceDate,
        serviceTypeId: payload.serviceTypeId,
        notes: payload.notes,
        mileage: payload.mileage,
        manualCost: payload.manualCost,
      }

      await putApiVehiclesVehicleIdServiceRecordsServiceRecordId(props.vehicleId, props.record.id, updatePayload)

      emit('refresh-data')
      closeSheet()
      notifications.show('Service record updated successfully.')
    }
  } catch (error) {
    console.error('Error saving record:', error)
    saveError.value = 'Failed to save record. Please try again.'
  } finally {
    isSaving.value = false
  }
}
</script>

<template>
  <v-dialog v-if="isMobile" v-model="isOpen" fullscreen transition="dialog-bottom-transition" scrollable>
    <v-card rounded="0" class="d-flex flex-column h-100">
      <v-toolbar class="toolbar">
        <v-btn v-if="mode === 'view'" icon="mdi-close" @click="closeSheet()" />
        <v-btn v-else icon="mdi-close" @click="cancelAction()" :disabled="isSaving" />

        <v-toolbar-title>{{ title }}</v-toolbar-title>
        <v-spacer />

        <v-btn v-if="mode === 'view'" icon="mdi-pencil-outline" variant="text" @click="editMetadata" />
        <!-- <v-progress-circular v-if="isSaving" indeterminate size="24" color="primary" class="mr-4" /> -->
      </v-toolbar>

      <div class="d-flex flex-column flex-grow-1 overflow-y-auto">
        <v-alert v-if="saveError" type="error" variant="tonal" closable class="ma-4 mb-0" @click:close="saveError = null">{{
          saveError
        }}</v-alert>

        <ServiceDetails v-if="mode === 'view'" :record="record" />
        <ServiceRecordForm
          v-else
          :mode="mode"
          :initial-data="mode === 'edit-metadata' ? record : null"
          :service-types="serviceTypes"
          :loading="isSaving"
          @submit="handleSubmit"
          @cancel="cancelAction"
        />
      </div>
    </v-card>
  </v-dialog>

  <v-navigation-drawer v-else v-model="isOpen" location="right" width="500" floating>
    <div class="d-flex flex-column h-100 pt-4 px-4">
      <v-toolbar density="compact" rounded="pill" class="toolbar">
        <v-toolbar-title>{{ title }}</v-toolbar-title>
        <v-spacer />
        <v-btn v-if="mode === 'view'" icon="mdi-pencil" @click="editMetadata" />
        <v-progress-circular v-if="isSaving" indeterminate size="24" color="primary" class="mr-4" />
        <v-btn v-if="mode === 'view'" icon="mdi-close" @click="closeSheet()" />
        <v-btn v-else icon="mdi-close" @click="cancelAction()" :disabled="isSaving" />
      </v-toolbar>

      <div class="flex-grow-1 overflow-y-auto d-flex flex-column">
        <v-alert v-if="saveError" type="error" variant="tonal" closable class="ma-4 mb-0" @click:close="saveError = null">{{
          saveError
        }}</v-alert>

        <ServiceDetails v-if="mode === 'view'" :record="record" />
        <ServiceRecordForm
          v-else
          :mode="mode"
          :initial-data="mode === 'edit-metadata' ? record : null"
          :service-types="serviceTypes"
          :loading="isSaving"
          @submit="handleSubmit"
          @cancel="cancelAction"
        />
      </div>
    </div>
  </v-navigation-drawer>

  <ServiceItemFormDialog :vehicle-id="vehicleId" @refresh-data="emit('refresh-data')" />
  <ServiceItemDeleteDialog :vehicle-id="vehicleId" @success="emit('refresh-data')" />
</template>

<style scoped>
.toolbar {
  background-color: rgba(var(--v-theme-primary), 0.08) !important;
}
</style>
