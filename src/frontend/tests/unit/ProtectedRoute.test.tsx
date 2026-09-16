import { render, screen } from '@testing-library/react'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { describe, expect, it, vi } from 'vitest'
import { AuthContext, type AuthContextValue } from '../../src/features/auth/authContext'
import { ProtectedRoute } from '../../src/features/auth/ProtectedRoute'

function renderWithAuth(value: AuthContextValue) {
  return render(
    <MemoryRouter initialEntries={['/']}>
      <AuthContext.Provider value={value}>
        <Routes>
          <Route
            path="/"
            element={
              <ProtectedRoute>
                <div>Protected content</div>
              </ProtectedRoute>
            }
          />
          <Route path="/login" element={<div>Login page</div>} />
        </Routes>
      </AuthContext.Provider>
    </MemoryRouter>,
  )
}

describe('ProtectedRoute', () => {
  it('renders nothing while auth state is loading, to avoid flashing protected UI', () => {
    renderWithAuth({ user: null, isLoading: true, login: vi.fn(), logout: vi.fn() })

    expect(screen.queryByText('Protected content')).not.toBeInTheDocument()
    expect(screen.queryByText('Login page')).not.toBeInTheDocument()
  })

  it('redirects to /login when there is no authenticated user', () => {
    renderWithAuth({ user: null, isLoading: false, login: vi.fn(), logout: vi.fn() })

    expect(screen.getByText('Login page')).toBeInTheDocument()
  })

  it('renders the protected children when a user is authenticated', () => {
    renderWithAuth({
      user: { id: '1', userName: 'admin', displayName: 'Seed Admin', roles: ['Admin'] },
      isLoading: false,
      login: vi.fn(),
      logout: vi.fn(),
    })

    expect(screen.getByText('Protected content')).toBeInTheDocument()
  })
})
