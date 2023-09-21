<script setup lang="ts">
import { ref } from "vue";
import axios from "axios";

const filePath = ref("");
const newFilePath = ref("");

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
</script>

<template>
  <div class=" mt-32">
    <h2 class="text-white my-mono font-bold">Settings</h2>
    <p class="text-white my-mono ">Current file path: {{ filePath }}</p>

    <br>

    <form @submit.prevent="updateFilePath">
      <label for="file-path" class="text-white my-mono">Enter file path:</label>
      <input type="text" id="file-path" v-model="newFilePath" class="my-mono mx-3" />
      <button type="submit" class="text-white my-mono">Save</button>
    </form>
  </div>
</template>

<style scoped></style>
