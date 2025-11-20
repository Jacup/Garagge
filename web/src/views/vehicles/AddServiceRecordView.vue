<script setup lang="ts">
import { computed, ref, reactive, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import type { Ref } from 'vue'
import { ServiceItemType } from '../../types/serviceItemType'

import type { ServiceTypeDto, ServiceRecordCreateRequest } from '@/api/generated/apiV1.schemas'
import { getServiceRecords } from '@/api/generated/service-records/service-records'

const { getApiVehiclesServiceRecordsTypes, postApiVehiclesVehicleIdServiceRecords } = getServiceRecords()

const route = useRoute()

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
const formRef: Ref<any> = ref(null)

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

// Validation rules matching EF constraints
const titleRules = [
  (v: string) => !!v || 'Title is required',
  (v: string) => (v ? v.length <= 64 : true) || 'Title must be at most 64 characters',
]

const notesRules = [(v: string) => (v ? v.length <= 500 : true) || 'Notes must be at most 500 characters']

const dateRules = [(v: string) => !!v || 'Date is required']

const manualCostRules = [(v: number) => v === null || v === undefined || v >= 0 || 'Manual cost must be >= 0']

// ServiceItem rules
const itemNameRules = [
  (v: string) => !!v || 'Name is required',
  (v: string) => (v ? v.length <= 64 : true) || 'Name must be at most 64 characters',
]

const itemTypeRules = [(v: any) => (v !== null && v !== undefined) || 'Type is required']

const itemQuantityRules = [
  (v: number) => (v !== null && v !== undefined) || 'Quantity is required',
  (v: number) => v === null || v === undefined || v >= 0 || 'Quantity must be >= 0',
]

const itemUnitPriceRules = [
  (v: number) => (v !== null && v !== undefined) || 'Unit price is required',
  (v: number) => v === null || v === undefined || v >= 0 || 'Unit price must be >= 0',
]

const partNumberRules = [(v: string) => (v ? v.length <= 64 : true) || 'Part number must be at most 64 characters']

const itemNotesRules = [(v: string) => (v ? v.length <= 500 : true) || 'Item notes must be at most 500 characters']

// Options for service item type (for now derived from enum; later will come from an endpoint)
const serviceItemTypeOptions = Object.keys(ServiceItemType)
  .filter((k) => isNaN(Number(k)))
  .map((k) => ({ label: k, value: (ServiceItemType as any)[k] }))

function addItem() {
  form.items.push({ name: '', type: null, partNumber: '', quantity: null, unitPrice: null, notes: '' })
}

function removeItem(index: number) {
  if (index >= 0 && index < form.items.length) form.items.splice(index, 1)
}

function submit() {
  const valid = formRef.value?.validate ? formRef.value.validate() : true
  if (!valid) return

  // Placeholder: in next step we'll build the ServiceRecordCreateRequest and send it to the API
  // For now just log the validated payload
  // Convert items to trimmed values
  const payload = {
    ...form,
    items: form.items.map((i) => ({ ...i })),
  }
  // eslint-disable-next-line no-console
  console.log('Form valid, payload:', payload)
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
          <!-- service record data -->
          <v-card-text>
            <v-row>
              <v-col cols="12" md="6">
                <v-text-field v-model="form.title" clearable label="Title" variant="outlined" :rules="titleRules"></v-text-field>
                <v-select
                  v-model="form.recordType"
                  clearable
                  label="Type"
                  :items="serviceTypes"
                  item-title="name"
                  item-value="id"
                  variant="outlined"
                ></v-select>
                <v-textarea v-model="form.notes" clearable label="Notes" variant="outlined" :rules="notesRules"></v-textarea>
              </v-col>
              <v-col cols="12" md="6">
                <v-text-field
                  v-model="form.serviceDate"
                  label="Date"
                  type="date"
                  variant="outlined"
                  density="comfortable"
                  :rules="dateRules"
                  class="form-field"
                />
                <v-number-input v-model="form.mileage" label="Mileage" clearable variant="outlined" :min="0" :step="100"></v-number-input>
                <v-number-input
                  v-model="form.manualCost"
                  label="Manual cost"
                  clearable
                  variant="outlined"
                  :step="0.01"
                  :rules="manualCostRules"
                ></v-number-input>
              </v-col>
            </v-row>
          </v-card-text>

          <!-- service items -->
          <v-card-text>
            <div class="service-items-header d-flex justify-space-between">
              <div class="text-h6">Service Items</div>
              <v-spacer></v-spacer>
              <v-btn color="primary" prepend-icon="mdi-plus" variant="text" @click.prevent="addItem">Add item</v-btn>
            </div>
            <v-divider class="my-4"></v-divider>

            <v-list bg-color="transparent">
              <v-list-item v-for="(item, idx) in form.items" :key="idx">
                <v-row class="w-100">
                  <v-col cols="12" md="8">
                    <v-text-field
                      v-model="item.name"
                      label="name"
                      variant="outlined"
                      density="compact"
                      :rules="itemNameRules"
                    ></v-text-field>
                    <div class="d-flex">
                      <v-select
                        v-model="item.type"
                        label="type"
                        :items="serviceItemTypeOptions"
                        item-title="label"
                        item-value="value"
                        variant="outlined"
                        density="compact"
                        max-width="200px"
                        class="mr-2"
                        :rules="itemTypeRules"
                      ></v-select>
                      <v-text-field
                        v-model="item.partNumber"
                        clearable
                        label="partNumber"
                        variant="outlined"
                        density="compact"
                        :rules="partNumberRules"
                      ></v-text-field>
                    </div>
                  </v-col>
                  <v-col cols="12" md="4">
                    <div class="service-item-pricing">
                      <v-number-input
                        v-model="item.quantity"
                        label="quantity"
                        variant="outlined"
                        density="compact"
                        control-variant="stacked"
                        :min="0"
                        :step="0.001"
                        class="mr-2"
                        :rules="itemQuantityRules"
                      ></v-number-input>
                      <v-number-input
                        v-model="item.unitPrice"
                        label="unitPrice"
                        variant="outlined"
                        density="compact"
                        control-variant="hidden"
                        :min="0"
                        :step="0.01"
                        :rules="itemUnitPriceRules"
                      ></v-number-input>
                    </div>
                  </v-col>
                </v-row>

                <v-list-item-action class="d-flex justify-space-between">
                  <v-spacer></v-spacer>
                  <v-btn color="error" prepend-icon="mdi-delete" variant="text" @click.prevent="removeItem(idx)">Remove</v-btn>
                </v-list-item-action>
                <v-divider class="my-4"></v-divider>
              </v-list-item>
            </v-list>
          </v-card-text>

          <v-card-actions>
            <v-spacer></v-spacer>
            <v-btn color="primary" variant="tonal" @click.prevent="submit">Create</v-btn>
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

.service-item-pricing {
  display: flex;
  flex-direction: row;
  gap: 16px;
  justify-content: flex-end;
}

.segment1 {
  flex-grow: 3;
}

.segment2 {
  flex-grow: auto;
}

.total-price {
  display: flex;
  justify-content: space-between;
  font-weight: 500;
  font-size: 16px;
  margin-top: 16px;
}

@media (max-width: 960px) {
  .service-item-layout {
    flex-direction: column;
  }
}
</style>
