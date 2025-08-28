<template>
  <v-container fluid class="pa-6">
    <div class="mb-6">
      <h1 class="text-h4 mb-2">V-Card Gallery</h1>
      <p class="text-body-1 text-medium-emphasis">Różne warianty v-card z Material Design 3 container colors</p>
    </div>

    <!-- Card Variants -->
    <div class="mb-8">
      <h2 class="text-h5 mb-4">Card Variants</h2>
      <v-row dense>
        <!-- elevated -->
        <v-col cols="12" md="4" lg="3">
          <v-card variant="elevated" class="mx-auto" max-width="320" title="Card Title" subtitle="This is a subtitle for the card">
            <v-card-text> This is the main content of the card. It can contain any text, images, or other components. </v-card-text>

            <template v-slot:actions>
              <v-btn color="primary">Action</v-btn>
              <v-btn color="surface-variant" variant="text">Cancel</v-btn>
            </template>
          </v-card>

          <div class="text-center text-caption mt-2 font-weight-medium">elevated</div>
        </v-col>

        <!-- flat -->
        <v-col cols="12" md="4" lg="3">
          <v-card variant="flat" class="mx-auto" max-width="320" title="Card Title" subtitle="This is a subtitle for the card">
            <v-card-text> This is the main content of the card. It can contain any text, images, or other components. </v-card-text>

            <template v-slot:actions>
              <v-btn color="primary" variant="text">Action</v-btn>
              <v-btn color="surface-variant" variant="text">Cancel</v-btn>
            </template>
          </v-card>

          <div class="text-center text-caption mt-2 font-weight-medium">flat</div>
        </v-col>

        <!-- tonal -->
        <v-col cols="12" md="4" lg="3">
          <v-card variant="tonal" class="mx-auto" max-width="320" title="Card Title" subtitle="This is a subtitle for the card">
            <v-card-text> This is the main content of the card. It can contain any text, images, or other components. </v-card-text>

            <template v-slot:actions>
              <v-btn color="primary" variant="text">Action</v-btn>
              <v-btn color="surface-variant" variant="text">Cancel</v-btn>
            </template>
          </v-card>

          <div class="text-center text-caption mt-2 font-weight-medium">tonal</div>
        </v-col>

        <!-- outlined -->
        <v-col cols="12" md="4" lg="3">
          <v-card variant="outlined" class="mx-auto" max-width="320" title="Card Title" subtitle="This is a subtitle for the card">
            <v-card-text> This is the main content of the card. It can contain any text, images, or other components. </v-card-text>

            <template v-slot:actions>
              <v-btn color="primary" variant="text">Action</v-btn>
              <v-btn color="surface-variant" variant="text">Cancel</v-btn>
            </template>
          </v-card>

          <div class="text-center text-caption mt-2 font-weight-medium">outlined</div>
        </v-col>

        <!-- text -->
        <v-col cols="12" md="4" lg="3">
          <v-card variant="text" class="mx-auto" max-width="320" title="Card Title" subtitle="This is a subtitle for the card">
            <v-card-text> This is the main content of the card. It can contain any text, images, or other components. </v-card-text>

            <template v-slot:actions>
              <v-btn color="primary" variant="text">Action</v-btn>
              <v-btn color="surface-variant" variant="text">Cancel</v-btn>
            </template>
          </v-card>

          <div class="text-center text-caption mt-2 font-weight-medium">text</div>
        </v-col>

        <!-- plain -->
        <v-col cols="12" md="4" lg="3">
          <v-card variant="plain" class="mx-auto" max-width="320" title="Card Title" subtitle="This is a subtitle for the card">
            <v-card-text> This is the main content of the card. It can contain any text, images, or other components. </v-card-text>

            <template v-slot:actions>
              <v-btn color="primary" variant="text">Action</v-btn>
              <v-btn color="surface-variant" variant="text">Cancel</v-btn>
            </template>
          </v-card>

          <div class="text-center text-caption mt-2 font-weight-medium">plain</div>
        </v-col>
      </v-row>
    </div>

    <!-- Container Colors -->
    <div class="mb-8">
      <h2 class="text-h5 mb-4">Container Colors (Flat Variant)</h2>
      <v-row dense>
        <!-- primary-container -->
        <v-col cols="12" md="4" lg="3">
          <v-card
            variant="flat"
            class="mx-auto"
            color="primary-container"
            max-width="320"
            title="Container Card"
            subtitle="Using container color"
          >
            <v-card-text>
              This card uses Primary Container as the background color. Notice how the text automatically adapts for readability.
            </v-card-text>

            <template v-slot:actions>
              <v-btn color="primary" variant="text">Primary</v-btn>
              <v-btn variant="text">Secondary</v-btn>
            </template>
          </v-card>

          <div class="text-center text-caption mt-2 font-weight-medium">Primary Container</div>
        </v-col>

        <!-- secondary-container -->
        <v-col cols="12" md="4" lg="3">
          <v-card
            variant="flat"
            class="mx-auto"
            color="secondary-container"
            max-width="320"
            title="Container Card"
            subtitle="Using container color"
          >
            <v-card-text>
              This card uses Secondary Container as the background color. Notice how the text automatically adapts for readability.
            </v-card-text>

            <template v-slot:actions>
              <v-btn color="primary" variant="text">Primary</v-btn>
              <v-btn variant="text">Secondary</v-btn>
            </template>
          </v-card>

          <div class="text-center text-caption mt-2 font-weight-medium">Secondary Container</div>
        </v-col>

        <!-- tertiary-container -->
        <v-col cols="12" md="4" lg="3">
          <v-card
            variant="flat"
            class="mx-auto"
            color="tertiary-container"
            max-width="320"
            title="Container Card"
            subtitle="Using container color"
          >
            <v-card-text>
              This card uses Tertiary Container as the background color. Notice how the text automatically adapts for readability.
            </v-card-text>

            <template v-slot:actions>
              <v-btn color="primary" variant="text">Primary</v-btn>
              <v-btn variant="text">Secondary</v-btn>
            </template>
          </v-card>

          <div class="text-center text-caption mt-2 font-weight-medium">Tertiary Container</div>
        </v-col>

        <!-- surface-container -->
        <v-col cols="12" md="4" lg="3">
          <v-card
            variant="flat"
            class="mx-auto"
            color="surface-container"
            max-width="320"
            title="Container Card"
            subtitle="Using container color"
          >
            <v-card-text>
              This card uses Surface Container as the background color. Notice how the text automatically adapts for readability.
            </v-card-text>

            <template v-slot:actions>
              <v-btn color="on-error" variant="flat">Primary</v-btn>
              <v-btn color="secondary" variant="flat">Secondary</v-btn>
              <v-btn color="secondary" variant="flat">Secondary</v-btn>
            </template>
          </v-card>

          <div class="text-center text-caption mt-2 font-weight-medium">Surface Container</div>
        </v-col>
      </v-row>
    </div>

    <!-- Mixed Examples -->
    <div class="mb-8">
      <h2 class="text-h5 mb-4">Mixed Examples</h2>
      <v-row dense>
        <!-- flat + primary-container -->
        <v-col cols="12" md="4">
          <v-card
            variant="flat"
            color="primary-container"
            class="mx-auto"
            max-width="320"
            title="Primary Action"
            subtitle="Important content"
          >
            <v-card-text> This card uses primary container color to indicate high importance or primary actions. </v-card-text>

            <template v-slot:actions>
              <v-btn color="primary" variant="text"> Confirm </v-btn>
              <v-btn variant="text">Cancel</v-btn>
            </template>
          </v-card>

          <div class="text-center text-caption mt-2 font-weight-medium">flat + primary-container</div>
        </v-col>

        <!-- outlined + secondary-container -->
        <v-col cols="12" md="4">
          <v-card
            variant="outlined"
            color="secondary-container"
            class="mx-auto"
            max-width="320"
            title="Secondary Info"
            subtitle="Supporting content"
          >
            <v-card-text>
              Secondary container with outlined variant creates subtle emphasis while maintaining clear boundaries.
            </v-card-text>

            <template v-slot:actions>
              <v-btn color="secondary" variant="text"> Learn More </v-btn>
              <v-btn variant="text">Skip</v-btn>
            </template>
          </v-card>

          <div class="text-center text-caption mt-2 font-weight-medium">outlined + secondary-container</div>
        </v-col>

        <!-- tonal + tertiary-container -->
        <v-col cols="12" md="4">
          <v-card
            variant="tonal"
            color="tertiary-container"
            class="mx-auto"
            max-width="320"
            title="Tertiary Option"
            subtitle="Alternative choice"
          >
            <v-card-text>
              Tertiary container with tonal variant provides a gentle background for alternative or optional content.
            </v-card-text>

            <template v-slot:actions>
              <v-btn color="tertiary" variant="text"> Try It </v-btn>
              <v-btn variant="text">Maybe Later</v-btn>
            </template>
          </v-card>

          <div class="text-center text-caption mt-2 font-weight-medium">tonal + tertiary-container</div>
        </v-col>
      </v-row>
    </div>
  </v-container>
</template>

<script setup lang="ts">
// Wszystkie karty są teraz bezpośrednio w template - bez list definicji
</script>
