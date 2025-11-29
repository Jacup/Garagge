import { ref, computed } from 'vue'
import type { ServiceItemDto } from '@/api/generated/apiV1.schemas'

export type ItemMode = 'create' | 'edit'

const isOpen = ref(false)
const selectedItem = ref<ServiceItemDto | null>(null)
const mode = ref<ItemMode>('create')
const parentRecordId = ref<string | null>(null)

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
    create,
    edit,
    close,
  }
}
