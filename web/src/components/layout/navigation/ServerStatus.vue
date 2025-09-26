<template>
  <div class="system-navigation">
    <v-list nav class="px-4" base-color="on-surface-variant" active-color="secondary">
      <v-list-item
        :key="'Server Online'"
        :title="'Server Online'"
        :min-height="48"
        rounded="pill"
        class="px-4 server-status-item"
        base-color="on-surface-variant"
        active-color="secondary"
        :ripple="false"
        link
        variant="tonal"
      >
        <template #prepend>
          <v-badge location="top right" :offset-y="-5" :offset-x="3" color="success" dot>
            <v-icon :size="24" class="mr-2">mdi-server</v-icon>
          </v-badge>
        </template>
        <template #append>
          <v-chip size="x-small" flat rounded="pill" class="roboto-mono">v1.2.0</v-chip>
        </template>
      </v-list-item>
    </v-list>
  </div>
</template>

<style lang="scss" scoped>
.system-navigation {
  flex: 0 0 auto;
  margin-top: auto;
}

.server-status-item {
  width: 100% !important;
  min-width: 100% !important;
}
</style>
