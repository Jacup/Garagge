import { ref, computed } from 'vue'
import type { ServiceRecordDto } from '@/api/generated/apiV1.schemas'

const isOpen = ref(false)
const selectedRecord = ref<ServiceRecordDto | null>(null)
const mode = ref<DetailsMode>('view')

export type DetailsMode = 'view' | 'create' | 'edit-metadata'

export function useServiceDetailsState() {
  const open = (record: ServiceRecordDto) => {
    selectedRecord.value = record
    mode.value = 'view'
    isOpen.value = true
  }

  const create = () => {
    selectedRecord.value = null
    mode.value = 'create'
    isOpen.value = true
  }

  const editMetadata = () => {
    if (selectedRecord.value) {
      mode.value = 'edit-metadata'
    }
  }

  const cancelAction = () => {
    if (mode.value === 'create') {
      close()
    } else if (mode.value === 'edit-metadata') {
      mode.value = 'view'
    }
  }

  const close = () => {
    isOpen.value = false
    setTimeout(() => {
      selectedRecord.value = null
      mode.value = 'view'
    }, 300)
  }

  return {
    isOpen: computed(() => isOpen.value),
    selectedRecord: computed(() => selectedRecord.value),
    mode: computed(() => mode.value),
    open,
    create,
    editMetadata,
    cancelAction,
    close,
  }
}
