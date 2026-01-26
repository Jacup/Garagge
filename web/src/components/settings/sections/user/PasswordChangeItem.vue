<script lang="ts" setup>
import { ref } from 'vue'
import { getAuth } from '@/api/generated/auth/auth'
import { parseApiError } from '@/utils/error-handler'
import { useNotificationsStore } from '@/stores/notifications'

const { putApiAuthChangePassword } = getAuth()
const notifications = useNotificationsStore()

const isExpanded = ref(false)
const loading = ref(false)
const error = ref('')
const formRef = ref()

const form = ref({
  currentPassword: '',
  newPassword: '',
  logoutAll: false,
})

const rules = {
  required: (value: string | null) => !!value || 'This field is required.',
  passwordMinLength: (value: string) => value.length >= 8 || 'Password must be at least 8 characters long.',
}

const handleUpdate = async () => {
  const { valid } = await formRef.value.validate()
  if (!valid) return

  loading.value = true
  error.value = ''

  try {
    await putApiAuthChangePassword({
      currentPassword: form.value.currentPassword,
      newPassword: form.value.newPassword,
      logoutAllDevices: form.value.logoutAll,
    })

    notifications.show('Password changed successfully.')
    if (formRef.value) {
      formRef.value.reset()
    }
  } catch (err) {
    const parsedError = parseApiError(err)
    if (parsedError) {
      error.value = parsedError.message
    } else {
      error.value = 'Failed to change password.'
    }
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <v-list-group v-model="isExpanded" class="md3-list-group">
    <template #activator="{ props, isOpen }">
      <v-list-item v-bind="props" class="md3-list-group-activator" :class="{ 'is-open': isOpen }">
        <v-list-item-title>Password</v-list-item-title>
        <v-list-item-subtitle v-if="!isExpanded"> Change your account password </v-list-item-subtitle>
      </v-list-item>
    </template>

    <v-list-item class="inner-item">
      <v-form ref="formRef" :disabled="loading" @submit.prevent="handleUpdate">
        <v-text-field
          v-model="form.currentPassword"
          label="Current password"
          type="password"
          variant="outlined"
          density="compact"
          class="my-2"
          :rules="[rules.required]"
          required
        />

        <v-text-field
          v-model="form.newPassword"
          label="New password"
          type="password"
          variant="outlined"
          density="compact"
          class="my-4"
          :rules="[rules.required, rules.passwordMinLength]"
          required
        />

        <div class="d-flex align-center justify-space-between mb-4 w-100">
          <div class="pr-2 flex-grow-1 flex-shrink-1" style="min-width: 0">
            <div class="text-body-medium text-wrap">Logout from all devices</div>
            <div class="text-caption text-secondary text-wrap">Highly recommended if you think your account is compromised</div>
          </div>
          <div class="flex-shrink-0">
            <v-switch v-model="form.logoutAll" color="primary" hide-details inset />
          </div>
        </div>

        <v-expand-transition>
          <v-alert v-if="error" type="error" variant="tonal" closable class="mb-4" @click:close="error = ''">
            {{ error }}
          </v-alert>
        </v-expand-transition>

        <div class="d-flex justify-end mt-4">
          <v-btn color="primary" variant="flat" :loading="loading" type="submit"> Update Password </v-btn>
        </div>
      </v-form>
    </v-list-item>
  </v-list-group>
</template>
