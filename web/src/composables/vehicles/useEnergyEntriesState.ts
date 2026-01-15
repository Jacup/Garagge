import { ref, readonly } from 'vue'
import type { EnergyEntryDto } from '@/api/generated/apiV1.schemas'

const showEntryDialog = ref(false)
const selectedEntry = ref<EnergyEntryDto | null>(null)

export function useEnergyEntriesState() {
  const openCreateDialog = () => {
    selectedEntry.value = null
    showEntryDialog.value = true
  }

  const openEditDialog = (entry: EnergyEntryDto) => {
    selectedEntry.value = entry
    showEntryDialog.value = true
  }

  const closeDialog = () => {
    showEntryDialog.value = false
    setTimeout(() => {
      selectedEntry.value = null
    }, 300)
  }

  return {
    showEntryDialog: readonly(showEntryDialog),
    selectedEntry: readonly(selectedEntry),
    openCreateDialog,
    openEditDialog,
    closeDialog,
  }
}
