import { render } from '@testing-library/react'
import type { ReactNode } from 'react'
import { MemoryRouter } from 'react-router-dom'
import { vi } from 'vitest'
import { AuthContext, type AuthContextValue } from '../../../src/features/auth/authContext'
import { LocationProbe } from './LocationProbe'

export type Reply = { status: number; body?: unknown }
export type Handler = Reply | Reply[] | ((signal?: AbortSignal) => Promise<Reply>)

export const calls: string[] = []

/** Stubs fetch: `routes['GET /api/dashboard']` etc. An array is consumed one reply per call, keeping the last. */
export function stubFetch(routes: Record<string, Handler>) {
  calls.length = 0
  vi.stubGlobal(
    'fetch',
    vi.fn(async (url: string, init: RequestInit = {}) => {
      const key = `${init.method ?? 'GET'} ${url}`
      calls.push(key)
      const entry = routes[key]
      const reply: Reply =
        typeof entry === 'function'
          ? await entry(init.signal ?? undefined)
          : Array.isArray(entry)
            ? entry.length > 1
              ? entry.shift()!
              : entry[0]
            : (entry ?? { status: 500 })
      const isProblem = reply.status >= 400
      return new Response(reply.body === undefined ? null : JSON.stringify(reply.body), {
        status: reply.status,
        headers: { 'Content-Type': isProblem ? 'application/problem+json' : 'application/json' },
      })
    }),
  )
}

export function renderWithAuth(ui: ReactNode, path = '/', roles = ['Operator']) {
  const auth: AuthContextValue = {
    user: { id: 'u1', userName: 'op', displayName: 'Op', roles },
    isLoading: false,
    login: vi.fn(),
    logout: vi.fn(),
  }
  return render(
    <MemoryRouter initialEntries={[path]}>
      <AuthContext.Provider value={auth}>
        {ui}
        <LocationProbe />
      </AuthContext.Provider>
    </MemoryRouter>,
  )
}
