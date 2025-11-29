<script setup lang="ts">
import { ref } from 'vue'

import DeleteDialog from '@/components/common/DeleteDialog.vue'

import { useServiceItemState } from '@/composables/vehicle/useServiceItemState'
import { getServiceItems } from '@/api/generated/service-items/service-items'

const props = defineProps<{
  vehicleId: string
}>()

// Emitujemy sukces, żeby rodzic mógł odświeżyć dane
const emit = defineEmits<{
  success: []
}>()

// 1. Pobieramy stan UI z composable
const { isDeleteOpen, itemToDelete, parentRecordId, cancelDelete } = useServiceItemState()

// 2. Pobieramy hook API
const { deleteApiVehiclesVehicleIdServiceRecordsServiceRecordIdServiceItemsServiceItemId } = getServiceItems()

// 3. Lokalny stan ładowania (dotyczy tylko tej konkretnej akcji)
const isDeleting = ref(false)

// 4. Logika wykonania usunięcia
const handleConfirm = async () => {
  if (!props.vehicleId || !parentRecordId.value || !itemToDelete.value) return

  isDeleting.value = true
  try {
    await deleteApiVehiclesVehicleIdServiceRecordsServiceRecordIdServiceItemsServiceItemId(
      props.vehicleId,
      parentRecordId.value,
      itemToDelete.value.id,
    )
    // Sukces
    emit('success')
    cancelDelete()
  } catch (error) {
    console.error('Failed to delete item', error)
    // Opcjonalnie: obsługa błędu
  } finally {
    isDeleting.value = false
  }
}
</script>

<template>
  <DeleteDialog
    :is-open="isDeleteOpen"
    :item-to-delete="itemToDelete ? `service item '${itemToDelete.name}'` : 'item'"
    :on-confirm="handleConfirm"
    :on-cancel="cancelDelete"
  />
</template>
