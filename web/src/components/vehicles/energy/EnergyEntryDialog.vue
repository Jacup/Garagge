<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import axios from 'axios'
import { useResponsiveLayout } from '@/composables/useResponsiveLayout'
import type { EnergyEntryDto, EnergyEntryCreateRequest, EnergyEntryUpdateRequest, EnergyType } from '@/api/generated/apiV1.schemas'
import { getEnergyEntries } from '@/api/generated/energy-entries/energy-entries'
import EnergyEntryForm, { type EnergyFormData } from './EnergyEntryForm.vue'
import { useNotificationsStore } from '@/stores/notifications'

interface Props {
  modelValue: boolean
  vehicleId: string
  entry?: EnergyEntryDto | null
  allowedEnergyTypes?: EnergyType[]
}

const props = withDefaults(defineProps<Props>(), {
  modelValue: false,
  entry: null,
  allowedEnergyTypes: () => [],
})

const emit = defineEmits<{
  'update:modelValue': [value: boolean]
  saved: []
}>()

const { postApiVehiclesVehicleIdEnergyEntries, putApiVehiclesVehicleIdEnergyEntriesId } = getEnergyEntries()
const notifications = useNotificationsStore()

const { isMobile } = useResponsiveLayout()

const isLoading = ref(false)
const formData = ref<EnergyFormData>({})
const apiErrors = ref<Array<{ description: string }>>([])
const formComponentRef = ref<InstanceType<typeof EnergyEntryForm> | null>(null)

const isEditMode = computed(() => !!props.entry?.id)

const dialogTitle = computed(() => (isEditMode.value ? 'Edit Energy Entry' : 'Add Energy Entry'))

watch(
  () => props.modelValue,
  (isOpen) => {
    if (isOpen) {
      apiErrors.value = []

      if (props.entry) {
        formData.value = {
          ...props.entry,
          date: props.entry.date ? props.entry.date.split('T')[0] : undefined,
        }
      } else {
        formData.value = {
          date: new Date().toISOString().split('T')[0],
          type: undefined,
        }
      }
    }
  },
  { immediate: false },
)

function handleClose() {
  if (!isLoading.value) {
    emit('update:modelValue', false)
  }
}

async function handleSave() {
  const isValid = await formComponentRef.value?.validate()

  if (!isValid) {
    return
  }

  isLoading.value = true
  apiErrors.value = []

  try {
    if (isEditMode.value && props.entry?.id) {
      await putApiVehiclesVehicleIdEnergyEntriesId(props.vehicleId, props.entry.id, formData.value as EnergyEntryUpdateRequest)
      notifications.show('Fuel entry updated successfully.')
    } else {
      await postApiVehiclesVehicleIdEnergyEntries(props.vehicleId, formData.value as EnergyEntryCreateRequest)
      notifications.show('Fuel entry created successfully.')
    }

    emit('saved')
    emit('update:modelValue', false)
  } catch (error) {
    console.error('Save failed:', error)

    if (axios.isAxiosError(error)) {
      const data = error.response?.data

      if (data?.detail) {
        apiErrors.value = [{ description: data.detail }]
      } else if (data?.errors) {
        apiErrors.value = Object.values(data.errors)
          .flat()
          .map((msg) => ({
            description: msg as string,
          }))
      } else {
        apiErrors.value = [{ description: error.message }]
      }
    } else if (error instanceof Error) {
      apiErrors.value = [{ description: error.message }]
    } else {
      apiErrors.value = [{ description: 'An unexpected error occurred' }]
    }
  } finally {
    isLoading.value = false
  }
}

const isSaveDisabled = computed(() => isLoading.value)
</script>

<template>
  <v-dialog
    :model-value="modelValue"
    @update:model-value="emit('update:modelValue', $event)"
    :fullscreen="isMobile"
    :max-width="560"
    scrollable
    persistent
    transition="dialog-bottom-transition"
  >
    <template v-if="isMobile">
      <v-card class="d-flex flex-column h-100" rounded="0">
        <v-toolbar density="comfortable" color="surface">
          <v-btn icon="mdi-close" variant="text" class="ml-2" :disabled="isLoading" @click="handleClose" />

          <v-toolbar-title class="ml-2">
            {{ dialogTitle }}
          </v-toolbar-title>

          <v-spacer />

          <v-btn color="primary" variant="flat" class="mr-2" :loading="isLoading" :disabled="isSaveDisabled" @click="handleSave">
            Save
          </v-btn>
        </v-toolbar>

        <v-card-text class="mt-8">
          <EnergyEntryForm ref="formComponentRef" v-model="formData" :allowed-energy-types="allowedEnergyTypes" :api-errors="apiErrors" />
        </v-card-text>
      </v-card>
    </template>

    <template v-else>
      <v-card :title="dialogTitle" variant="flat" class="dialog-card">
        <v-card-text>
          <EnergyEntryForm ref="formComponentRef" v-model="formData" :allowed-energy-types="allowedEnergyTypes" :api-errors="apiErrors" />
        </v-card-text>

        <v-card-actions>
          <v-spacer />

          <v-btn variant="text" :disabled="isLoading" @click="handleClose"> Cancel </v-btn>

          <v-btn color="primary" variant="flat" :loading="isLoading" :disabled="isSaveDisabled" @click="handleSave"> Save </v-btn>
        </v-card-actions>
      </v-card>
    </template>
  </v-dialog>
</template>

<style scoped lang="scss"></style>
