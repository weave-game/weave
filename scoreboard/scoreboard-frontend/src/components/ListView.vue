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
const crownSrc = 'https://static.vecteezy.com/system/resources/thumbnails/010/331/776/small/3d-rendering-gold-crown-with-three-blue-diamonds-isolated-png.png';
const lSrc = 'https://media.tenor.com/FnFH6kxGLbUAAAAM/red-alphabet-letter-dancing-letter-l.gif'

// Fetch scores from the server
const fetchScores = async () => {
  try {
    const response = await fetch('http://localhost:3000/scores')

    if (!response.ok) {
      throw new Error('Failed to fetch scores')
    }

    const data: ScoresDTO = await response.json()
    scores.value = data.scores.sort((a, b) => b.score - a.score)
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

  <table class="text-white w-full text-3xl font-black my-32 border-solid border-2 border-neutral-800">
    <thead>
      <tr>
        <th></th>
        <th class="text-left">Name</th>
        <th class="text-left">Score</th>
        <!-- <th></th> -->
      </tr>
    </thead>

    <tbody>
      <tr v-for="(score, index) in scores" :key="score.name" class="pt-32" style="height: 80px"
        :class="{ 'bg-neutral-800': index % 2 === 0, 'bg-neutral-900': index % 2 === 1 }">
        <td class="pl-8">{{ index + 1 }}.</td>
        <td>{{ score.name }}</td>
        <td>
          <div class="flex items-center">
            {{ score.score }}

            <img v-if="index === 0" :src="crownSrc" alt="crown" class="crown ml-7">

            <img v-if="score.name.toLowerCase() === 'monkey'" :src="lSrc" alt="crown" class="crown ml-7">
          </div>
        </td>
      </tr>
    </tbody>
  </table>
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
