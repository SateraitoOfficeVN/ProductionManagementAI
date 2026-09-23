import type { ReactNode } from 'react'
import { Navigate } from 'react-router-dom'
import { useAuth } from './useAuth'

// UX convenience only (0002_ADR) — the server enforces the real auth boundary via
// [Authorize]/the fallback policy; this just avoids flashing protected UI before redirecting.
export function ProtectedRoute({ children }: { children: ReactNode }) {
  const { user, isLoading } = useAuth()

  if (isLoading) {
    return null
  }

  if (!user) {
    return <Navigate to="/login" replace />
  }

  return children
}
