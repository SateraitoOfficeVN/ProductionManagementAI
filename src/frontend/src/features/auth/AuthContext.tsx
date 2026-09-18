import { useEffect, useState, type ReactNode } from 'react'
import { setUnauthorizedHandler } from '../../lib/apiClient'
import { AuthContext } from './authContext'
import type { User } from './types'

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null)
  const [isLoading, setIsLoading] = useState(true)

  // Any 401 from the shared apiClient (e.g. session expired mid-edit) clears the user; ProtectedRoute then
  // redirects to /login (DD-001-SPD §3).
  useEffect(() => {
    setUnauthorizedHandler(() => setUser(null))
    return () => setUnauthorizedHandler(null)
  }, [])

  useEffect(() => {
    let cancelled = false

    fetch('/api/auth/me', { credentials: 'same-origin' })
      .then((response) => (response.ok ? (response.json() as Promise<User>) : null))
      .then((data) => {
        if (!cancelled) setUser(data)
      })
      .finally(() => {
        if (!cancelled) setIsLoading(false)
      })

    return () => {
      cancelled = true
    }
  }, [])

  async function login(userName: string, password: string) {
    const response = await fetch('/api/auth/login', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'same-origin',
      body: JSON.stringify({ userName, password }),
    })

    if (!response.ok) {
      throw new Error('Invalid username or password.')
    }

    setUser((await response.json()) as User)
  }

  async function logout() {
    await fetch('/api/auth/logout', { method: 'POST', credentials: 'same-origin' })
    setUser(null)
  }

  return <AuthContext.Provider value={{ user, isLoading, login, logout }}>{children}</AuthContext.Provider>
}
