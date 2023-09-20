<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'

type Score = {
  name: string
  score: number
}

type ScoresDTO = {
  scores: Score[],
  timestamp: string,
  error: {
    message: string
  }
}

const scores = ref([] as Score[])
const fetchIntervalSeconds = 10 // fetch every 10 seconds
const countdown = ref(fetchIntervalSeconds)

// Fetch scores from the server
const fetchScores = async () => {
  try {
    const response = await fetch('http://localhost:3000/scores')

    if (!response.ok) {
      throw new Error('Failed to fetch scores')
    }

    const data: ScoresDTO = await response.json()
    scores.value = data.scores
  } catch (error) {
    console.error('Error fetching scores:', error)
  }
}

// Fetch scores when the component is mounted and every X seconds
let fetchInterval: number
let countdownInterval: number

onMounted(() => {
  fetchScores()
  fetchInterval = setInterval(fetchScores, fetchIntervalSeconds * 1000)
  countdownInterval = setInterval(() => {
    countdown.value--
    if (countdown.value <= 0) {
      countdown.value = fetchIntervalSeconds
    }
  }, 1000)
})

onUnmounted(() => {
  clearInterval(fetchInterval)
  clearInterval(countdownInterval)
})

</script>

<template>
  <div class="card">
    <p>Fetching scores in: {{ countdown }} seconds</p>
  </div>

  <ul>
    <li v-for="score in scores" :key="score.name">
      {{ score.name }}: {{ score.score }}
    </li>
  </ul>
</template>

<style scoped></style>
