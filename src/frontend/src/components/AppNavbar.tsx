import { useRef, useState, type ComponentType, type KeyboardEvent } from 'react'
import { useLocation } from 'react-router-dom'
import { useAuth } from '../features/auth/useAuth'
import { currentEntry } from '../lib/navigation'
import { GuardedLink } from './GuardedLink'
import {
  CloseIcon,
  iconProps,
  MenuIcon,
  NavDashboardIcon,
  NavNewOrderIcon,
  NavOrdersIcon,
  SignOutIcon,
} from './icons'

type Entry = { label: string; to: string; Icon: ComponentType<typeof iconProps>; primary?: boolean }

const entries: Entry[] = [
  { label: 'Dashboard', to: '/', Icon: NavDashboardIcon },
  { label: 'Production orders', to: '/production-orders', Icon: NavOrdersIcon },
  { label: 'New production order', to: '/production-orders/new', Icon: NavNewOrderIcon, primary: true },
]

const focusRing = 'focus-visible:ring-2 focus-visible:ring-gray-900 focus-visible:ring-offset-2 focus-visible:outline-none'

// The application navbar in the shared header (WI-004 DEC-016, BD-003 H-2–H-5, DD-003-SPD §8).
export function AppNavbar() {
  const { user, logout } = useAuth()
  const { pathname } = useLocation()
  const current = currentEntry(pathname)
  // The SP panel is open only on the route it was opened on, so any navigation closes it.
  const [openOn, setOpenOn] = useState<string | null>(null)
  const open = openOn === pathname
  const setOpen = (value: boolean) => setOpenOn(value ? pathname : null)
  const menuButton = useRef<HTMLButtonElement>(null)

  function onPanelKeyDown(event: KeyboardEvent<HTMLDivElement>) {
    if (event.key === 'Escape') {
      setOpen(false)
      menuButton.current?.focus()
    }
  }

  function linkClass(entry: Entry, stacked: boolean) {
    const isCurrent = current === entry.to
    if (entry.primary && !stacked) {
      return `inline-flex items-center gap-1.5 rounded bg-gray-900 px-3 py-1.5 text-white ${focusRing}`
    }
    return [
      'inline-flex items-center gap-1.5 rounded px-2 py-1.5',
      stacked ? 'py-2' : '',
      isCurrent ? 'font-semibold text-gray-900 underline decoration-2 underline-offset-8' : 'text-gray-700 hover:text-gray-900',
      focusRing,
    ].join(' ')
  }

  return (
    <>
      <nav aria-label="Main" className="hidden items-center gap-1 text-sm sm:flex">
        {entries.map((entry) => (
          <GuardedLink
            key={entry.to}
            to={entry.to}
            aria-current={current === entry.to ? 'page' : undefined}
            className={linkClass(entry, false)}
          >
            <entry.Icon {...iconProps} />
            {entry.label}
          </GuardedLink>
        ))}
      </nav>

      <button
        ref={menuButton}
        type="button"
        aria-expanded={open}
        aria-controls="app-menu"
        onClick={() => setOpen(!open)}
        className={`inline-flex items-center gap-1.5 rounded border border-gray-300 px-2.5 py-1 text-sm text-gray-900 sm:hidden ${focusRing}`}
      >
        {open ? <CloseIcon {...iconProps} /> : <MenuIcon {...iconProps} />}
        {open ? 'Close' : 'Menu'}
      </button>

      {open && (
        <div
          id="app-menu"
          onKeyDown={onPanelKeyDown}
          className="absolute inset-x-0 top-full z-20 grid gap-1 border-b border-gray-200 bg-white px-4 pt-2 pb-3 shadow-sm sm:hidden"
        >
          <nav aria-label="Main" className="grid">
            {entries.map((entry) => (
              <GuardedLink
                key={entry.to}
                to={entry.to}
                aria-current={current === entry.to ? 'page' : undefined}
                className={linkClass(entry, true)}
              >
                <entry.Icon {...iconProps} />
                {entry.label}
              </GuardedLink>
            ))}
          </nav>
          {user && (
            <div className="flex items-center gap-2 border-t border-gray-100 pt-2 text-sm text-gray-600">
              <span>{user.displayName}</span>
              <button
                type="button"
                onClick={() => void logout()}
                className={`inline-flex items-center gap-1 rounded text-gray-900 underline underline-offset-4 ${focusRing}`}
              >
                <SignOutIcon {...iconProps} />
                Sign out
              </button>
            </div>
          )}
        </div>
      )}
    </>
  )
}
