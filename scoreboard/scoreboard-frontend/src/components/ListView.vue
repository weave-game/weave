<script setup lang="ts">
import { ref, onMounted } from 'vue'

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

const count = ref(0)
const scores = ref([] as Score[])

// Fetch scores from the server
const fetchScores = async () => {
  try {
    const response = await fetch('http://localhost:3000/scores')

    if (!response.ok) {
      throw new Error('Failed to fetch scores')
    }

    console.log('response', response)

    const data: ScoresDTO = await response.json()
    console.log('data', data)
    scores.value = data.scores
    console.log('scores', scores)
  } catch (error) {
    console.error('Error fetching scores:', error)
  }
}

// Fetch scores when the component is mounted
onMounted(fetchScores)

</script>

<template>
  <div class="card">
    <button type="button" @click="count++">{{ count }}</button>
  </div>

  <ul>
    <li v-for="score in scores" :key="score.name">
      {{ score.name }}: {{ score.score }}
    </li>
  </ul>
</template>

<style scoped></style>
