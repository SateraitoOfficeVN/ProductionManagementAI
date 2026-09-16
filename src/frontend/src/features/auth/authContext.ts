import { createContext } from 'react'
import type { User } from './types'

export interface AuthContextValue {
  user: User | null
  isLoading: boolean
  login: (userName: string, password: string) => Promise<void>
  logout: () => Promise<void>
}

export const AuthContext = createContext<AuthContextValue | undefined>(undefined)
