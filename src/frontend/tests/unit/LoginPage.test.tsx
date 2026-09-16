import { fireEvent, render, screen, waitFor } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { describe, expect, it, vi } from 'vitest'
import { AuthContext, type AuthContextValue } from '../../src/features/auth/authContext'
import { LoginPage } from '../../src/features/auth/LoginPage'

const navigateMock = vi.fn()

vi.mock('react-router-dom', async (importOriginal) => {
  const actual = await importOriginal<typeof import('react-router-dom')>()
  return { ...actual, useNavigate: () => navigateMock }
})

function renderLoginPage(login: AuthContextValue['login']) {
  const value: AuthContextValue = { user: null, isLoading: false, login, logout: vi.fn() }

  return render(
    <MemoryRouter>
      <AuthContext.Provider value={value}>
        <LoginPage />
      </AuthContext.Provider>
    </MemoryRouter>,
  )
}

describe('LoginPage', () => {
  it('navigates home on successful login', async () => {
    const login = vi.fn().mockResolvedValue(undefined)
    renderLoginPage(login)

    fireEvent.change(screen.getByLabelText('Username'), { target: { value: 'admin' } })
    fireEvent.change(screen.getByLabelText('Password'), { target: { value: 'correct-password' } })
    fireEvent.click(screen.getByRole('button', { name: /sign in/i }))

    await waitFor(() => expect(login).toHaveBeenCalledWith('admin', 'correct-password'))
    await waitFor(() => expect(navigateMock).toHaveBeenCalledWith('/'))
  })

  it('shows a generic error message when login fails, regardless of cause', async () => {
    const login = vi.fn().mockRejectedValue(new Error('Invalid username or password.'))
    renderLoginPage(login)

    fireEvent.change(screen.getByLabelText('Username'), { target: { value: 'admin' } })
    fireEvent.change(screen.getByLabelText('Password'), { target: { value: 'wrong-password' } })
    fireEvent.click(screen.getByRole('button', { name: /sign in/i }))

    expect(await screen.findByText('Invalid username or password.')).toBeInTheDocument()
    expect(navigateMock).not.toHaveBeenCalled()
  })
})
