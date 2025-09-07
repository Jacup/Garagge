<script setup lang="ts">
interface Props {
  itemToDelete: string
  isOpen: boolean
  onConfirm: () => void
  onCancel: () => void
}

const props = defineProps<Props>()
</script>

<template>
  <v-dialog :model-value="isOpen" class="dialog-container" @update:model-value="props.onCancel">
    <v-card variant="flat" class="dialog-card" rounded="xl" elevation="6">
      <template v-slot:title>
        <p>Delete {{ itemToDelete }}?</p>
      </template>
      <template v-slot:text>
        <p>This {{ itemToDelete }} will be permanently removed. This action can't be undone.</p>
      </template>

      <v-card-actions>
        <v-spacer></v-spacer>
        <v-btn variant="text" class="cancel-btn text-none" @click="onCancel">Cancel</v-btn>
        <v-btn variant="text" class="text-none" color="error" @click="onConfirm">Delete</v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<style scoped>
.dialog-container {
  min-width: 280px;
  max-width: 560px;
}

.dialog-card {
  background-color: rgb(var(--v-theme-surface-container-high)) !important;
}

.dialog-card :deep(.v-card-title) {
  color: rgb(var(--v-theme-on-surface)) !important;
}

.dialog-card :deep(.v-card-text) {
  color: rgb(var(--v-theme-on-surface-variant)) !important;
}

.dialog-card :deep(.v-card-actions) {
  padding: 0 24px 24px 24px !important;
}

.cancel-btn {
  color: rgb(var(--v-theme-primary)) !important;
}
</style>
