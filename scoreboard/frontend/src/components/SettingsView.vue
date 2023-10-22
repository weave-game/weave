<script setup lang="ts">
import { ref } from "vue";
import axios from "axios";

const filePath = ref("");
const newFilePath = ref("");
const showSettings = ref(false);

axios
  .get("http://localhost:3000/settings/file-path")
  .then((response) => {
    filePath.value = response.data.filePath;
  })
  .catch((error) => {
    console.log(error);
  });

function updateFilePath() {
  axios
    .put("http://localhost:3000/settings/file-path", {
      filePath: newFilePath.value,
    })
    .then((response) => {
      filePath.value = response.data.filePath;
      newFilePath.value = "";
    })
    .catch((error) => {
      console.log(error);
    });
}

function clearHiddenScores() {
  localStorage.removeItem("hiddenScores");
  window.location.reload();
}

</script>

<template>
  <div class="my-32">
    <button @click="showSettings = !showSettings" class="text-white my-mono">
      {{ showSettings ? "Hide" : "Show" }} settings
    </button>

    <div v-show="showSettings" class="mt-5">
      <!-- <h2 class="text-white my-mono font-bold">Settings</h2> -->
      <p class="text-white my-mono">Current file path: {{ filePath }}</p>

      <br />

      <form @submit.prevent="updateFilePath">
        <label for="file-path" class="text-white my-mono">Enter file path:</label>
        <input type="text" id="file-path" v-model="newFilePath" class="my-mono mx-3" />
        <button type="submit" class="text-white my-mono">Save</button>
      </form>

      <hr class="my-4">

      <form @submit.prevent="clearHiddenScores">
        <button class="text-white">Unhide all scores</button>
      </form>
    </div>
  </div>
</template>

<style scoped></style>
