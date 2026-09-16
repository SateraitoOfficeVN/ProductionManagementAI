import { Route, Routes } from 'react-router-dom'
import { LoginPage } from './features/auth/LoginPage'
import { ProtectedRoute } from './features/auth/ProtectedRoute'
import { useAuth } from './features/auth/useAuth'

function HomePage() {
  const { user, logout } = useAuth()

  return (
    <main className="flex min-h-screen flex-col items-center justify-center gap-4">
      <h1 className="text-2xl font-medium text-gray-900">ProductionManagementAI</h1>
      {user && (
        <p className="text-gray-600">
          Signed in as {user.displayName} ({user.roles.join(', ') || 'no roles'})
        </p>
      )}
      <button type="button" onClick={() => void logout()} className="rounded bg-gray-900 px-3 py-2 text-white">
        Sign out
      </button>
    </main>
  )
}

function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route
        path="/"
        element={
          <ProtectedRoute>
            <HomePage />
          </ProtectedRoute>
        }
      />
    </Routes>
  )
}

export default App
