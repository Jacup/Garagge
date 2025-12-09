<script lang="ts" setup>
import type { SessionDto } from '@/api/generated'
import { computed } from 'vue'

const props = defineProps<{
  session?: SessionDto
  isLoading?: boolean
}>()

const emit = defineEmits(['deleteSession'])

const getDeviceIcon = (os: string | undefined | null) => {
  if (!os) return 'mdi-laptop'
  const lowerOs = os.toLowerCase()
  if (lowerOs.includes('windows')) {
    return 'mdi-microsoft-windows'
  }
  if (lowerOs.includes('mac')) {
    return 'mdi-apple'
  }
  if (lowerOs.includes('android')) {
    return 'mdi-android'
  }
  if (lowerOs.includes('ios')) {
    return 'mdi-apple-ios'
  }
  if (lowerOs.includes('linux')) {
    return 'mdi-linux'
  }
  return 'mdi-laptop'
}

const subtitle = computed(() => {
  if (props.session?.isCurrent) {
    return 'Active now'
  }
  return [props.session?.createdDate, props.session?.location].filter((i) => i).join(' • ')
})

function onDelete() {
  if (props.session?.id) {
    emit('deleteSession', props.session)
  }
}
</script>

<template>
  <v-skeleton-loader v-if="isLoading" type="list-item-two-line" class="inner-item"></v-skeleton-loader>
  <v-list-item v-else-if="session" lines="two">
    <template #prepend>
      <div class="d-flex align-center justify-center" style="width: 40px">
        <v-icon :icon="getDeviceIcon(session.deviceOs)" :color="session.isCurrent ? 'primary' : 'medium-emphasis'"></v-icon>
      </div>
    </template>

    <v-list-item-title>
      {{ session.deviceOs }} • {{ session.deviceBrowser }}
      <v-chip v-if="session.isCurrent" size="x-small" color="primary" variant="outlined" class="suggestion-chip ml-2">
        Current Device
      </v-chip>
    </v-list-item-title>

    <v-list-item-subtitle class="text-caption mt-1">
      <span :class="{ 'text-primary': session.isCurrent }">{{ subtitle }}</span>
    </v-list-item-subtitle>

    <template #append v-if="!session.isCurrent">
      <v-btn
        icon="mdi-logout"
        variant="text"
        color="error"
        density="comfortable"
        @click.stop="onDelete"
        v-tooltip:bottom="'Logout session'"
      ></v-btn>
    </template>
  </v-list-item>
</template>

<style scoped>
:deep(.v-list-item__prepend) {
  margin-right: 16px;
}

.v-list-item:has(.v-chip) {
  background-color: rgba(var(--v-theme-primary), 0.12) !important;
}

.suggestion-chip {
  border-color: rgb(var(--v-theme-outline)) !important;
  color: rgb(var(--v-theme-on-surface-variant)) !important;
  border-radius: 8px !important;
  font-weight: 500;
}
</style>
