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
    <v-card
      :title="`Delete ${itemToDelete}?`"
      :text="`This ${itemToDelete} will be permanently removed from the vehicle history. This action can't be undone.`"
      variant="flat"
      class="delete-dialog-card"
      rounded="xl"
      elevation="6"
    >
      <v-card-actions>
        <v-spacer></v-spacer>
        <v-btn variant="text" class="cancel-btn" @click="onCancel">Cancel</v-btn>
        <v-btn color="error" variant="text" @click="onConfirm">Delete</v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<style scoped>
.dialog-container {
  min-width: 280px;
  max-width: 560px;
}

/* Główny kontener dialogu */
.dialog-container :deep(.v-overlay__scrim) {
  background-color: rgba(var(--v-theme-scrim), 0.32) !important;
}

/* Card powierzchnia */
.delete-dialog-card {
  background-color: rgb(var(--v-theme-surface-container-high)) !important;
}

/* Tytuł - On Surface */
.delete-dialog-card :deep(.v-card-title) {
  color: rgb(var(--v-theme-on-surface)) !important;
}

/* Tekst - On Surface Variant */
.delete-dialog-card :deep(.v-card-text) {
  color: rgb(var(--v-theme-on-surface-variant)) !important;
}

/* Przycisk Cancel - Primary */
.cancel-btn {
  color: rgb(var(--v-theme-primary)) !important;
}

/* Przycisk Delete już ma color="error" więc automatycznie używa --v-theme-error */
</style>
