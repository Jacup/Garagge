<script lang="ts" setup>
/**
 * Extended FAB - icon + text
 */

interface Props {
  icon: string
  text: string
}

defineProps<Props>()

const emit = defineEmits<{
  click: []
}>()
</script>

<template>
  <v-fab
    :icon="icon"
    :text="text"
    color="tertiary-container"
    size="large"
    location="bottom end"
    :aria-label="`${text} button`"
    class="extended-fab"
    @click="emit('click')"
  />
</template>

<style lang="scss" scoped>
.extended-fab {
  --fab-margin: 16px;
  --bottom-nav-height: 64px;

  position: fixed !important;
  right: var(--fab-margin);
  z-index: 1001;

  bottom: var(--fab-margin);

  @media (max-width: 959px) {
    bottom: calc(var(--bottom-nav-height) + var(--fab-margin));
  }

  :deep(.v-icon) {
    font-size: 28px !important;
  }
}
</style>
