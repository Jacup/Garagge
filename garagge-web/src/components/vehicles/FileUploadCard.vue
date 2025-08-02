<script lang="ts" setup>
defineProps<{
  files: File[]
}>()

const emit = defineEmits<{
  'update:files': [files: File[]]
}>()

function handleFileUpload(event: Event) {
  const target = event.target as HTMLInputElement
  if (target.files) {
    const filesArray = Array.from(target.files)
    emit('update:files', filesArray)
  }
}

function handleFileDrop(event: DragEvent) {
  event.preventDefault()
  if (event.dataTransfer?.files) {
    const filesArray = Array.from(event.dataTransfer.files)
    emit('update:files', filesArray)
  }
}

function triggerFileInput() {
  const fileInput = document.querySelector('.file-input-hidden input') as HTMLInputElement
  fileInput?.click()
}
</script>

<template>
  <v-card class="file-upload-card">
    <v-card-title>Upload photo</v-card-title>
    <div class="upload-area">
      <v-file-input :model-value="files" accept="image/*" multiple prepend-icon="" class="file-input-hidden" @change="handleFileUpload" />
      <div class="drop-zone" @click="triggerFileInput" @dragover.prevent @drop.prevent="handleFileDrop">
        <v-icon size="48">mdi-upload-box</v-icon>
        <p class="upload-text">Drag and drop your images here</p>
        <p>or</p>
        <v-btn variant="tonal" size="small">Choose Files</v-btn>
      </div>
    </div>
  </v-card>
</template>

<style scoped>
.file-upload-card {
  flex: 1;
  display: flex;
  flex-direction: column;
  justify-content: center;
  max-width: 350px;
  align-items: center;
}

.upload-area {
  width: 100%;
  padding: 20px;
}

.file-input-hidden {
  display: none;
}

.drop-zone {
  border: 2px dashed #ccc;
  border-radius: 8px;
  padding: 40px 20px;
  text-align: center;
  cursor: pointer;
  transition: all 0.3s ease;
  background-color: var(--color-background-soft);
}

.drop-zone:hover {
  border-color: var(--color-primary);
  background-color: var(--color-background-mute);
}

.upload-text {
  margin: 16px 0;
  color: var(--color-text-muted);
  font-size: 14px;
}
</style>
