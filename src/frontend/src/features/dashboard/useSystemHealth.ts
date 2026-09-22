import { useEffect, useState } from 'react'
import { ApiError } from '../../lib/apiClient'
import { getHealth } from './dashboardApi'

export type ServerStatus = 'ok' | 'unreachable'
export type DatabaseStatus = 'ok' | 'unavailable' | 'unknown'

export interface HealthState {
  phase: 'checking' | 'ready' | 'forbidden'
  server: ServerStatus
  database: DatabaseStatus
  /** checkedAt of the last answer the server gave (kept when a later check gets no answer). */
  lastAnswered: string | null
}

export const POLL_MS = 30_000
export const TIMEOUT_MS = 5_000

// BD-003 HS-01–HS-05, E-26–E-28; DD-003-SPD §10. Checks are chained with setTimeout after each completes, so a slow
// check never overlaps the next; polling pauses while the tab is hidden and resumes with an immediate check.
export function useSystemHealth(): HealthState {
  const [state, setState] = useState<HealthState>({ phase: 'checking', server: 'ok', database: 'unknown', lastAnswered: null })

  useEffect(() => {
    const unmount = new AbortController()
    let timer: ReturnType<typeof setTimeout> | undefined
    let inFlight = false
    let stopped = false

    const schedule = () => {
      clearTimeout(timer)
      if (!stopped && document.visibilityState === 'visible') {
        timer = setTimeout(() => void check(), POLL_MS)
      }
    }

    async function check() {
      if (inFlight || stopped) return
      inFlight = true
      try {
        const health = await getHealth(AbortSignal.any([unmount.signal, AbortSignal.timeout(TIMEOUT_MS)]))
        setState({ phase: 'ready', server: 'ok', database: health.database, lastAnswered: health.checkedAt })
      } catch (error) {
        if (unmount.signal.aborted) return
        if (error instanceof ApiError && error.status === 403) {
          stopped = true
          setState((s) => ({ ...s, phase: 'forbidden' }))
          return
        }
        // No answer, a timeout or a 5xx (a 401 has already sent the user to /login): the database is unknown,
        // never its last good value.
        setState((s) => ({ ...s, phase: 'ready', server: 'unreachable', database: 'unknown' }))
      } finally {
        inFlight = false
        if (!unmount.signal.aborted) schedule()
      }
    }

    const onVisibility = () => {
      if (document.visibilityState === 'visible') {
        void check()
      } else {
        clearTimeout(timer)
      }
    }

    document.addEventListener('visibilitychange', onVisibility)
    void check()
    return () => {
      unmount.abort()
      clearTimeout(timer)
      document.removeEventListener('visibilitychange', onVisibility)
    }
  }, [])

  return state
}
