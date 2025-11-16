<script lang="ts" setup>
interface Props {
  icon: string
  text: string
  isExtended?: boolean
}

defineProps<Props>()

const emit = defineEmits<{
  click: []
}>()
</script>

<template>
  <v-fab
    v-if="isExtended"
    extended
    :prepend-icon="icon"
    :text="text"
    color="tertiary-container"
    :aria-label="`${text} button`"
    class="extended-fab"
    height="80"
    @click="emit('click')"
  />
  <v-fab
    v-else
    :icon="icon"
    color="tertiary-container"
    :size="80"
    :aria-label="`${text} button`"
    class="regular-fab"
    @click="emit('click')"
  />
</template>

<style lang="scss" scoped>
.regular-fab {
  :deep(.v-icon) {
    font-size: 28px !important;
  }
}

.extended-fab {
  :deep(.v-btn) {
    padding-inline-start: 26px !important;
    padding-inline-end: 26px !important;
  }

  :deep(.v-icon) {
    font-size: 28px !important;
  }

  :deep(.v-btn__content) {
    font-family: 'Roboto', sans-serif;
    font-weight: 400;
    font-size: 22px;
    line-height: 28px;
    letter-spacing: 0;
  }

  // Leading space: 26dp, Icon-label space: 12dp, Trailing space: 26dp
  :deep(.v-btn__prepend) {
    margin-inline-start: 0;
    margin-inline-end: 12px;
  }
}
</style>
