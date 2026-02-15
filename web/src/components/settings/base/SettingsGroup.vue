<script lang="ts" setup>
import { ref } from 'vue'

const props = defineProps<{
  value: string
  title: string
  subtitle?: string
  icon: string
}>()

const opened = ref<string[]>([])

const handleUpdate = (newOpened: any[]) => {
  if (!newOpened.includes(props.value)) {
    opened.value = []
    return
  }

  const activeSubGroups = newOpened.filter((item) => item !== props.value)
  opened.value = [props.value, ...activeSubGroups.slice(-1)]
}
</script>

<template>
  <v-list :opened="opened" @update:opened="handleUpdate" rounded class="material-list">
    <v-list-group :value="value">
      <template v-slot:activator="{ props: activatorProps }">
        <v-list-item v-bind="activatorProps" lines="two" :title="title" :subtitle="subtitle" :prepend-icon="icon" />
      </template>

      <slot></slot>
    </v-list-group>
  </v-list>
</template>
