<script setup lang="ts">
import { ref, reactive, watch, computed } from 'vue'
import { ServiceItemType } from '@/types/serviceItemType'

import type {
  ServiceItemCreateRequest,
  ServiceItemUpdateRequest,
  ServiceItemType as ApiServiceItemType,
} from '@/api/generated/apiV1.schemas'
import { getServiceItems } from '@/api/generated/service-items/service-items'
import { useServiceItemState } from '@/composables/vehicles/useServiceItemState'
import { useNotificationsStore } from '@/stores/notifications'

const props = defineProps<{
  vehicleId: string
}>()

const emit = defineEmits<{
  'refresh-data': []
}>()

const { isOpen, mode, selectedItem, parentRecordId, close: closeDialog } = useServiceItemState()

const {
  postApiVehiclesVehicleIdServiceRecordsServiceRecordIdServiceItems,
  putApiVehiclesVehicleIdServiceRecordsServiceRecordIdServiceItemsServiceItemId,
} = getServiceItems()
const notifications = useNotificationsStore()

const isSaving = ref(false)
const saveError = ref<string | null>(null)
interface ValidateResult {
  valid: boolean
}
type FormRefType = { validate: () => Promise<ValidateResult> } | null
const formRef = ref<FormRefType>(null)

const title = computed(() => (mode.value === 'create' ? 'Add Service Item' : 'Edit Service Item'))
const submitLabel = computed(() => (mode.value === 'create' ? 'Add Item' : 'Save Changes'))

const getDefaultFormState = () => ({
  name: '',
  type: null as ServiceItemType | null,
  partNumber: '',
  quantity: 1 as number | null,
  unitPrice: null as number | null,
  notes: '',
})

const form = reactive(getDefaultFormState())

watch([isOpen, selectedItem], ([newIsOpen, newItem]) => {
  if (newIsOpen) {
    saveError.value = null
    if (newItem && mode.value === 'edit') {
      form.name = newItem.name
      form.type = newItem.type as unknown as ServiceItemType
      form.partNumber = newItem.partNumber || ''
      form.quantity = newItem.quantity
      form.unitPrice = newItem.unitPrice
      form.notes = newItem.notes || ''
    } else {
      Object.assign(form, getDefaultFormState())
    }
  }
})

const rules = {
  required: (value: string | number | null) => !!value || 'This field is required.',
  greaterThanZero: (value: number) => value > 0 || 'Must be > 0',
  greaterOrEqualToZero: (value: number) => value >= 0 || 'Must be >= 0',
}
const serviceItemTypeOptions = Object.keys(ServiceItemType)
  .filter((k) => isNaN(Number(k)))
  .map((k) => ({ label: k, value: ServiceItemType[k as keyof typeof ServiceItemType] }))

const submit = async () => {
  const result = await formRef.value?.validate()
  if (!result?.valid) return

  if (!props.vehicleId || !parentRecordId.value) {
    saveError.value = 'Missing vehicle or record ID'
    return
  }

  isSaving.value = true
  saveError.value = null

  try {
    const payloadBase = {
      name: form.name,
      type: form.type! as unknown as ApiServiceItemType,
      partNumber: form.partNumber || null,
      quantity: form.quantity!,
      unitPrice: form.unitPrice!,
      notes: form.notes || null,
    }

    if (mode.value === 'create') {
      const createPayload: ServiceItemCreateRequest = { ...payloadBase }
      await postApiVehiclesVehicleIdServiceRecordsServiceRecordIdServiceItems(props.vehicleId, parentRecordId.value, createPayload)
      notifications.show('Service item created successfully.')
    } else if (mode.value === 'edit' && selectedItem.value?.id) {
      const updatePayload: ServiceItemUpdateRequest = { ...payloadBase }
      await putApiVehiclesVehicleIdServiceRecordsServiceRecordIdServiceItemsServiceItemId(
        props.vehicleId,
        parentRecordId.value,
        selectedItem.value.id,
        updatePayload,
      )
      notifications.show('Service item updated successfully.')
    }

    emit('refresh-data')
    closeDialog()
  } catch (error) {
    console.error('Error saving item:', error)
    saveError.value = 'Failed to save item. Please try again.'
  } finally {
    isSaving.value = false
  }
}
</script>

<template>
  <v-dialog v-model="isOpen" max-width="500px" persistent scrollable>
    <v-card>
      <v-toolbar density="compact" color="surface" class="border-b pr-2">
        <v-toolbar-title class="text-subtitle-1 font-weight-medium">{{ title }}</v-toolbar-title>
        <v-spacer></v-spacer>
        <v-btn icon="mdi-close" variant="text" @click="closeDialog" :disabled="isSaving"></v-btn>
      </v-toolbar>

      <v-card-text class="pt-4 pb-0">
        <v-alert v-if="saveError" type="error" variant="tonal" closable class="mb-4" @click:close="saveError = null">{{
          saveError
        }}</v-alert>

        <v-form ref="formRef" @submit.prevent="submit">
          <v-text-field
            v-model="form.name"
            label="Item Name"
            variant="outlined"
            density="comfortable"
            :rules="[rules.required]"
          ></v-text-field>

          <div class="d-flex gap-3">
            <v-select
              v-model="form.type"
              label="Type"
              :items="serviceItemTypeOptions"
              item-title="label"
              item-value="value"
              variant="outlined"
              density="comfortable"
              :rules="[rules.required]"
              style="flex: 1.5"
            ></v-select>
            <v-text-field
              v-model="form.partNumber"
              label="Part Number"
              variant="outlined"
              density="comfortable"
              style="flex: 1"
            ></v-text-field>
          </div>

          <div class="d-flex gap-3">
            <v-number-input
              v-model="form.quantity"
              label="Quantity"
              variant="outlined"
              density="comfortable"
              :min="1"
              :step="1"
              :rules="[rules.required, rules.greaterThanZero]"
              style="flex: 1"
            ></v-number-input>
            <v-number-input
              v-model="form.unitPrice"
              label="Unit Price"
              variant="outlined"
              density="comfortable"
              :min="0"
              :step="0.01"
              :rules="[rules.required, rules.greaterOrEqualToZero]"
              style="flex: 1"
            ></v-number-input>
          </div>
          <v-textarea
            v-model="form.notes"
            label="Item Notes (Optional)"
            variant="outlined"
            density="comfortable"
            rows="2"
            auto-grow
          ></v-textarea>
        </v-form>
      </v-card-text>

      <v-card-actions class="justify-end pa-4 pt-2">
        <v-btn variant="text" @click="closeDialog" :disabled="isSaving">Cancel</v-btn>
        <v-btn color="primary" variant="tonal" @click="submit" :loading="isSaving" prepend-icon="mdi-content-save-outline">{{
          submitLabel
        }}</v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<style scoped>
.gap-3 {
  gap: 12px;
}
:deep(.v-number-input__control) {
  height: 48px;
}
</style>
