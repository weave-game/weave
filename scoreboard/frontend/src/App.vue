<script setup lang="ts">
import ListView from "./components/ListView.vue";
import SettingsView from "./components/SettingsView.vue";
</script>

<template>
  <div class="container mx-auto px-5 pt-16">
    <img
      src="./assets/scoreboard-title.svg"
      alt="scoreboard"
      class="w-full animated-rainbow-shadow"
    />

    <ListView />
    <SettingsView />
  </div>
</template>

<style scoped>
@keyframes rainbowAnimation {
  0% {
    filter: drop-shadow(0 0 10px rgba(231, 176, 255, 0.8));
  }
  20% {
    filter: drop-shadow(0 0 10px rgba(0, 225, 255, 0.8));
  }
  40% {
    filter: drop-shadow(0 0 10px rgb(0, 255, 0));
  }
  60% {
    filter: drop-shadow(0 0 10px rgb(255, 255, 0));
  }
  80% {
    filter: drop-shadow(0 0 10px rgba(255, 154, 31, 0.8));
  }
  100% {
    filter: drop-shadow(0 0 10px rgba(231, 176, 255, 0.8));
  } /* violet again to loop smoothly */
}

.animated-rainbow-shadow {
  animation: rainbowAnimation 10s ease-in-out infinite;
}
</style>
