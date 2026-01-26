<script setup lang="ts">
import { ref } from 'vue'

import DeleteDialog from '@/components/common/DeleteDialog.vue'

import { useServiceItemState } from '@/composables/vehicles/useServiceItemState'
import { getServiceItems } from '@/api/generated/service-items/service-items'
import { useNotificationsStore } from '@/stores/notifications'

const props = defineProps<{
  vehicleId: string
}>()

const emit = defineEmits<{
  success: []
}>()

const { isDeleteOpen, itemToDelete, parentRecordId, cancelDelete } = useServiceItemState()
const { deleteApiVehiclesVehicleIdServiceRecordsServiceRecordIdServiceItemsServiceItemId } = getServiceItems()
const notifications = useNotificationsStore()

const isDeleting = ref(false)

const handleConfirm = async () => {
  if (!props.vehicleId || !parentRecordId.value || !itemToDelete.value) return

  isDeleting.value = true
  try {
    await deleteApiVehiclesVehicleIdServiceRecordsServiceRecordIdServiceItemsServiceItemId(
      props.vehicleId,
      parentRecordId.value,
      itemToDelete.value.id,
    )
    notifications.show('Service item deleted successfully.')
    emit('success')
    cancelDelete()
  } catch (error) {
    console.error('Failed to delete item', error)
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
