<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'

type Score = {
  id: string;
  name: string
  points: number
}

type ScoresDTO = {
  scores: Score[],
  timestamp: string,
  error: {
    message: string
  }
}

const scores = ref([] as Score[])
const fetchIntervalSeconds = 5
const scoresToDisplay = 10
const countdown = ref(fetchIntervalSeconds)
const hiddenScores = ref<string[]>(JSON.parse(localStorage.getItem('hiddenScores') || '[]'))

// Fetch scores from the server
const fetchScores = async () => {
  try {
    const response = await fetch('http://localhost:3000/scores')

    if (!response.ok) {
      throw new Error('Failed to fetch scores')
    }

    const data: ScoresDTO = await response.json()
    scores.value = filter(data.scores);
  } catch (error) {
    console.error('Error fetching scores:', error)
  }
}

const toggleScoreVisibility = (name: string) => {
  if (window.confirm('Are you sure you want to hide this score?')) {
    hiddenScores.value.push(name)
    localStorage.setItem('hiddenScores', JSON.stringify(hiddenScores.value))
  }
}

const filter = (scores: Score[]) => {
  const visibleScores = scores.filter(score => !hiddenScores.value.includes(score.id))
  const sorted = visibleScores.sort((a, b) => b.points - a.points).slice(0, scoresToDisplay)

  if (scores.length > scoresToDisplay) {
    const fakeScore: Score = {
      name: '...',
    } as Score

    sorted.splice(sorted.length - 1, 0, fakeScore)
  }

  return sorted;
}

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
  <div class="text-neutral-700 font-bold">
    <p class="my-mono">Update in {{ countdown }}...</p>
  </div>

  <table class="text-white w-full text-3xl font-black my-16 border-solid border-2 border-neutral-800">
    <tbody>
      <tr v-for="(score, index) in scores" :key="score.name" class="pt-32" style="height: 80px"
        :class="{ 'bg-neutral-800': index % 2 === 0, 'bg-neutral-900': index % 2 === 1 }">
        <td class="pl-8">{{ index + 1 }}.</td>
        <td>{{ score.name }}</td>
        <td>
          <div class="flex items-center">
            <span class="my-mono">
              {{ score.points.toLocaleString('sv-SE') }}
            </span>

            <img v-if="index === 0" src="../assets/img/crown.png" alt="crown" class="crown ml-7">

            <img v-if="index + 1 === scores.length" src="../assets/img/l.gif" alt="crown" class="crown ml-7">

            <button @click="toggleScoreVisibility(score.id)" class="ml-3">--- Hide</button>
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
