<script lang="ts" setup>
// Server info component - visible only in expanded navigation
</script>

<template>
  <div class="px-6">
    <v-card variant="outlined" density="compact">
      <v-card-text class="server-info">
        <div>
          <v-icon color="success" icon="mdi-circle-medium"></v-icon>
        </div>
        <div>
          <v-chip size="small" flat rounded="pill" class="font-mono">v1.2.3</v-chip>
        </div>
      </v-card-text>
    </v-card>
  </div>
</template>

<style lang="scss" scoped>
.server-info {
  display: flex;
  align-items: center;
  gap: 12px;
}
</style>
