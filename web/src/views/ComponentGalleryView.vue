<script lang="ts" setup>
import { ref } from 'vue'
import { useAppTheme, type ThemeVariant } from '@/composables/useTheme'
import SectionCard from '@/components/gallery/SectionCard.vue'
import ComponentDemoCard from '@/components/gallery/ComponentDemoCard.vue'

// Theme management
const { currentTheme, isDark, isHighContrast, setTheme, availableThemes } = useAppTheme()

// Component demo states
const textInput = ref('Sample text')
const password = ref('password123')
const email = ref('user@example.com')
const textarea = ref('This is a multi-line text area example.')
const selectedRadio = ref('option1')
const selectedCheckbox = ref(['option1', 'option3'])
const switchValue = ref(true)
const sliderValue = ref(30)
const selectValue = ref('option2')
const comboboxValue = ref('Vue.js')
const rating = ref(3.5)

const menuItems = [
  { title: 'Dashboard', icon: 'mdi-view-dashboard', value: 'dashboard' },
  { title: 'Profile', icon: 'mdi-account', value: 'profile' },
  { title: 'Settings', icon: 'mdi-cog', value: 'settings' },
]

const tableHeaders = [
  { title: 'Name', key: 'name' },
  { title: 'Role', key: 'role' },
  { title: 'Status', key: 'status' },
  { title: 'Actions', key: 'actions', sortable: false }
]

const tableData = [
  { name: 'John Doe', role: 'Admin', status: 'Active' },
  { name: 'Jane Smith', role: 'User', status: 'Inactive' },
  { name: 'Bob Johnson', role: 'Editor', status: 'Active' }
]

const breadcrumbs = [
  { title: 'Home', href: '/' },
  { title: 'Components', href: '/components' },
  { title: 'Gallery', href: '/gallery' }
]

const expansionItems = [
  { title: 'Expansion Panel 1', text: 'Content for panel 1' },
  { title: 'Expansion Panel 2', text: 'Content for panel 2' },
  { title: 'Expansion Panel 3', text: 'Content for panel 3' }
]
</script>

<template>
  <div class="component-gallery">
    <!-- Theme Selector Header -->
    <v-app-bar color="surface-container" class="mb-4">
      <v-app-bar-title>🎨 Vuetify Components Gallery</v-app-bar-title>
      <v-spacer />

      <v-select
        v-model="currentTheme"
        :items="availableThemes"
        label="Theme"
        variant="outlined"
        density="compact"
        style="max-width: 200px"
        @update:model-value="setTheme($event as ThemeVariant)"
      />

      <v-chip
        :color="isDark ? 'primary' : 'secondary'"
        class="ml-2"
      >
        {{ isDark ? '🌙 Dark' : '☀️ Light' }}
        {{ isHighContrast ? ' (HC)' : '' }}
      </v-chip>
    </v-app-bar>

    <v-container fluid>
      <!-- Buttons Section -->
      <section-card title="🔘 Buttons">
        <v-row>
          <!-- Button Variants -->
          <v-col cols="12" md="6">
            <component-demo-card title="Button Variants">
              <div class="d-flex flex-wrap ga-2 mb-4">
                <v-btn>Default</v-btn>
                <v-btn variant="elevated">Elevated</v-btn>
                <v-btn variant="tonal">Tonal</v-btn>
                <v-btn variant="outlined">Outlined</v-btn>
                <v-btn variant="text">Text</v-btn>
                <v-btn variant="plain">Plain</v-btn>
              </div>

              <div class="d-flex flex-wrap ga-2 mb-4">
                <v-btn color="primary">Primary</v-btn>
                <v-btn color="secondary">Secondary</v-btn>
                <v-btn color="success">Success</v-btn>
                <v-btn color="warning">Warning</v-btn>
                <v-btn color="error">Error</v-btn>
              </div>

              <div class="d-flex flex-wrap ga-2 mb-4">
                <v-btn size="x-small">X-Small</v-btn>
                <v-btn size="small">Small</v-btn>
                <v-btn size="default">Default</v-btn>
                <v-btn size="large">Large</v-btn>
                <v-btn size="x-large">X-Large</v-btn>
              </div>

              <div class="d-flex flex-wrap ga-2">
                <v-btn disabled>Disabled</v-btn>
                <v-btn loading>Loading</v-btn>
                <v-btn prepend-icon="mdi-heart">With Icon</v-btn>
                <v-btn append-icon="mdi-arrow-right">Arrow</v-btn>
              </div>
            </component-demo-card>
          </v-col>

          <!-- Icon Buttons -->
          <v-col cols="12" md="6">
            <component-demo-card title="Icon Buttons">
              <div class="d-flex flex-wrap ga-2 mb-4">
                <v-btn icon="mdi-heart" />
                <v-btn icon="mdi-star" variant="tonal" />
                <v-btn icon="mdi-share" variant="outlined" />
                <v-btn icon="mdi-delete" variant="text" />
              </div>

              <div class="d-flex flex-wrap ga-2 mb-4">
                <v-btn icon="mdi-heart" color="error" />
                <v-btn icon="mdi-star" color="warning" />
                <v-btn icon="mdi-thumb-up" color="success" />
                <v-btn icon="mdi-bookmark" color="primary" />
              </div>

              <div class="d-flex flex-wrap ga-2">
                <v-btn icon="mdi-menu" size="x-small" />
                <v-btn icon="mdi-menu" size="small" />
                <v-btn icon="mdi-menu" />
                <v-btn icon="mdi-menu" size="large" />
                <v-btn icon="mdi-menu" size="x-large" />
              </div>
            </component-demo-card>
          </v-col>
        </v-row>
      </section-card>

      <!-- Form Controls Section -->
      <section-card title="📝 Form Controls">
        <v-row>
          <!-- Text Inputs -->
          <v-col cols="12" md="6">
            <component-demo-card title="Text Fields">
              <v-text-field
                v-model="textInput"
                label="Standard"
                class="mb-3"
              />
              <v-text-field
                v-model="textInput"
                label="Outlined"
                variant="outlined"
                class="mb-3"
              />
              <v-text-field
                v-model="textInput"
                label="Filled"
                variant="filled"
                class="mb-3"
              />
              <v-text-field
                v-model="textInput"
                label="Solo"
                variant="solo"
                class="mb-3"
              />
              <v-text-field
                v-model="password"
                label="Password"
                type="password"
                variant="outlined"
                class="mb-3"
              />
              <v-text-field
                v-model="email"
                label="Email"
                type="email"
                variant="outlined"
                prepend-inner-icon="mdi-email"
                class="mb-3"
              />
              <v-textarea
                v-model="textarea"
                label="Textarea"
                variant="outlined"
                rows="3"
              />
            </component-demo-card>
          </v-col>

          <!-- Selection Controls -->
          <v-col cols="12" md="6">
            <component-demo-card title="Selection Controls">
              <!-- Radio Buttons -->
              <div class="mb-4">
                <h4 class="mb-2">Radio Buttons</h4>
                <v-radio-group v-model="selectedRadio" inline>
                  <v-radio label="Option 1" value="option1" />
                  <v-radio label="Option 2" value="option2" />
                  <v-radio label="Option 3" value="option3" />
                </v-radio-group>
              </div>

              <!-- Checkboxes -->
              <div class="mb-4">
                <h4 class="mb-2">Checkboxes</h4>
                <v-checkbox
                  v-model="selectedCheckbox"
                  label="Option 1"
                  value="option1"
                />
                <v-checkbox
                  v-model="selectedCheckbox"
                  label="Option 2"
                  value="option2"
                />
                <v-checkbox
                  v-model="selectedCheckbox"
                  label="Option 3"
                  value="option3"
                />
              </div>

              <!-- Switches -->
              <div class="mb-4">
                <h4 class="mb-2">Switches</h4>
                <v-switch v-model="switchValue" label="Default Switch" />
                <v-switch v-model="switchValue" label="Colored Switch" color="success" />
                <v-switch v-model="switchValue" label="Inset Switch" inset />
              </div>

              <!-- Slider -->
              <div class="mb-4">
                <h4 class="mb-2">Slider</h4>
                <v-slider
                  v-model="sliderValue"
                  :min="0"
                  :max="100"
                  step="1"
                  thumb-label
                />
              </div>

              <!-- Rating -->
              <div>
                <h4 class="mb-2">Rating</h4>
                <v-rating v-model="rating" half-increments />
              </div>
            </component-demo-card>
          </v-col>
        </v-row>
      </section-card>

      <!-- Selection & Navigation -->
      <section-card title="🧭 Selection & Navigation">
        <v-row>
          <!-- Selects -->
          <v-col cols="12" md="6">
            <component-demo-card title="Selects & Combobox">
              <v-select
                v-model="selectValue"
                :items="['option1', 'option2', 'option3', 'option4']"
                label="Select"
                variant="outlined"
                class="mb-3"
              />

              <v-combobox
                v-model="comboboxValue"
                :items="['Vue.js', 'React', 'Angular', 'Svelte']"
                label="Combobox"
                variant="outlined"
                class="mb-3"
              />

              <v-autocomplete
                v-model="selectValue"
                :items="['Apple', 'Banana', 'Cherry', 'Date', 'Elderberry']"
                label="Autocomplete"
                variant="outlined"
                class="mb-3"
              />

              <v-file-input
                label="File input"
                variant="outlined"
                prepend-icon="mdi-paperclip"
              />
            </component-demo-card>
          </v-col>

          <!-- Navigation -->
          <v-col cols="12" md="6">
            <component-demo-card title="Navigation">
              <div class="mb-4">
                <h4 class="mb-2">Breadcrumbs</h4>
                <v-breadcrumbs :items="breadcrumbs" />
              </div>

              <div class="mb-4">
                <h4 class="mb-2">Tabs</h4>
                <v-tabs>
                  <v-tab value="tab1">Tab 1</v-tab>
                  <v-tab value="tab2">Tab 2</v-tab>
                  <v-tab value="tab3">Tab 3</v-tab>
                </v-tabs>
              </div>

              <div class="mb-4">
                <h4 class="mb-2">Stepper</h4>
                <v-stepper :items="['Step 1', 'Step 2', 'Step 3']" :model-value="2" />
              </div>

              <div>
                <h4 class="mb-2">Bottom Navigation</h4>
                <v-bottom-navigation>
                  <v-btn value="dashboard">
                    <v-icon>mdi-view-dashboard</v-icon>
                    Dashboard
                  </v-btn>
                  <v-btn value="profile">
                    <v-icon>mdi-account</v-icon>
                    Profile
                  </v-btn>
                  <v-btn value="settings">
                    <v-icon>mdi-cog</v-icon>
                    Settings
                  </v-btn>
                </v-bottom-navigation>
              </div>
            </component-demo-card>
          </v-col>
        </v-row>
      </section-card>

      <!-- Cards & Surfaces -->
      <section-card title="📇 Cards & Surfaces">
        <v-row>
          <!-- Cards -->
          <v-col cols="12" md="4">
            <component-demo-card title="Basic Cards">
              <v-card class="mb-3">
                <v-card-title>Simple Card</v-card-title>
                <v-card-text>This is a basic card with some content.</v-card-text>
                <v-card-actions>
                  <v-btn>Action</v-btn>
                  <v-btn variant="text">Cancel</v-btn>
                </v-card-actions>
              </v-card>

              <v-card class="mb-3" variant="outlined">
                <v-card-title>Outlined Card</v-card-title>
                <v-card-text>Card with outlined variant.</v-card-text>
              </v-card>

              <v-card variant="tonal">
                <v-card-title>Tonal Card</v-card-title>
                <v-card-text>Card with tonal variant.</v-card-text>
              </v-card>
            </component-demo-card>
          </v-col>

          <!-- Complex Cards -->
          <v-col cols="12" md="4">
            <component-demo-card title="Rich Cards">
              <v-card>
                <v-img
                  src="https://cdn.vuetifyjs.com/images/cards/sunshine.jpg"
                  height="200"
                  cover
                />
                <v-card-title>Card with Image</v-card-title>
                <v-card-subtitle>Subtitle here</v-card-subtitle>
                <v-card-text>
                  Beautiful card with an image header and rich content.
                </v-card-text>
                <v-card-actions>
                  <v-btn color="primary">Learn More</v-btn>
                  <v-spacer />
                  <v-btn icon="mdi-heart-outline" />
                  <v-btn icon="mdi-share-variant" />
                </v-card-actions>
              </v-card>
            </component-demo-card>
          </v-col>

          <!-- Surfaces -->
          <v-col cols="12" md="4">
            <component-demo-card title="Surfaces & Sheets">
              <v-sheet class="pa-4 mb-3" color="surface-container">
                <h4>Sheet (surface-container)</h4>
                <p>This is a sheet with surface-container color.</p>
              </v-sheet>

              <v-sheet class="pa-4 mb-3" color="surface-container-high" rounded>
                <h4>Rounded Sheet</h4>
                <p>Sheet with high surface container color.</p>
              </v-sheet>

              <v-sheet class="pa-4" color="primary" rounded>
                <h4>Primary Sheet</h4>
                <p>Colored sheet example.</p>
              </v-sheet>
            </component-demo-card>
          </v-col>
        </v-row>
      </section-card>

      <!-- Data Display -->
      <section-card title="📊 Data Display">
        <v-row>
          <!-- Tables -->
          <v-col cols="12" md="8">
            <component-demo-card title="Data Table">
              <v-data-table
                :headers="tableHeaders"
                :items="tableData"
                class="elevation-1"
              >
                <template v-slot:[`item.status`]="{ item }">
                  <v-chip
                    :color="item.status === 'Active' ? 'success' : 'warning'"
                    size="small"
                  >
                    {{ item.status }}
                  </v-chip>
                </template>
                <template v-slot:[`item.actions`]>
                  <v-btn icon="mdi-pencil" size="small" variant="text" />
                  <v-btn icon="mdi-delete" size="small" variant="text" />
                </template>
              </v-data-table>
            </component-demo-card>
          </v-col>

          <!-- Lists -->
          <v-col cols="12" md="4">
            <component-demo-card title="Lists">
              <v-list>
                <v-list-item
                  v-for="item in menuItems"
                  :key="item.value"
                  :prepend-icon="item.icon"
                  :title="item.title"
                />
              </v-list>

              <v-divider class="my-3" />

              <v-list>
                <v-list-item
                  prepend-avatar="https://cdn.vuetifyjs.com/images/john.jpg"
                  title="John Doe"
                  subtitle="Software Engineer"
                />
                <v-list-item
                  prepend-avatar="https://cdn.vuetifyjs.com/images/lisa.jpg"
                  title="Lisa Smith"
                  subtitle="Product Manager"
                />
              </v-list>
            </component-demo-card>
          </v-col>
        </v-row>
      </section-card>

      <!-- Feedback & Overlay -->
      <section-card title="💬 Feedback & Overlays">
        <v-row>
          <!-- Alerts & Banners -->
          <v-col cols="12" md="6">
            <component-demo-card title="Alerts & Messages">
              <v-alert type="success" class="mb-3">Success alert message</v-alert>
              <v-alert type="info" class="mb-3">Information alert</v-alert>
              <v-alert type="warning" class="mb-3">Warning alert</v-alert>
              <v-alert type="error" class="mb-3">Error alert</v-alert>

              <v-banner
                lines="two"
                icon="mdi-information"
                class="mb-3"
              >
                <v-banner-text>
                  This is a banner with some important information that spans multiple lines.
                </v-banner-text>
                <template v-slot:actions>
                  <v-btn>Action</v-btn>
                </template>
              </v-banner>

              <v-snackbar v-model="switchValue" :timeout="0">
                Snackbar message here
                <template v-slot:actions>
                  <v-btn variant="text" @click="switchValue = false">Close</v-btn>
                </template>
              </v-snackbar>
            </component-demo-card>
          </v-col>

          <!-- Progress & Loading -->
          <v-col cols="12" md="6">
            <component-demo-card title="Progress & Loading">
              <div class="mb-4">
                <h4 class="mb-2">Progress Bars</h4>
                <v-progress-linear :model-value="75" class="mb-2" />
                <v-progress-linear :model-value="45" color="success" class="mb-2" />
                <v-progress-linear :model-value="60" color="warning" striped />
              </div>

              <div class="mb-4">
                <h4 class="mb-2">Progress Circular</h4>
                <div class="d-flex ga-4">
                  <v-progress-circular :model-value="75" />
                  <v-progress-circular :model-value="45" color="success" />
                  <v-progress-circular indeterminate color="primary" />
                </div>
              </div>

              <div>
                <h4 class="mb-2">Skeletons</h4>
                <v-skeleton-loader type="list-item-avatar" class="mb-2" />
                <v-skeleton-loader type="card" />
              </div>
            </component-demo-card>
          </v-col>
        </v-row>
      </section-card>

      <!-- Expansion & Layout -->
      <section-card title="🗂️ Expansion & Layout">
        <v-row>
          <!-- Expansion Panels -->
          <v-col cols="12" md="6">
            <component-demo-card title="Expansion Panels">
              <v-expansion-panels>
                <v-expansion-panel
                  v-for="item in expansionItems"
                  :key="item.title"
                  :title="item.title"
                  :text="item.text"
                />
              </v-expansion-panels>
            </component-demo-card>
          </v-col>

          <!-- Timeline -->
          <v-col cols="12" md="6">
            <component-demo-card title="Timeline">
              <v-timeline>
                <v-timeline-item
                  icon="mdi-star"
                  dot-color="success"
                >
                  <template v-slot:opposite>
                    <span class="text-caption">09:30</span>
                  </template>
                  <div>
                    <h4>Project Started</h4>
                    <p>Initial commit and setup</p>
                  </div>
                </v-timeline-item>

                <v-timeline-item
                  icon="mdi-account-multiple"
                  dot-color="primary"
                >
                  <template v-slot:opposite>
                    <span class="text-caption">10:15</span>
                  </template>
                  <div>
                    <h4>Team Meeting</h4>
                    <p>Discussed project requirements</p>
                  </div>
                </v-timeline-item>

                <v-timeline-item
                  icon="mdi-check"
                  dot-color="warning"
                >
                  <template v-slot:opposite>
                    <span class="text-caption">14:30</span>
                  </template>
                  <div>
                    <h4>First Milestone</h4>
                    <p>Completed basic setup</p>
                  </div>
                </v-timeline-item>
              </v-timeline>
            </component-demo-card>
          </v-col>
        </v-row>
      </section-card>

      <!-- Custom CSS Variables Demo -->
      <section-card title="🎨 CSS Variables Demo">
        <v-row>
          <v-col cols="12">
            <component-demo-card title="Custom Styled Elements">
              <div class="custom-elements-demo">
                <div class="custom-card">
                  <h3>Custom Card with CSS Variables</h3>
                  <p>This card uses Material Design 3 CSS variables for styling:</p>
                  <ul>
                    <li><code>background-color: rgb(var(--md-sys-color-surface-container))</code></li>
                    <li><code>color: rgb(var(--md-sys-color-on-surface))</code></li>
                    <li><code>border: 1px solid rgb(var(--md-sys-color-outline-variant))</code></li>
                  </ul>
                </div>

                <div class="custom-button-group">
                  <button class="custom-button primary">Primary Button</button>
                  <button class="custom-button secondary">Secondary Button</button>
                  <button class="custom-button surface">Surface Button</button>
                </div>
              </div>
            </component-demo-card>
          </v-col>
        </v-row>
      </section-card>
    </v-container>
  </div>
</template>

<style scoped>
.component-gallery {
  min-height: 100vh;
  background-color: rgb(var(--md-sys-color-background));
}

/* Custom CSS Variables Demo Styles */
.custom-elements-demo {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.custom-card {
  background-color: rgb(var(--md-sys-color-surface-container));
  color: rgb(var(--md-sys-color-on-surface));
  border: 1px solid rgb(var(--md-sys-color-outline-variant));
  border-radius: 12px;
  padding: 1.5rem;
}

.custom-card h3 {
  color: rgb(var(--md-sys-color-primary));
  margin-bottom: 1rem;
}

.custom-card code {
  background-color: rgb(var(--md-sys-color-surface-variant));
  color: rgb(var(--md-sys-color-on-surface-variant));
  padding: 0.25rem 0.5rem;
  border-radius: 4px;
  font-size: 0.875rem;
}

.custom-button-group {
  display: flex;
  gap: 0.5rem;
  flex-wrap: wrap;
}

.custom-button {
  padding: 0.75rem 1.5rem;
  border-radius: 8px;
  border: none;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s ease;
}

.custom-button.primary {
  background-color: rgb(var(--md-sys-color-primary));
  color: rgb(var(--md-sys-color-on-primary));
}

.custom-button.primary:hover {
  background-color: rgb(var(--md-sys-color-primary-container));
  color: rgb(var(--md-sys-color-on-primary-container));
}

.custom-button.secondary {
  background-color: rgb(var(--md-sys-color-secondary));
  color: rgb(var(--md-sys-color-on-secondary));
}

.custom-button.secondary:hover {
  background-color: rgb(var(--md-sys-color-secondary-container));
  color: rgb(var(--md-sys-color-on-secondary-container));
}

.custom-button.surface {
  background-color: rgb(var(--md-sys-color-surface-container-high));
  color: rgb(var(--md-sys-color-on-surface));
  border: 1px solid rgb(var(--md-sys-color-outline));
}

.custom-button.surface:hover {
  background-color: rgb(var(--md-sys-color-surface-container-highest));
}
</style>
