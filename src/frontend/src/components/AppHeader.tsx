import { Link } from 'react-router-dom'
import { useAuth } from '../features/auth/useAuth'

export function AppHeader() {
  const { user, logout } = useAuth()

  return (
    <header className="flex items-center justify-between border-b border-gray-200 px-4 py-3 sm:px-6">
      <Link
        to="/"
        className="font-medium text-gray-900 focus-visible:rounded focus-visible:ring-2 focus-visible:ring-gray-900 focus-visible:ring-offset-2 focus-visible:outline-none"
      >
        <span className="hidden sm:inline">ProductionManagementAI</span>
        <span className="sm:hidden">PMAI</span>
      </Link>
      {user && (
        <div className="flex items-center gap-3 text-sm text-gray-600">
          <span className="hidden sm:inline">{user.displayName}</span>
          <button
            type="button"
            onClick={() => void logout()}
            className="rounded text-gray-900 underline underline-offset-4 focus-visible:ring-2 focus-visible:ring-gray-900 focus-visible:ring-offset-2 focus-visible:outline-none"
          >
            Sign out
          </button>
        </div>
      )}
    </header>
  )
}
