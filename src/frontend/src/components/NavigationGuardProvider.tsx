import { useLayoutEffect, useMemo, useRef, type ReactNode } from 'react'
import { useLocation } from 'react-router-dom'
import { NavigationGuardContext, type NavigationGuard, type NavigationGuardValue } from '../lib/navigationGuard'

export function NavigationGuardProvider({ children }: { children: ReactNode }) {
  const guard = useRef<NavigationGuard | null>(null)
  const location = useLocation()
  const historyIndex = useRef<number | null>(null)
  const restoring = useRef(false)
  useLayoutEffect(() => {
    const state: unknown = window.history.state
    historyIndex.current = state && typeof state === 'object' && 'idx' in state && typeof state.idx === 'number' ? state.idx : null
  }, [location.key])
  useLayoutEffect(() => {
    const onPop = (event: PopStateEvent) => {
      if (restoring.current) { restoring.current = false; return }
      const state: unknown = event.state
      const next = state && typeof state === 'object' && 'idx' in state && typeof state.idx === 'number' ? state.idx : null
      const current = historyIndex.current
      // BrowserRouter entries carry an index. External-document navigation uses beforeunload instead.
      if (current === null || next === null || current === next) return
      const to = window.location.pathname + window.location.search + window.location.hash
      if (guard.current?.(to)) {
        event.stopImmediatePropagation()
        restoring.current = true
        window.history.go(current - next)
      }
    }
    window.addEventListener('popstate', onPop, true)
    return () => window.removeEventListener('popstate', onPop, true)
  }, [])

  const value = useMemo<NavigationGuardValue>(
    () => ({
      register(next) {
        guard.current = next
        return () => {
          if (guard.current === next) {
            guard.current = null
          }
        }
      },
      intercepts: (to) => guard.current?.(to) ?? false,
    }),
    [],
  )

  return <NavigationGuardContext.Provider value={value}>{children}</NavigationGuardContext.Provider>
}
