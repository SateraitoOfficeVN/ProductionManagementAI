import { useAuth } from '../features/auth/useAuth'
import { AppNavbar } from './AppNavbar'
import { GuardedLink } from './GuardedLink'
import { iconProps, SignOutIcon } from './icons'
import { labels } from '../features/production-orders/messages'

// Shared header on every authenticated screen (003_BD "Shared application header", WI-004 DEC-016).
export function AppHeader() {
  const { user, logout } = useAuth()

  return (
    <header className="relative flex items-center justify-between gap-4 border-b border-gray-200 px-4 py-3 sm:px-6">
      <div className="flex flex-1 items-center justify-between gap-6 sm:flex-none sm:justify-start">
        <GuardedLink
          to="/"
          className="font-medium text-gray-900 focus-visible:rounded focus-visible:ring-2 focus-visible:ring-gray-900 focus-visible:ring-offset-2 focus-visible:outline-none"
        >
          <span className="hidden sm:inline">{labels.app.name}</span>
          <span className="sm:hidden">{labels.app.shortName}</span>
        </GuardedLink>
        {user && <AppNavbar />}
      </div>
      {user && (
        <div className="hidden items-center gap-3 text-sm text-gray-600 sm:flex">
          <span>{user.displayName}</span>
          <button
            type="button"
            onClick={() => void logout()}
            className="inline-flex items-center gap-1 rounded text-gray-900 underline underline-offset-4 focus-visible:ring-2 focus-visible:ring-gray-900 focus-visible:ring-offset-2 focus-visible:outline-none"
          >
            <SignOutIcon {...iconProps} />
            {labels.nav.signOut}
          </button>
        </div>
      )}
    </header>
  )
}
