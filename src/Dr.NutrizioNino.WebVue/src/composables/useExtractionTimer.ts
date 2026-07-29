import { ref, computed } from 'vue'

const GREEN_THRESHOLD = 180   // secondi
const ORANGE_THRESHOLD = 300  // secondi

export function useExtractionTimer() {
  const elapsed = ref(0)
  let intervalId: ReturnType<typeof setInterval> | null = null

  const status = computed<'idle' | 'green' | 'orange' | 'red'>(() => {
    if (elapsed.value === 0) return 'idle'
    if (elapsed.value < GREEN_THRESHOLD) return 'green'
    if (elapsed.value < ORANGE_THRESHOLD) return 'orange'
    return 'red'
  })

  const formattedElapsed = computed(() => {
    const m = Math.floor(elapsed.value / 60).toString().padStart(2, '0')
    const s = (elapsed.value % 60).toString().padStart(2, '0')
    return `${m}:${s}`
  })

  function start() {
    elapsed.value = 0
    intervalId = setInterval(() => { elapsed.value++ }, 1000)
  }

  function stop() {
    if (intervalId) {
      clearInterval(intervalId)
      intervalId = null
    }
  }

  function reset() {
    stop()
    elapsed.value = 0
  }

  return { elapsed, status, formattedElapsed, start, stop, reset }
}
