<script lang="ts" setup>
import { ref, computed, nextTick } from 'vue'
import { getUsers } from '@/api/generated/users/users'
import type { ChangePasswordRequest } from '@/api/generated/apiV1.schemas'
import { parseApiError } from '@/utils/error-handler'
import { useNotificationsStore } from '@/stores/notifications'

const { putApiUsersMeChangePassword } = getUsers()
const notifications = useNotificationsStore()

const formRef = ref()
const loading = ref(false)
const globalError = ref('')
const isFormValid = ref(false)

const showCurrentPassword = ref(false)
const showNewPassword = ref(false)

const form = ref<ChangePasswordRequest>({
  currentPassword: '',
  newPassword: '',
  logoutAllDevices: false,
})

const apiErrors = ref({
  currentPassword: '',
  newPassword: '',
})

const rules = {
  required: (v: string | null) => !!v || 'This field is required',
  minLength: (v: string) => v.length >= 8 || 'At least 8 characters',
}

const isDirty = computed(() => {
  return !!form.value.currentPassword || !!form.value.newPassword
})

const handleUpdate = async () => {
  const { valid } = await formRef.value.validate()
  if (!valid) return

  loading.value = true
  clearErrors()

  try {
    await putApiUsersMeChangePassword({
      currentPassword: form.value.currentPassword,
      newPassword: form.value.newPassword,
      logoutAllDevices: form.value.logoutAllDevices,
    })

    notifications.show('Password changed successfully')
    resetForm()
  } catch (err: any) {
    handleApiError(err)
  } finally {
    loading.value = false
  }
}

const handleApiError = (err: any) => {
  if (err.response?.status === 400 && err.response?.data?.errors) {
    const backendErrors = err.response.data.errors
    const unmatchedErrors: string[] = []

    backendErrors.forEach((e: any) => {
      const desc = e.description || 'Invalid value'
      if (e.code?.includes('CurrentPassword')) {
        apiErrors.value.currentPassword = desc
      } else if (e.code?.includes('NewPassword')) {
        apiErrors.value.newPassword = desc
      } else {
        unmatchedErrors.push(desc)
      }
    })

    if (unmatchedErrors.length > 0) {
      globalError.value = unmatchedErrors.join('\n')
    }
  } else {
    const parsedError = parseApiError(err)
    globalError.value = parsedError?.message || 'Failed to update password'
  }
}

const clearErrors = () => {
  apiErrors.value = { currentPassword: '', newPassword: '' }
  globalError.value = ''
}

const resetForm = async () => {
  form.value = {
    currentPassword: '',
    newPassword: '',
    logoutAllDevices: false,
  }

  clearErrors()
  await nextTick()
  formRef.value?.resetValidation()
}
</script>

<template>
  <v-list-group value="password">
    <template v-slot:activator="{ props }">
      <v-list-item v-bind="props" lines="two" subtitle="Change your password" prepend-icon="mdi-form-textbox-password">
        <template #title>
          <div class="d-flex ga-2">
            Password
            <v-chip v-if="isDirty" size="small" density="compact" color="warning" class="suggestion-chip" variant="outlined">
              Unsaved changes
            </v-chip>
          </div>
        </template>
      </v-list-item>
    </template>

    <v-list-item lines="three">
      <v-form ref="formRef" v-model="isFormValid" :disabled="loading" @submit.prevent="handleUpdate">
        <v-text-field
          v-model="form.currentPassword"
          label="Current password"
          variant="outlined"
          density="comfortable"
          class="py-2"
          :type="showCurrentPassword ? 'text' : 'password'"
          :append-inner-icon="showCurrentPassword ? 'mdi-eye-off' : 'mdi-eye'"
          @click:append-inner="showCurrentPassword = !showCurrentPassword"
          :rules="[rules.required]"
          :error-messages="apiErrors.currentPassword"
          @update:model-value="apiErrors.currentPassword = ''"
        />

        <v-text-field
          v-model="form.newPassword"
          label="New password"
          variant="outlined"
          density="comfortable"
          counter
          class="pb-2"
          :type="showNewPassword ? 'text' : 'password'"
          :append-inner-icon="showNewPassword ? 'mdi-eye-off' : 'mdi-eye'"
          @click:append-inner="showNewPassword = !showNewPassword"
          :rules="[rules.required, rules.minLength]"
          :error-messages="apiErrors.newPassword"
          @update:model-value="apiErrors.newPassword = ''"
        />

        <div class="d-flex align-center justify-space-between mb-4 w-100">
          <div class="pr-2 flex-grow-1">
            <div class="text-body-2 font-weight-medium">Logout from all devices</div>
            <div class="text-caption text-medium-emphasis">Highly recommended if you think your account is compromised</div>
          </div>
          <v-switch v-model="form.logoutAllDevices" color="primary" hide-details inset density="compact" />
        </div>

        <v-expand-transition>
          <v-alert v-if="globalError" type="error" variant="tonal" closable class="mb-4" @click:close="globalError = ''">
            {{ globalError }}
          </v-alert>
        </v-expand-transition>

        <div class="d-flex justify-end ga-2">
          <v-btn variant="text" :disabled="!isDirty || loading" @click="resetForm"> Reset </v-btn>

          <v-btn color="primary" variant="flat" :loading="loading" :disabled="!isFormValid" type="submit"> Save Changes </v-btn>
        </div>
      </v-form>
    </v-list-item>
  </v-list-group>
</template>
