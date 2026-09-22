import { useMemo, useRef, type ReactNode } from 'react'
import { NavigationGuardContext, type NavigationGuard, type NavigationGuardValue } from '../lib/navigationGuard'

export function NavigationGuardProvider({ children }: { children: ReactNode }) {
  const guard = useRef<NavigationGuard | null>(null)

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
