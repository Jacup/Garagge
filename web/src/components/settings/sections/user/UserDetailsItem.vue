<script lang="ts" setup>
import { ref, onMounted, computed } from 'vue'
import { useUserStore } from '@/stores/user'
import { useNotificationsStore } from '@/stores/notifications'
import { getUsers } from '@/api/generated/users/users'
import type { UserUpdateMeRequest } from '@/api/generated/apiV1.schemas'
import { parseApiError } from '@/utils/error-handler'

const { putApiUsersMe } = getUsers()
const userStore = useUserStore()
const notifications = useNotificationsStore()

const loading = ref(false)
const error = ref('')
const isFormValid = ref(false)
const formRef = ref()

const form = ref<UserUpdateMeRequest>({
  email: '',
  firstName: '',
  lastName: '',
})

const apiErrors = ref<Record<string, string>>({
  email: '',
  firstName: '',
  lastName: '',
})

const rules = {
  required: (value: string | null) => !!value || 'This field is required.',
  validEmail: (value: string) => /.+@.+\..+/.test(value) || 'Email is invalid.',
}

const isDirty = computed(() => {
  return (
    form.value.email !== (userStore.email || '') ||
    form.value.firstName !== (userStore.firstName || '') ||
    form.value.lastName !== (userStore.lastName || '')
  )
})

const resetForm = () => {
  form.value = {
    email: userStore.email || '',
    firstName: userStore.firstName || '',
    lastName: userStore.lastName || '',
  }

  apiErrors.value = { email: '', firstName: '', lastName: '' }
  error.value = ''

  if (formRef.value) {
    formRef.value.resetValidation()
  }
}

const clearFieldError = (field: string) => {
  if (apiErrors.value[field]) apiErrors.value[field] = ''
}

onMounted(() => {
  resetForm()
})

const handleUpdate = async () => {
  const { valid } = await formRef.value.validate()
  if (!valid) return

  loading.value = true
  apiErrors.value = { email: '', firstName: '', lastName: '' }
  error.value = ''

  try {
    await putApiUsersMe({
      email: form.value.email,
      firstName: form.value.firstName,
      lastName: form.value.lastName,
    })

    userStore.fetchUserData()

    notifications.show('User details updated successfully.')
  } catch (err: any) {
    if (err.response?.status === 400 && err.response?.data?.errors) {
      const backendErrors = err.response.data.errors
      const unmatchedErrors: string[] = []

      backendErrors.forEach((e: any) => {
        const desc = e.description || 'Invalid value'

        if (e.code?.includes('LastName')) apiErrors.value.lastName = desc
        else if (e.code?.includes('FirstName')) apiErrors.value.firstName = desc
        else if (e.code?.includes('Email')) apiErrors.value.email = desc
        else {
          unmatchedErrors.push(desc)
        }
      })

      if (unmatchedErrors.length > 0) {
        error.value = unmatchedErrors.join('\n')
      }
    } else {
      const parsedError = parseApiError(err)
      error.value = parsedError?.message || 'Failed to update user details.'
    }
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <v-list-item class="inner-item">
    <v-form ref="formRef" v-model="isFormValid" :disabled="loading" @submit.prevent="handleUpdate">
      <v-text-field
        v-model="form.email"
        label="Email"
        type="email"
        variant="outlined"
        density="comfortable"
        class="mt-4 mb-2"
        :rules="[rules.required, rules.validEmail]"
        :error-messages="apiErrors.email"
        @update:model-value="clearFieldError('email')"
        required
      />

      <v-text-field
        v-model="form.firstName"
        label="First name"
        variant="outlined"
        density="comfortable"
        class="mb-2"
        counter="64"
        maxlength="64"
        :rules="[rules.required]"
        :error-messages="apiErrors.firstName"
        @update:model-value="clearFieldError('firstName')"
      />

      <v-text-field
        v-model="form.lastName"
        label="Last name"
        variant="outlined"
        density="comfortable"
        class="mb-2"
        counter="64"
        maxlength="64"
        :rules="[rules.required]"
        :error-messages="apiErrors.lastName"
        @update:model-value="clearFieldError('lastName')"
      />

      <v-expand-transition>
        <v-alert v-if="error" type="error" variant="tonal" closable class="mb-4" @click:close="error = ''">
          {{ error }}
        </v-alert>
      </v-expand-transition>

      <div class="d-flex justify-end ga-2 mt-2">
        <v-btn variant="text" :disabled="!isDirty || loading" @click="resetForm"> Reset </v-btn>

        <v-btn color="primary" variant="flat" :loading="loading" :disabled="!isDirty || !isFormValid" type="submit"> Save Changes </v-btn>
      </div>
    </v-form>
  </v-list-item>
</template>
