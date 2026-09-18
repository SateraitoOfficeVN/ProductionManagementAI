import { Link, Route, Routes } from 'react-router-dom'
import { AppHeader } from './components/AppHeader'
import { LoginPage } from './features/auth/LoginPage'
import { ProtectedRoute } from './features/auth/ProtectedRoute'
import { useAuth } from './features/auth/useAuth'
import { ProductionOrderPage } from './features/production-orders/ProductionOrderPage'

function HomePage() {
  const { user } = useAuth()

  return (
    <div className="min-h-screen bg-white">
      <AppHeader />
      <main className="mx-auto grid max-w-3xl gap-4 px-4 pt-8 sm:px-6">
        <h1 className="text-2xl font-medium text-gray-900">ProductionManagementAI</h1>
        {user && (
          <p className="text-gray-600">
            Signed in as {user.displayName} ({user.roles.join(', ') || 'no roles'})
          </p>
        )}
        {/* Entry point to Screen A until Screen B (the order list) exists (BD-001 screen transition). */}
        <Link
          to="/production-orders/new"
          className="justify-self-start rounded bg-gray-900 px-3 py-2 text-white focus-visible:ring-2 focus-visible:ring-gray-900 focus-visible:ring-offset-2 focus-visible:outline-none"
        >
          New production order
        </Link>
      </main>
    </div>
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
      <Route
        path="/production-orders/new"
        element={
          <ProtectedRoute>
            <ProductionOrderPage />
          </ProtectedRoute>
        }
      />
      <Route
        path="/production-orders/:id"
        element={
          <ProtectedRoute>
            <ProductionOrderPage />
          </ProtectedRoute>
        }
      />
    </Routes>
  )
}

export default App
