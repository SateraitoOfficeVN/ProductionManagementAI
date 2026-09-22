import { useCallback, useEffect, useRef, useState } from 'react'
import { Link, useLocation, useNavigate, useParams } from 'react-router-dom'
import { AppHeader } from '../../components/AppHeader'
import { ApiError } from '../../lib/apiClient'
import { useAuth } from '../auth/useAuth'
import { getOrder, listProducts } from './api'
import type { Banner } from './MessageBanner'
import { ProductionOrderForm } from './ProductionOrderForm'
import { message } from './messages'
import type { Product, ProductionOrder } from './types'

type LoadResult =
  | { kind: 'forbidden' }
  | { kind: 'notFound' }
  | { kind: 'loadError' }
  | { kind: 'ready'; products: Product[]; order: ProductionOrder | null; flash: Banner | null }

type PageState = { kind: 'loading' } | LoadResult

const APP = 'ProductionManagementAI'
const EDITOR_ROLES = ['Admin', 'Operator']

// DD-001 module 7 / DD-001-SPD §1 (P-01): route component for /production-orders/new and /production-orders/:id.
export function ProductionOrderPage() {
  const { id } = useParams()
  const mode = id === undefined ? 'create' : 'edit'
  const { user } = useAuth()
  const location = useLocation()
  const navigate = useNavigate()
  const canEdit = user?.roles.some((role) => EDITOR_ROLES.includes(role)) ?? false
  const [loadKey, setLoadKey] = useState(0)
  // Each load is identified by its request key; "loading" is derived (no result yet for the current key)
  // rather than set inside the effect.
  const requestKey = `${id ?? 'new'}#${loadKey}`
  const [loaded, setLoaded] = useState<{ key: string; result: LoadResult } | null>(null)
  const state: PageState = !canEdit
    ? { kind: 'forbidden' } // UX only; the server enforces the role (REQ-012)
    : loaded?.key === requestKey
      ? loaded.result
      : { kind: 'loading' }
  const flashRef = useRef<string | null>(null)
  const navigationFlash = (location.state as { flash?: string } | null)?.flash

  const reload = useCallback(() => setLoadKey((key) => key + 1), [])

  // Declared before the load effect so it runs first: capture the one-time flash handed over by the create
  // redirect, then clear it from history so Back/refresh doesn't repeat it (DD-001-SPD §1 step 8). The page
  // instance is reused when /new becomes /:id, so this can't be read once at mount.
  useEffect(() => {
    if (navigationFlash) {
      flashRef.current = navigationFlash
      navigate(location.pathname, { replace: true, state: null })
    }
  }, [navigationFlash, location.pathname, navigate])

  useEffect(() => {
    if (!canEdit) {
      return
    }

    let cancelled = false
    const settle = (result: LoadResult) => {
      if (!cancelled) setLoaded({ key: requestKey, result })
    }
    Promise.all([listProducts(), id === undefined ? Promise.resolve(null) : getOrder(id)])
      .then(([products, order]) => {
        const flashId = flashRef.current
        flashRef.current = null
        settle({
          kind: 'ready',
          products,
          order,
          flash: flashId && order ? { kind: 'success', text: message(flashId, { orderNumber: order.orderNumber }) } : null,
        })
      })
      .catch((error: unknown) => {
        if (error instanceof ApiError && error.status === 401) return // apiClient cleared the session → /login
        if (error instanceof ApiError && error.status === 404) settle({ kind: 'notFound' })
        else if (error instanceof ApiError && error.status === 403) settle({ kind: 'forbidden' })
        else settle({ kind: 'loadError' })
      })

    return () => {
      cancelled = true
    }
  }, [canEdit, id, requestKey])

  const order = state.kind === 'ready' ? state.order : null
  const heading = order ? `Production order ${order.orderNumber}` : 'New production order'

  useEffect(() => {
    document.title = mode === 'edit' && !order ? `Production order — ${APP}` : `${heading} — ${APP}`
  }, [heading, mode, order])

  return (
    <div className="min-h-screen bg-white">
      <AppHeader />
      <main className="mx-auto grid max-w-3xl gap-4 px-4 pt-5 pb-8 sm:px-6">
        <nav aria-label="Breadcrumb" className="text-sm text-gray-500">
          <Link to="/production-orders" className="underline-offset-4 hover:underline">
            {/* Back goes to the list now that Screen B exists (BD-002 screen transition). */}
            <span className="sm:hidden">‹ Production orders</span>
            <span className="hidden sm:inline">Production orders</span>
          </Link>
          <span className="hidden sm:inline">
            {' / '}
            <span className="text-gray-700">{order ? order.orderNumber : mode === 'create' ? 'New' : ''}</span>
          </span>
        </nav>
        <h1 className="text-xl font-medium text-gray-900">{mode === 'edit' && !order ? 'Production order' : heading}</h1>

        {state.kind === 'loading' && <LoadingCard />}
        {state.kind === 'forbidden' && <Panel text={message('MSG-E012')} />}
        {state.kind === 'notFound' && <Panel text={message('MSG-E011')} />}
        {state.kind === 'loadError' && <Panel text={message('MSG-E013')} onRetry={reload} />}
        {state.kind === 'ready' && (
          <ProductionOrderForm
            key={`${state.order?.id ?? 'new'}-${loadKey}`}
            mode={mode}
            order={state.order}
            products={state.products}
            initialBanner={state.flash}
            onCreated={(created) =>
              navigate(`/production-orders/${created.id}`, { replace: true, state: { flash: 'MSG-I001' } })
            }
            onReload={reload}
            onForbidden={() => setLoaded({ key: requestKey, result: { kind: 'forbidden' } })}
            onNotFound={() => setLoaded({ key: requestKey, result: { kind: 'notFound' } })}
          />
        )}
      </main>
    </div>
  )
}

function LoadingCard() {
  return (
    <div aria-busy="true" aria-label="Loading production order" className="grid gap-3 rounded-lg border border-gray-200 p-6">
      {[0, 1, 2, 3].map((row) => (
        <div key={row} className="h-9 animate-pulse rounded bg-gray-100 motion-reduce:animate-none" />
      ))}
    </div>
  )
}

function Panel({ text, onRetry }: { text: string; onRetry?: () => void }) {
  const ref = useRef<HTMLDivElement>(null)
  useEffect(() => ref.current?.focus(), [])

  return (
    <div
      ref={ref}
      tabIndex={-1}
      role="alert"
      className="grid justify-items-start gap-2 rounded-lg border border-gray-200 px-6 py-7 focus:outline-none"
    >
      <p className="text-gray-700">{text}</p>
      {onRetry ? (
        <button
          type="button"
          onClick={onRetry}
          className="rounded border border-gray-300 px-3 py-1.5 text-sm text-gray-900 focus-visible:ring-2 focus-visible:ring-gray-900 focus-visible:ring-offset-2 focus-visible:outline-none"
        >
          Try again
        </button>
      ) : (
        <Link to="/production-orders" className="text-gray-900 underline underline-offset-4">
          Back to production orders
        </Link>
      )}
    </div>
  )
}
