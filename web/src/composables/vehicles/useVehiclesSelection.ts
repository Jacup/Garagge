import { ref, computed, watch, onMounted, onUnmounted } from 'vue'

interface UseVehiclesSelectionOptions {
  enableHistoryIntegration?: boolean
  onSelectionChange?: (selectedIds: string[]) => void
  maxSelection?: number
}

export function useVehiclesSelection(options: UseVehiclesSelectionOptions = {}) {
  const { enableHistoryIntegration = false, onSelectionChange, maxSelection } = options

  const selectedIds = ref<string[]>([])

  let isPoppingState = false

  const hasSelection = computed(() => selectedIds.value.length > 0)
  const selectedCount = computed(() => selectedIds.value.length)
  const canSelectMore = computed(() => maxSelection === undefined || selectedIds.value.length < maxSelection)

  function isSelected(id: string): boolean {
    return selectedIds.value.includes(id)
  }

  function toggle(id: string): boolean {
    const index = selectedIds.value.indexOf(id)

    if (index > -1) {
      selectedIds.value = selectedIds.value.filter((selectedId) => selectedId !== id)
      return false
    } else {
      if (!canSelectMore.value) {
        console.warn(`Selection limit reached: ${maxSelection}`)
        return false
      }
      selectedIds.value = [...selectedIds.value, id]
      return true
    }
  }

  function select(ids: string | string[]): void {
    const idsArray = Array.isArray(ids) ? ids : [ids]

    if (maxSelection !== undefined && idsArray.length > maxSelection) {
      console.warn(`Cannot select ${idsArray.length} items. Max: ${maxSelection}`)
      selectedIds.value = idsArray.slice(0, maxSelection)
    } else {
      selectedIds.value = idsArray
    }
  }

  function add(ids: string | string[]): void {
    const idsArray = Array.isArray(ids) ? ids : [ids]
    const uniqueNewIds = idsArray.filter((id) => !selectedIds.value.includes(id))

    if (maxSelection !== undefined) {
      const availableSlots = maxSelection - selectedIds.value.length
      if (availableSlots <= 0) {
        console.warn('Selection limit reached')
        return
      }
      selectedIds.value = [...selectedIds.value, ...uniqueNewIds.slice(0, availableSlots)]
    } else {
      selectedIds.value = [...selectedIds.value, ...uniqueNewIds]
    }
  }

  function remove(ids: string | string[]): void {
    const idsArray = Array.isArray(ids) ? ids : [ids]
    selectedIds.value = selectedIds.value.filter((id) => !idsArray.includes(id))
  }

  function selectAll(allIds: string[]): void {
    if (maxSelection !== undefined && allIds.length > maxSelection) {
      console.warn(`Cannot select all ${allIds.length} items. Max: ${maxSelection}`)
      selectedIds.value = allIds.slice(0, maxSelection)
    } else {
      selectedIds.value = [...allIds]
    }
  }

  function clear(): void {
    selectedIds.value = []
  }

  function invert(allIds: string[]): void {
    const currentSet = new Set(selectedIds.value)
    const inverted = allIds.filter((id) => !currentSet.has(id))

    if (maxSelection !== undefined && inverted.length > maxSelection) {
      console.warn(`Cannot invert selection: would exceed limit of ${maxSelection}`)
      return
    }

    selectedIds.value = inverted
  }

  const handlePopState = () => {
    if (hasSelection.value) {
      isPoppingState = true
      clear()
    }
  }

  if (enableHistoryIntegration) {
    watch(hasSelection, (hasSelectionNow) => {
      if (hasSelectionNow) {
        if (!history.state?.selectionMode) {
          history.pushState({ selectionMode: true }, '')
        }
      } else {
        if (isPoppingState) {
          isPoppingState = false
        } else {
          if (history.state?.selectionMode) {
            history.back()
          }
        }
      }
    })
  }

  if (enableHistoryIntegration) {
    onMounted(() => {
      window.addEventListener('popstate', handlePopState)
    })

    onUnmounted(() => {
      window.removeEventListener('popstate', handlePopState)

      if (history.state?.selectionMode) {
        history.back()
      }
    })
  }

  if (onSelectionChange) {
    watch(
      selectedIds,
      (newIds) => {
        onSelectionChange(newIds)
      },
      { deep: true },
    )
  }

  return {
    selectedIds,

    hasSelection,
    selectedCount,
    canSelectMore,

    isSelected,
    toggle,
    select,
    add,
    remove,
    selectAll,
    clear,
    invert,
  }
}

export type UseVehiclesSelectionReturn = ReturnType<typeof useVehiclesSelection>
