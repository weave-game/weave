<script setup lang="ts">
import ListView from './components/ListView.vue'
import SettingsView from './components/SettingsView.vue';
</script>

<template>
  <div class="container mx-auto px-5 pt-16">
    <h1 class="text-9xl text-neutral-400 font-black uppercase catcher-font tracking-wider">Scoreboard</h1>
    <ListView />
    <SettingsView />
  </div>
</template>

<style scoped></style>
