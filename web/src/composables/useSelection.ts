import { ref, computed, onUnmounted } from 'vue'

interface SelectableItem {
  id: string | undefined
}

export function useSelection<T extends SelectableItem>(
  items: T[],
  initialSelection: string[] = [],
  emit: (event: 'update:modelValue', value: string[]) => void,
) {
  const selectedIds = computed({
    get: () => initialSelection,
    set: (val) => emit('update:modelValue', val),
  })

  const isSelected = (id: string | undefined) => {
    return id ? selectedIds.value.includes(id) : false
  }

  const toggleSelection = (id: string | undefined) => {
    if (!id) return

    const currentIds = [...selectedIds.value]
    const index = currentIds.indexOf(id)

    if (index === -1) {
      currentIds.push(id)
    } else {
      currentIds.splice(index, 1)
    }

    selectedIds.value = currentIds
  }

  const longPressTimeout = ref<ReturnType<typeof setTimeout> | null>(null)
  const isLongPressHandled = ref(false)

  const handleTouchStart = (item: T) => {
    isLongPressHandled.value = false
    longPressTimeout.value = setTimeout(() => {
      isLongPressHandled.value = true
      if (navigator.vibrate) navigator.vibrate(20)
      toggleSelection(item.id)
    }, 500)
  }

  const handleTouchMove = () => {
    if (longPressTimeout.value) {
      clearTimeout(longPressTimeout.value)
      longPressTimeout.value = null
    }
  }

  const handleTouchEnd = () => {
    if (longPressTimeout.value) {
      clearTimeout(longPressTimeout.value)
      longPressTimeout.value = null
    }
  }

  onUnmounted(() => {
    if (longPressTimeout.value) clearTimeout(longPressTimeout.value)
  })

  return {
    selectedIds,
    isSelected,
    toggleSelection,
    handleTouchStart,
    handleTouchMove,
    handleTouchEnd,
    isLongPressHandled,
  }
}
