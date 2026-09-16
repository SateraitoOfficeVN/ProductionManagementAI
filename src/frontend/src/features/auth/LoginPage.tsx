import { useState, type FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from './useAuth'

export function LoginPage() {
  const { login } = useAuth()
  const navigate = useNavigate()
  const [userName, setUserName] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [isSubmitting, setIsSubmitting] = useState(false)

  async function handleSubmit(event: FormEvent) {
    event.preventDefault()
    setError(null)
    setIsSubmitting(true)
    try {
      await login(userName, password)
      navigate('/')
    } catch {
      // Generic message regardless of cause, matching the backend's own generic failure response (ADR-0002).
      setError('Invalid username or password.')
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <main className="flex min-h-screen items-center justify-center">
      <form onSubmit={(event) => void handleSubmit(event)} className="w-full max-w-sm space-y-4 rounded-lg border border-gray-200 p-6">
        <h1 className="text-xl font-medium text-gray-900">Sign in</h1>
        {error && <p className="text-sm text-red-600">{error}</p>}
        <div>
          <label htmlFor="userName" className="block text-sm font-medium text-gray-700">
            Username
          </label>
          <input
            id="userName"
            name="userName"
            type="text"
            required
            value={userName}
            onChange={(event) => setUserName(event.target.value)}
            className="mt-1 w-full rounded border border-gray-300 px-3 py-2"
          />
        </div>
        <div>
          <label htmlFor="password" className="block text-sm font-medium text-gray-700">
            Password
          </label>
          <input
            id="password"
            name="password"
            type="password"
            required
            value={password}
            onChange={(event) => setPassword(event.target.value)}
            className="mt-1 w-full rounded border border-gray-300 px-3 py-2"
          />
        </div>
        <button
          type="submit"
          disabled={isSubmitting}
          className="w-full rounded bg-gray-900 px-3 py-2 text-white disabled:opacity-50"
        >
          {isSubmitting ? 'Signing in…' : 'Sign in'}
        </button>
      </form>
    </main>
  )
}
