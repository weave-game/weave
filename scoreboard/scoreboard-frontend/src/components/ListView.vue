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
  <div class="text-neutral-500 font-bold">
    <p>Fetching scores in: {{ countdown }} seconds</p>
  </div>

  <div v-for="(score, index) in scores" :key="score.name" class="mt-16">
    <div class="flex flex-row">
      <div>
        <h2 class="text-7xl font-black text-white">{{ index + 1 }}. {{ score.name }}</h2>
        <h3 class="text-5xl my-mono font-bold text-neutral-500">{{ score.score }}</h3>
      </div>

      <img v-if="index === 0"
        src="https://static.vecteezy.com/system/resources/thumbnails/010/331/776/small/3d-rendering-gold-crown-with-three-blue-diamonds-isolated-png.png"
        alt="crown" class="crown ml-7">
    </div>
  </div>
</template>

<style scoped>
@keyframes float {
  0% {
    transform: translatey(0px) rotate(0deg);
  }

  50% {
    transform: translatey(-20px) rotate(-10deg);
  }

  100% {
    transform: translatey(0px) rotate(0deg);
  }
}

.crown {
  transform: translatey(0px);
  animation: float 6s ease-in-out infinite;
  width: 80px;
  height: 80px;
}
</style>
