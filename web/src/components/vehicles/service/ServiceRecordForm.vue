<script setup lang="ts">
import { ref, reactive, watch, computed } from 'vue'
import { ServiceItemType } from '@/types/serviceItemType'

// --- IMPORTY TYPÓW API ---
import type {
  ServiceTypeDto,
  ServiceRecordCreateRequest,
  ServiceItemType as ApiServiceItemType,
  ServiceRecordDto,
} from '@/api/generated/apiV1.schemas'

// --- INTERFEJS KOMPONENTU ---
const props = defineProps<{
  /** Opcjonalne dane początkowe (dla trybu edycji). */
  initialData?: ServiceRecordDto | null
  /** Lista dostępnych typów serwisów. */
  serviceTypes: ServiceTypeDto[]
  /** Stan ładowania przycisku zapisu. */
  loading?: boolean
  /** Tryb pracy formularza. 'create' pokazuje sekcję itemów. 'edit-metadata' ukrywa ją. */
  mode: 'create' | 'edit-metadata'
}>()

const emit = defineEmits<{
  /** Emitowane, gdy formularz jest poprawny. Payload jest gotowy do wysłania. */
  (e: 'submit', payload: ServiceRecordCreateRequest): void
  /** Emitowane po kliknięciu Anuluj. */
  (e: 'cancel'): void
}>()

// --- STAN I WALIDACJA ---
interface ValidateResult {
  valid: boolean
}
type FormRefType = { validate: () => Promise<ValidateResult> } | null
const formRef = ref<FormRefType>(null)
const errorMessage = ref('')

// Definicja stanu itemu w formularzu
interface FormItem {
  id?: string // Opcjonalne ID (tylko w trybie edycji)
  name: string
  type: ServiceItemType | null
  partNumber: string
  quantity: number | null
  unitPrice: number | null
}

// Domyślny stan formularza
const getDefaultFormState = () => ({
  title: '',
  recordType: null as string | null,
  notes: '',
  serviceDate: new Date().toISOString().split('T')[0], // Domyślnie dzisiaj (YYYY-MM-DD)
  manualCost: null as number | null,
  mileage: null as number | null,
  items: [] as FormItem[],
})

// Reaktywny model formularza
const form = reactive(getDefaultFormState())

// --- INICJALIZACJA ---
// Wypełnia formularz danymi, jeśli initialData jest dostępne.
watch(
  () => props.initialData,
  (newData) => {
    if (newData) {
      // TRYB EDYCJI (Wypełnienie danych)
      form.title = newData.title
      form.recordType = newData.typeId
      form.notes = newData.notes || ''
      form.serviceDate = newData.serviceDate.split('T')[0]
      form.mileage = newData.mileage
      // Mapowanie itemów (tylko w trybie create, ale dla bezpieczeństwa robimy zawsze)
      form.items = newData.serviceItems.map((i) => ({
        id: i.id, // Ważne: zapamiętujemy ID
        name: i.name,
        type: i.type as unknown as ServiceItemType,
        partNumber: i.partNumber || '',
        quantity: i.quantity,
        unitPrice: i.unitPrice,
      }))
    } else {
      // TRYB TWORZENIA (Reset)
      Object.assign(form, getDefaultFormState())
      // Dodajemy jeden pusty item na start dla wygody (tylko w trybie create)
      if (props.mode === 'create') {
        addItem()
      }
    }
  },
  { immediate: true, deep: true },
)

// --- REGUŁY WALIDACJI ---
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

// --- OPERACJE NA ITEMACH ---
function addItem() {
  form.items.push({ name: '', type: null, partNumber: '', quantity: null, unitPrice: null })
}
function removeItem(index: number) {
  if (index >= 0 && index < form.items.length) form.items.splice(index, 1)
}
function removeAllItems() {
  form.items.splice(0, form.items.length)
}

// --- SUBMIT ---
async function submit() {
  const result = await formRef.value?.validate()
  if (!result?.valid) {
    errorMessage.value = 'Please correct all form errors before submitting'
    // Scroll do góry kontenera
    document.querySelector('.v-navigation-drawer__content, .v-dialog__content')?.scrollTo({ top: 0, behavior: 'smooth' })
    return
  }
  errorMessage.value = ''

  // Przygotowanie payloadu dla API (ServiceRecordCreateRequest)
  const payload: ServiceRecordCreateRequest = {
    title: form.title,
    serviceDate: form.serviceDate,
    serviceTypeId: form.recordType!,
    mileage: form.mileage,
    manualCost: form.manualCost,
    // Mapujemy itemy formularza na format API
    serviceItems: form.items.map((i) => ({
      name: i.name,
      type: i.type! as unknown as ApiServiceItemType,
      partNumber: i.partNumber || null,
      quantity: i.quantity!,
      unitPrice: i.unitPrice!,
      notes: null, // Notatki do itemów są usunięte z UI
    })),
    notes: form.notes || null,
  }

  // Emitujemy gotowy payload do rodzica
  emit('submit', payload)
}

const submitButtonLabel = computed(() => (props.mode === 'create' ? 'Create Record' : 'Save Changes'))
</script>

<template>
  <div class="h-100 d-flex flex-column bg-surface font-responsive">
    <v-form ref="formRef" @submit.prevent="submit" class="flex-grow-1 overflow-y-auto px-4 pt-4">
      <v-expand-transition>
        <div v-if="errorMessage" class="mb-4">
          <v-alert type="error" :text="errorMessage" closable @click:close="errorMessage = ''" variant="tonal" density="compact" />
        </div>
      </v-expand-transition>

      <v-row dense>
        <v-col cols="12">
          <v-text-field
            v-model="form.title"
            label="Title"
            variant="outlined"
            density="comfortable"
            :rules="[rules.required, rules.textFieldCounter]"
          ></v-text-field>
        </v-col>
        <v-col cols="12" md="6">
          <v-select
            v-model="form.recordType"
            label="Service Type"
            :items="serviceTypes"
            item-title="name"
            item-value="id"
            variant="outlined"
            density="comfortable"
            :rules="[rules.required]"
          ></v-select>
        </v-col>
        <v-col cols="12" md="6">
          <v-text-field
            v-model="form.serviceDate"
            label="Date"
            type="date"
            variant="outlined"
            density="comfortable"
            :rules="[rules.required]"
          />
        </v-col>
        <v-col cols="12" md="6">
          <v-number-input
            v-model="form.mileage"
            label="Mileage (km)"
            variant="outlined"
            density="comfortable"
            :min="0"
            :step="100"
          ></v-number-input>
        </v-col>
        <v-col cols="12" md="6">
          <v-number-input
            v-model="form.manualCost"
            label="Total Cost (Override)"
            variant="outlined"
            density="comfortable"
            :min="0"
            :step="0.01"
            hide-details="auto"
            persistent-hint
            hint="Leave empty to calc from items"
          ></v-number-input>
        </v-col>
        <v-col cols="12">
          <v-textarea
            v-model="form.notes"
            label="Global Notes"
            variant="outlined"
            density="comfortable"
            :rules="[rules.textAreaCounter]"
            rows="3"
            auto-grow
          ></v-textarea>
        </v-col>
      </v-row>

      <template v-if="mode === 'create'">
        <v-divider class="my-4"></v-divider>

        <div>
          <div class="d-flex justify-space-between align-center mb-4 px-1">
            <div class="text-subtitle-1 font-weight-medium">Service Items</div>
            <div class="d-flex gap-2 align-center">
              <v-btn color="primary" variant="text" density="comfortable" @click.prevent="addItem" class="px-2">
                <v-icon start>mdi-plus</v-icon> Add Item
              </v-btn>

              <v-tooltip v-if="form.items.length > 0" text="Remove all items" location="top">
                <template v-slot:activator="{ props }">
                  <v-btn
                    color="error"
                    icon="mdi-delete-sweep-outline"
                    variant="text"
                    density="comfortable"
                    v-bind="props"
                    @click.prevent="removeAllItems"
                  ></v-btn>
                </template>
              </v-tooltip>
            </div>
          </div>

          <div
            v-if="form.items.length < 1"
            class="text-center text-medium-emphasis py-8 border-dashed rounded-lg mb-4 bg-surface-variant-low"
          >
            <v-icon icon="mdi-playlist-plus" size="large" class="mb-2 opacity-50"></v-icon>
            <div>No items added. Click "Add Item" to start.</div>
          </div>

          <v-list v-else lines="three" bg-color="transparent" class="pa-0 item-list">
            <v-list-item v-for="(item, idx) in form.items" :key="`item-${idx}`" class="list-item mb-3 elevation-1" rounded="lg">
              <v-container class="pa-3">
                <v-row dense>
                  <v-col cols="12" sm="6">
                    <v-text-field
                      v-model="item.name"
                      label="Item Name"
                      variant="outlined"
                      density="compact"
                      :rules="[rules.required]"
                      hide-details="auto"
                      class="mb-3"
                    ></v-text-field>
                    <div class="d-flex gap-3">
                      <v-select
                        v-model="item.type"
                        label="Type"
                        :items="serviceItemTypeOptions"
                        item-title="label"
                        item-value="value"
                        variant="outlined"
                        density="compact"
                        :rules="[rules.required]"
                        hide-details="auto"
                        style="flex: 1.5"
                      ></v-select>
                      <v-text-field
                        v-model="item.partNumber"
                        label="Part No."
                        variant="outlined"
                        density="compact"
                        hide-details="auto"
                        style="flex: 1"
                      ></v-text-field>
                    </div>
                  </v-col>
                  <v-col cols="12" sm="6" class="d-flex flex-column justify-end mt-3 mt-sm-0">
                    <div class="d-flex gap-3 align-end">
                      <v-number-input
                        v-model="item.quantity"
                        label="Qty"
                        variant="outlined"
                        density="compact"
                        :min="1"
                        :step="1"
                        :rules="[rules.required]"
                        hide-details="auto"
                        style="flex: 1"
                      ></v-number-input>
                      <v-number-input
                        v-model="item.unitPrice"
                        label="Unit Price"
                        variant="outlined"
                        density="compact"
                        :min="0"
                        :step="0.01"
                        :rules="[rules.required]"
                        hide-details="auto"
                        style="flex: 2"
                      ></v-number-input>
                    </div>
                  </v-col>
                </v-row>
              </v-container>

              <template #append>
                <div class="d-flex align-center h-100 pl-2 border-s opacity-70 hover-opacity-100">
                  <v-btn
                    color="error"
                    icon="mdi-delete-outline"
                    variant="text"
                    density="comfortable"
                    @click.prevent="removeItem(idx)"
                  ></v-btn>
                </div>
              </template>
            </v-list-item>
          </v-list>
        </div>
      </template>
    </v-form>

    <v-divider></v-divider>
    <div class="pa-4 bg-surface d-flex justify-end gap-3 align-center">
      <v-btn variant="text" @click="emit('cancel')" :disabled="loading" class="px-4">Cancel</v-btn>
      <v-btn
        color="primary"
        variant="tonal"
        :loading="loading"
        @click="submit"
        class="px-6 font-weight-medium"
        prepend-icon="mdi-content-save-outline"
      >
        {{ submitButtonLabel }}
      </v-btn>
    </div>
  </div>
</template>

<style scoped>
/* Stylizacja listy itemów */
.list-item {
  background-color: rgba(var(--v-theme-surface-container-low), 0.5) !important;
  border: 1px solid rgba(var(--v-border-color), 0.08);
}

.gap-2 {
  gap: 8px;
}
.gap-3 {
  gap: 12px;
}

/* Style dla v-number-input żeby wyglądały spójnie */
:deep(.v-number-input__control) {
  height: 40px;
}
/* Subtelny separator dla przycisku usuwania */
.border-s {
  border-left: 1px solid rgba(var(--v-border-color), 0.12);
}
/* Efekt hover na przycisku usuwania */
.hover-opacity-100 {
  transition: opacity 0.2s;
}
.hover-opacity-100:hover {
  opacity: 1 !important;
}

/* Dostosowanie bordera dla stanu pustego */
.border-dashed {
  border: 2px dashed rgba(var(--v-border-color), 0.25);
}
</style>
