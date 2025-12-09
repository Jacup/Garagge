<script lang="ts" setup>
import { ref, computed, onMounted } from 'vue'
import { getUsers, type SessionDto } from '@/api/generated'
import SessionListItem from '@/components/settings/sections/sessions/SessionListItem.vue'
import DeleteDialog from '@/components/common/DeleteDialog.vue'

const { getApiUsersMeSessions, deleteApiUsersMeSessionsId } = getUsers()
const sessions = ref<SessionDto[]>([])

const isLoading = ref(true)

const isDeleteDialogOpen = ref(false)
const sessionToDelete = ref<SessionDto | null>(null)

const currentSession = computed(() => sessions.value.find((s) => s.isCurrent))
const otherSessions = computed(() => sessions.value.filter((s) => !s.isCurrent))

onMounted(async () => {
  await loadSessions()
})

async function loadSessions() {
  try {
    isLoading.value = true
    const res = await getApiUsersMeSessions()
    sessions.value = res.items ?? []
  } catch (e) {
    console.error(e)
  } finally {
    isLoading.value = false
  }
}

const openDeleteDialog = (session: SessionDto) => {
  sessionToDelete.value = session
  isDeleteDialogOpen.value = true
}

const closeDeleteDialog = () => {
  sessionToDelete.value = null
  isDeleteDialogOpen.value = false
}

const deleteSession = (session: SessionDto) => {
  openDeleteDialog(session)
}

const confirmDelete = async () => {
  if (!sessionToDelete.value?.id) {
    return
  }

  try {
    await deleteApiUsersMeSessionsId(sessionToDelete.value.id)
    await loadSessions()
  } catch (error) {
    console.error(error)
  } finally {
    closeDeleteDialog()
  }
}

const deleteOtherSessions = () => {
  /* ... */
}
</script>

<template>
  <template v-if="isLoading">
    <SessionListItem :is-loading="true" class="inner-item" />
    <SessionListItem :is-loading="true" class="inner-item" />
  </template>
  <template v-else>
    <SessionListItem v-if="currentSession" :session="currentSession" class="inner-item" />

    <SessionListItem
      v-for="session in otherSessions"
      :key="session.id"
      :session="session"
      @deleteSession="deleteSession"
      class="inner-item"
    />

    <v-list-item class="inner-item">
      <template #append>
        <v-btn color="error" variant="text" disabled @click="deleteOtherSessions"> Sign out all sessions</v-btn>
      </template>
    </v-list-item>
  </template>

  <DeleteDialog
    :is-open="isDeleteDialogOpen"
    :item-to-delete="sessionToDelete?.deviceOs || 'session'"
    :on-cancel="closeDeleteDialog"
    :on-confirm="confirmDelete"
  />
</template>
