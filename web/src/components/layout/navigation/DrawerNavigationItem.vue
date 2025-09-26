<script lang="ts" setup>
import type { NavigationItem } from '@/constants/navigation'
import { useNavigationItem } from '@/composables/useNavigation'

interface Props {
  item: NavigationItem
}

const { item } = defineProps<Props>()

const emit = defineEmits<{ navigate: [] }>()

const { currentIcon } = useNavigationItem(item)
</script>

<template>
  <v-list-item
    :title="item.title"
    :to="item.link"
    :min-height="56"
    rounded="pill"
    class="px-4"
    base-color="on-surface-variant"
    active-color="secondary"
    link
    @click="emit('navigate')"
  >
    <template #prepend>
      <v-icon :size="24" class="mr-2">
        {{ currentIcon }}
      </v-icon>
    </template>
  </v-list-item>
</template>
