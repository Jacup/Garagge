<script>
import { ref } from 'vue'

const admins = [
  ['Management', 'mdi-account-multiple-outline'],
  ['Settings', 'mdi-cog-outline'],
]
const cruds = [
  ['Create', 'mdi-plus-outline'],
  ['Read', 'mdi-file-outline'],
  ['Update', 'mdi-update'],
  ['Delete', 'mdi-delete'],
]
</script>
<template>
  <h1>1. Basic list</h1>
  <div class="ma-4">
    <v-list rounded class="material-list">
      <v-list-item>Item 1</v-list-item>
      <v-list-item>Item 2</v-list-item>
      <v-list-item>Item 3</v-list-item>
      <v-list-item>Item 4</v-list-item>
    </v-list>
  </div>

  <h1>2. Nested expandable groups list</h1>
  <div class="ma-4">
    <v-list rounded class="material-list">
      <v-list-group value="Users">
        <template v-slot:activator="{ props }">
          <v-list-item v-bind="props" prepend-icon="mdi-account-circle" title="Users"></v-list-item>
        </template>

        <v-list-group value="Admin">
          <template v-slot:activator="{ props }">
            <v-list-item v-bind="props" title="Admin"></v-list-item>
          </template>

          <v-list-item>Admin 1</v-list-item>
          <v-list-item>Admin 2</v-list-item>
        </v-list-group>

        <v-list-group value="Actions">
          <template v-slot:activator="{ props }">
            <v-list-item v-bind="props" title="Actions"></v-list-item>
          </template>

          <v-list-item>Action 1</v-list-item>
          <v-list-item>Action 2</v-list-item>
        </v-list-group>
      </v-list-group>
    </v-list>
  </div>

  <h1>3. Group at the end of basic list</h1>
  <div class="ma-4">
    <v-list rounded class="material-list">
      <v-list-item>Item 1</v-list-item>
      <v-list-item>Item 2</v-list-item>

      <v-list-group value="Users">
        <template v-slot:activator="{ props }">
          <v-list-item v-bind="props" title="Users"></v-list-item>
        </template>

        <v-list-item>Admin 1</v-list-item>
        <v-list-item>Admin 2</v-list-item>
      </v-list-group>
    </v-list>
  </div>

  <h1>4. Group in the middle of basic list</h1>
  <div class="ma-4">
    <v-list rounded class="material-list">
      <v-list-item>Item 1</v-list-item>
      <v-list-item>Item 2</v-list-item>

      <v-list-group value="Users">
        <template v-slot:activator="{ props }">
          <v-list-item v-bind="props" title="Users"></v-list-item>
        </template>

        <v-list-item>Admin 1</v-list-item>
        <v-list-item>Admin 2</v-list-item>
      </v-list-group>
      <v-list-item>Item 3</v-list-item>
    </v-list>
  </div>
</template>
