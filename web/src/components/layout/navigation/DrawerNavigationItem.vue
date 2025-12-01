<script lang="ts" setup>
import type { NavigationItem } from '@/constants/navigation'
import { useNavigationItem } from '@/composables/useNavigation'

interface Props {
  item: NavigationItem
}

const { item } = defineProps<Props>()

const emit = defineEmits<{ navigate: [] }>()

const { currentIcon, isActive, navigate } = useNavigationItem(item)

const handleClick = () => {
  emit('navigate')
  navigate()
}
</script>

<template>
  <v-list-item
    :title="item.title"
    :min-height="56"
    :active="isActive"
    rounded="pill"
    class="px-4"
    base-color="on-surface-variant"
    color="secondary"
    link
    @click="handleClick"
  >
    <template #prepend>
      <v-icon :size="24" class="mr-2">
        {{ currentIcon }}
      </v-icon>
    </template>
  </v-list-item>
</template>
