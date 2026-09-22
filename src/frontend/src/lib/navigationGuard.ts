import { createContext, useContext } from 'react'

/** Returns true when it has taken over the navigation (e.g. an edited form opened its discard dialog). */
export type NavigationGuard = (to: string) => boolean

export interface NavigationGuardValue {
  /** Registers the guard; returns the function that unregisters it. One guard at a time (one form per screen). */
  register: (guard: NavigationGuard) => () => void
  /** Asks the registered guard whether it intercepts navigation to `to`. */
  intercepts: (to: string) => boolean
}

// WI-004 DEC-022 (DD-003 module 11). BrowserRouter has no useBlocker, so in-app links consult this context instead.
// Without a provider (e.g. a component rendered alone in a test) nothing is ever intercepted.
const noGuard: NavigationGuardValue = { register: () => () => {}, intercepts: () => false }

export const NavigationGuardContext = createContext<NavigationGuardValue>(noGuard)

export function useNavigationGuard(): NavigationGuardValue {
  return useContext(NavigationGuardContext)
}
