<script lang="ts" setup>
interface Props {
  icon: string
  label: string
  color?: string
  iconColor?: string
  to?: string
  href?: string
  loading?: boolean
  disabled?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  color: 'primary',
  iconColor: 'on-primary',
  loading: false,
  disabled: false,
})

const emit = defineEmits<{
  (e: 'click', event: MouseEvent): void
}>()

const handleClick = (event: MouseEvent) => {
  if (!props.loading && !props.disabled) {
    emit('click', event)
  }
}
</script>

<template>
  <div class="action-btn-wrapper">
    <v-btn
      :color="color"
      :loading="loading"
      :disabled="disabled"
      :to="to"
      :href="href"
      class="action-btn mb-2"
      flat
      :icon="false"
      @click="handleClick"
    >
      <v-icon :color="iconColor" size="24">{{ icon }}</v-icon>
    </v-btn>

    <span class="action-label" :class="{ 'text-disabled': disabled }">
      {{ label }}
    </span>
  </div>
</template>

<style lang="scss" scoped>
.action-btn-wrapper {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: flex-start;
  width: min-content;
}

.action-btn {
  width: 70px !important;
  height: 56px !important;
  border-radius: 9999px !important;

  letter-spacing: normal;
  text-transform: none;
}

.action-label {
  font-family: 'Roboto', sans-serif; /* Lub twoja czcionka */
  font-size: 14px;
  font-weight: 500;
  line-height: 1.2;
  text-align: center;
  color: rgb(var(--v-theme-on-surface));

  white-space: nowrap;
  max-width: 100px;
  overflow: hidden;
  text-overflow: ellipsis;
}

.text-disabled {
  opacity: 0.5;
}
</style>
