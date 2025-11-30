import { ref, computed } from 'vue'
import type { ServiceItemDto } from '@/api/generated/apiV1.schemas'

export type ItemMode = 'create' | 'edit'

const isOpen = ref(false)
const mode = ref<ItemMode>('create')
const parentRecordId = ref<string | null>(null)

const selectedItem = ref<ServiceItemDto | null>(null)

const isDeleteOpen = ref(false)
const itemToDelete = ref<ServiceItemDto | null>(null)

export function useServiceItemState() {
  const create = (recordId: string) => {
    selectedItem.value = null
    parentRecordId.value = recordId
    mode.value = 'create'
    isOpen.value = true
  }

  const edit = (item: ServiceItemDto, recordId: string) => {
    selectedItem.value = item
    parentRecordId.value = recordId
    mode.value = 'edit'
    isOpen.value = true
  }

  const openDeleteDialog = (item: ServiceItemDto, recordId: string) => {
    itemToDelete.value = item
    parentRecordId.value = recordId
    isDeleteOpen.value = true
  }

  const cancelDelete = () => {
    isDeleteOpen.value = false
    setTimeout(() => {
      itemToDelete.value = null
      parentRecordId.value = null
    }, 300)
  }
  const close = () => {
    isOpen.value = false
    setTimeout(() => {
      selectedItem.value = null
      parentRecordId.value = null
      mode.value = 'create'
    }, 300)
  }

  return {
    isOpen: computed(() => isOpen.value),
    selectedItem: computed(() => selectedItem.value),
    mode: computed(() => mode.value),
    parentRecordId: computed(() => parentRecordId.value),
    isDeleteOpen: computed(() => isDeleteOpen.value),
    itemToDelete: computed(() => itemToDelete.value),
    create,
    edit,
    openDeleteDialog,
    cancelDelete,
    close,
  }
}
