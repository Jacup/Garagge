<script setup lang="ts">
import { computed, ref, reactive, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ServiceItemType } from '../../types/serviceItemType'

import type { ServiceTypeDto, ServiceRecordCreateRequest, ServiceItemType as ApiServiceItemType } from '@/api/generated/apiV1.schemas'
import { getServiceRecords } from '@/api/generated/service-records/service-records'

const { getApiVehiclesServiceRecordsTypes, postApiVehiclesVehicleIdServiceRecords } = getServiceRecords()

const route = useRoute()
const router = useRouter()

const props = defineProps({
  id: {
    type: String,
    required: true,
  },
})

const serviceTypes = ref([] as ServiceTypeDto[])

const vehicleName = computed(() => {
  const brand = route.query.brand || ''
  const model = route.query.model || ''
  return `${brand} ${model}`.trim()
})

async function loadServiceTypes() {
  try {
    const res = await getApiVehiclesServiceRecordsTypes()
    serviceTypes.value = res.data ?? []
  } catch (error) {
    console.error('Error loading service types:', error)
    serviceTypes.value = []
  }
}

onMounted(() => {
  loadServiceTypes()
})

// Form ref
interface ValidateResult {
  valid: boolean
}
type FormRefType = { validate: () => Promise<ValidateResult> } | null
const formRef = ref<FormRefType>(null)
const submitting = ref(false)
const errorMessage = ref('')

// Reactive form model
const form = reactive({
  title: '',
  recordType: null as string | null,
  notes: '',
  serviceDate: '',
  manualCost: null as number | null,
  mileage: null as number | null,
  items: [
    {
      name: '',
      type: null as ServiceItemType | null,
      partNumber: '',
      quantity: null as number | null,
      unitPrice: null as number | null,
      notes: '',
    },
  ],
})

// FORM VALIDATION RULES
const MAX_FIELD_LENGTH = 64
const MAX_NOTES_LENGTH = 500

const rules = {
  required: (v: string | number | null) => !!v || 'This field is required',
  textFieldCounter: (value: string) => value.length <= MAX_FIELD_LENGTH || `Max ${MAX_FIELD_LENGTH} characters`,
  textAreaCounter: (value: string) => value.length <= MAX_NOTES_LENGTH || `Max ${MAX_NOTES_LENGTH} characters`,
  greaterOrEqualToZero: (v: number) => v === null || v === undefined || v >= 0 || 'Value must be greater or equal to 0',
  greaterThanZero: (v: number) => v === null || v === undefined || v > 0 || 'Value must be greater than 0',
}

const serviceItemTypeOptions = Object.keys(ServiceItemType)
  .filter((k) => isNaN(Number(k)))
  .map((k) => ({ label: k, value: ServiceItemType[k as keyof typeof ServiceItemType] }))

function addItem() {
  form.items.push({ name: '', type: null, partNumber: '', quantity: null, unitPrice: null, notes: '' })
}

function removeItem(index: number) {
  if (index >= 0 && index < form.items.length) form.items.splice(index, 1)
}

function removeAllItems() {
  form.items.splice(0, form.items.length)
}

async function submit() {
  const result = await formRef.value?.validate()
  if (!result?.valid) {
    errorMessage.value = 'Please correct all form errors before submitting'
    return
  }
  errorMessage.value = ''

  submitting.value = true

  const payload: ServiceRecordCreateRequest = {
    title: form.title,
    serviceDate: form.serviceDate,
    serviceTypeId: form.recordType!,
    mileage: form.mileage,
    manualCost: form.manualCost,
    serviceItems: form.items.map((i) => ({
      name: i.name,
      type: i.type! as unknown as ApiServiceItemType,
      partNumber: i.partNumber || null,
      quantity: i.quantity!,
      unitPrice: i.unitPrice!,
      notes: null,
    })),
    notes: form.notes || null,
  }

  try {
    await postApiVehiclesVehicleIdServiceRecords(props.id, payload)
    await router.push({ name: 'VehicleOverview', params: { id: props.id } })
  } catch (error) {
    const errorMsg = error instanceof Error ? error.message : 'Failed to create service record'
    errorMessage.value = errorMsg
    console.error('Error creating service record:', error)
  } finally {
    submitting.value = false
  }
}
</script>

<template>
  <v-row>
    <v-col cols="12">
      <v-card class="card-background" variant="flat" rounded="md-16px">
        <v-card-text class="d-flex justify-space-between">
          <v-spacer></v-spacer>

          <div class="vehicle-info">{{ vehicleName }}</div>
        </v-card-text>

        <v-form ref="formRef">
          <v-card-text v-if="errorMessage" class="error-message" role="alert">
            <v-alert type="error" :text="errorMessage" closable @click:close="errorMessage = ''" />
          </v-card-text>

          <v-card-text>
            <v-row>
              <v-col cols="12" md="6">
                <v-text-field
                  v-model="form.title"
                  clearable
                  label="Title"
                  variant="outlined"
                  :rules="[rules.required, rules.textFieldCounter]"
                ></v-text-field>
                <v-select
                  v-model="form.recordType"
                  clearable
                  label="Type"
                  :items="serviceTypes"
                  item-title="name"
                  item-value="id"
                  variant="outlined"
                  :rules="[rules.required]"
                ></v-select>
                <v-textarea v-model="form.notes" clearable label="Notes" variant="outlined" :rules="[rules.textAreaCounter]"></v-textarea>
              </v-col>
              <v-col cols="12" md="6">
                <v-text-field
                  v-model="form.serviceDate"
                  label="Date"
                  type="date"
                  variant="outlined"
                  density="comfortable"
                  :rules="[rules.required]"
                  class="form-field"
                />
                <v-number-input v-model="form.mileage" label="Mileage" clearable variant="outlined" :min="0" :step="100"></v-number-input>
                <v-number-input
                  v-model="form.manualCost"
                  label="Manual cost"
                  clearable
                  variant="outlined"
                  :min="0"
                  :step="0.01"
                  :rules="[rules.greaterOrEqualToZero]"
                ></v-number-input>
              </v-col>
            </v-row>
          </v-card-text>

          <v-card-text>
            <div class="d-flex justify-space-between">
              <div class="text-h6">Items</div>
              <v-spacer></v-spacer>
              <v-btn color="primary" prepend-icon="mdi-plus" variant="text" @click.prevent="addItem">Add item</v-btn>

              <v-tooltip v-if="form.items.length > 0" text="Remove all items" location="bottom" open-delay="200" close-delay="1000">
                <template v-slot:activator="{ props }">
                  <v-btn color="error" icon="mdi-close" variant="text" density="comfortable" v-bind="props" @click.prevent="removeAllItems"></v-btn>
                </template>
              </v-tooltip>

            </div>
            <div v-if="form.items.length < 1">No items added. Click "Add item" to add service items.</div>

            <v-list v-else lines="three" bg-color="transparent">
              <v-list-item v-for="(item, idx) in form.items" :key="`item-${idx}`" class="list-item">
                <v-row>
                  <v-col cols="12" md="8">
                    <v-text-field
                      v-model="item.name"
                      label="Name"
                      variant="outlined"
                      density="compact"
                      :rules="[rules.required]"
                    ></v-text-field>
                    <div class="d-flex">
                      <v-select
                        v-model="item.type"
                        label="Type"
                        clearable
                        :items="serviceItemTypeOptions"
                        item-title="label"
                        item-value="value"
                        variant="outlined"
                        density="compact"
                        max-width="200px"
                        class="mr-2"
                        :rules="[rules.required]"
                      ></v-select>
                      <v-text-field
                        v-model="item.partNumber"
                        clearable
                        label="Part Number"
                        variant="outlined"
                        density="compact"
                        :rules="[rules.textFieldCounter]"
                      ></v-text-field>
                    </div>
                  </v-col>
                  <v-col cols="12" md="4">
                    <div class="service-item-pricing">
                      <v-number-input
                        v-model="item.quantity"
                        label="Quantity"
                        variant="outlined"
                        density="compact"
                        control-variant="stacked"
                        :min="1"
                        :step="1"
                        class="mr-2"
                        :rules="[rules.required, rules.greaterThanZero]"
                      ></v-number-input>
                      <v-number-input
                        v-model="item.unitPrice"
                        label="Price per unit"
                        variant="outlined"
                        density="compact"
                        control-variant="hidden"
                        :min="0"
                        :step="0.01"
                        :rules="[rules.required, rules.greaterOrEqualToZero]"
                      ></v-number-input>
                    </div>
                  </v-col>
                </v-row>

                <v-list-item-action class="d-flex justify-space-between">
                  <v-spacer></v-spacer>
                  <v-btn color="error" prepend-icon="mdi-delete" variant="text" @click.prevent="removeItem(idx)">Remove</v-btn>
                </v-list-item-action>
              </v-list-item>
            </v-list>
          </v-card-text>

          <v-card-actions>
            <v-spacer></v-spacer>
            <v-btn color="primary" variant="tonal" :disabled="submitting" :loading="submitting" @click.prevent="submit">Create</v-btn>
          </v-card-actions>
        </v-form>
      </v-card>
    </v-col>
  </v-row>
</template>

<style scoped>
.card-background {
  background-color: rgba(var(--v-theme-primary), 0.08) !important;
}

.vehicle-info {
  font-weight: 500;
  font-size: 18px;
}

.list-item {
  background-color: rgba(var(--v-theme-primary), 0.08) !important;
  margin-bottom: 2px !important;
  padding-top: 0px !important;
  border-radius: 2px !important;
}

.list-item:first-child {
  border-top-left-radius: 12px !important;
  border-top-right-radius: 12px !important;
}

.list-item:last-child {
  border-bottom-left-radius: 12px !important;
  border-bottom-right-radius: 12px !important;
  margin-bottom: 0 !important;
}

.list-item :deep(.v-list-item__content) {
  padding-top: 16px !important;
}

.service-item-pricing {
  display: flex;
  flex-direction: row;
  gap: 16px;
  justify-content: flex-end;
}
</style>
