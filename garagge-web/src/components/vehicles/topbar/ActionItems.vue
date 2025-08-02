<script lang="ts" setup>
import { useRouter } from 'vue-router'

const router = useRouter()
function goToAddVehicle() {
  router.push('/vehicles/add')
}
</script>

<template>
  <div class="action-items-container">
    <v-btn variant="outlined" :disabled="true" prepend-icon="mdi-export-variant">Export</v-btn>
    <v-btn prepend-icon="mdi-plus" color="primary" @click="goToAddVehicle">Add car</v-btn>
  </div>
</template>

<style scoped>
.action-items-container {
  display: flex;
  align-items: center;
  justify-content: flex-end;
  gap: 0.5rem;
}
</style>
