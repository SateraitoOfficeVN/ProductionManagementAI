import { useCallback, useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { AppHeader } from '../../components/AppHeader'
import { GuardedLink } from '../../components/GuardedLink'
import { ApiError } from '../../lib/apiClient'
import { useAuth } from '../auth/useAuth'
import { listOrders, listProducts } from './api'
import { ListPagination, ListSummary, PageSizeSelect } from './ListPagination'
import { MessageBanner } from './MessageBanner'
import { ProductionOrderFilters } from './ProductionOrderFilters'
import { ProductionOrderTable } from './ProductionOrderTable'
import { hasFilters, useListViewState } from './listViewState'
import { labels, message } from './messages'
import type {
  ListFilters,
  ListViewState,
  PageSize,
  PagedResult,
  Product,
  ProductionOrderListItem,
  ProductionOrderSort,
  SortDirection,
} from './types'

type QueryResult =
  | { kind: 'ready'; page: PagedResult<ProductionOrderListItem> }
  | { kind: 'invalid'; errors: Record<string, string[]> }
  | { kind: 'forbidden' }
  | { kind: 'error' }

const VIEWER_ROLES = ['Admin', 'Operator']

// 002_DD module 1 / 002_DD-SPD §1 and §6: route component for /production-orders. The URL is the single source of
// truth for what is displayed; the query is a reaction to it, so Back, reload and a pasted link all take one path.
export function ProductionOrderListPage() {
  const { user } = useAuth()
  const canView = user?.roles.some((role) => VIEWER_ROLES.includes(role)) ?? false
  const [view, setView] = useListViewState()
  const [products, setProducts] = useState<Product[]>([])
  const [productsFailed, setProductsFailed] = useState(false)
  const [result, setResult] = useState<QueryResult | null>(null)
  const [retryKey, setRetryKey] = useState(0)
  const loading = result === null

  const viewKey = JSON.stringify(view)

  useEffect(() => {
    document.title = labels.app.title(labels.list.heading)
  }, [])

  useEffect(() => {
    if (!canView) {
      return
    }
    let cancelled = false
    listProducts()
      .then((loaded) => !cancelled && setProducts(loaded))
      .catch(() => !cancelled && setProductsFailed(true))
    return () => {
      cancelled = true
    }
  }, [canView])

  useEffect(() => {
    if (!canView) {
      return
    }
    const controller = new AbortController()
    setResult(null)
    listOrders(view, controller.signal)
      .then((page) => setResult({ kind: 'ready', page }))
      .catch((error: unknown) => {
        // A superseded query was aborted by the cleanup below: its response is discarded, not rendered.
        if (controller.signal.aborted) {
          return
        }
        setResult(toFailure(error))
      })
    return () => controller.abort()
    // viewKey rather than view: the object is rebuilt on every render, the value is what identifies the query.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [canView, viewKey, retryKey])

  const publish = useCallback((next: ListViewState) => setView(next), [setView])

  const applyFilters = useCallback(
    (filters: ListFilters) => publish({ ...view, ...filters, page: 1 }),
    [publish, view],
  )

  const clearFilters = useCallback(
    () =>
      publish({
        ...view,
        statuses: [],
        productId: null,
        dueFrom: null,
        dueTo: null,
        orderNumber: null,
        page: 1,
      }),
    [publish, view],
  )

  const changeSort = useCallback(
    (sort: ProductionOrderSort, dir: SortDirection) => publish({ ...view, sort, dir, page: 1 }),
    [publish, view],
  )

  const changePage = useCallback((page: number) => publish({ ...view, page }), [publish, view])

  const changePageSize = useCallback(
    (pageSize: PageSize) => publish({ ...view, pageSize, page: 1 }),
    [publish, view],
  )

  if (!canView) {
    // UX only; the server enforces the role (REQ-026).
    return <Panel title={message('MSG-E020')} />
  }

  const page = result?.kind === 'ready' ? result.page : null
  const filterErrors = result?.kind === 'invalid' ? result.errors : null

  return (
    <div className="min-h-screen bg-white">
      <AppHeader />
      <main className="mx-auto grid max-w-5xl gap-4 px-4 pt-6 pb-10 sm:px-6">
        <nav aria-label={labels.nav.breadcrumb} className="text-sm text-gray-500">
          {/* Always underlined, not only on hover: a link inside a line of text must not rely on colour (WCAG 1.4.1). */}
          <GuardedLink to="/" className="text-gray-700 underline underline-offset-4">
            {labels.nav.home}
          </GuardedLink>
          <span aria-hidden="true"> › </span>
          {labels.nav.orders}
        </nav>

        <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
          <h1 className="text-xl font-medium text-gray-900">{labels.list.heading}</h1>
          <Link
            to="/production-orders/new"
            className="rounded bg-gray-900 px-4 py-2 text-center text-white focus-visible:ring-2 focus-visible:ring-gray-900 focus-visible:ring-offset-2 focus-visible:outline-none"
          >
            {labels.list.newOrder}
          </Link>
        </div>

        {result?.kind === 'error' && (
          <MessageBanner banner={{ kind: 'error', text: message('MSG-E013') }}>
            <button
              type="button"
              onClick={() => setRetryKey((key) => key + 1)}
              className="rounded border border-gray-300 bg-white px-2.5 py-1 text-gray-900 focus-visible:ring-2 focus-visible:ring-gray-900 focus-visible:ring-offset-2 focus-visible:outline-none"
            >
              {labels.common.retry}
            </button>
          </MessageBanner>
        )}
        {result?.kind === 'forbidden' && <Panel title={message('MSG-E020')} />}

        {/* The empty state offers nothing to filter, so the panel isn't rendered at all. */}
        {!isEmptyDatabase(page, view) && (
          <ProductionOrderFilters
            filters={view}
            products={products}
            productsFailed={productsFailed}
            busy={loading}
            onApply={applyFilters}
            onClear={clearFilters}
          />
        )}

        {filterErrors && (
          <p role="alert" className="text-sm text-red-600">
            {Object.values(filterErrors)
              .flat()
              .map((id) => message(id))
              .join(' ')}
          </p>
        )}

        {loading && (
          <p role="status" aria-busy="true" className="py-8 text-center text-sm text-gray-500">
            {labels.list.loading}
          </p>
        )}

        {page && page.total > 0 && (
          <>
            <div className="flex items-center justify-between gap-4">
              <ListSummary total={page.total} page={page.page} pageSize={page.pageSize} />
              <PageSizeSelect pageSize={page.pageSize} onPageSize={changePageSize} />
            </div>
            <ProductionOrderTable items={page.items} sort={page.sort} dir={page.dir} onSort={changeSort} />
            <ListPagination total={page.total} page={page.page} pageSize={page.pageSize} onPage={changePage} />
          </>
        )}

        {page && page.total === 0 && hasFilters(view) && (
          <Panel title={message('MSG-I004')} hint={labels.list.noMatchHint}>
            <button
              type="button"
              onClick={clearFilters}
              className="rounded border border-gray-300 bg-white px-4 py-2 text-gray-900 focus-visible:ring-2 focus-visible:ring-gray-900 focus-visible:ring-offset-2 focus-visible:outline-none"
            >
              {labels.list.clearFilters}
            </button>
          </Panel>
        )}

        {isEmptyDatabase(page, view) && (
          <Panel title={message('MSG-I003')}>
            <Link
              to="/production-orders/new"
              className="rounded bg-gray-900 px-4 py-2 text-white focus-visible:ring-2 focus-visible:ring-gray-900 focus-visible:ring-offset-2 focus-visible:outline-none"
            >
              {labels.list.newOrder}
            </Link>
          </Panel>
        )}
      </main>
    </div>
  )
}

function Panel({ title, hint, children }: { title: string; hint?: string; children?: React.ReactNode }) {
  return (
    <div className="grid justify-items-center gap-3 rounded-lg border border-gray-200 bg-white px-6 py-10 text-center">
      <p className="text-gray-700">{title}</p>
      {hint && <p className="text-sm text-gray-500">{hint}</p>}
      {children}
    </div>
  )
}

/** "No orders at all" is a different state from "no match" (002_BD items 24/25). */
function isEmptyDatabase(page: PagedResult<ProductionOrderListItem> | null, view: ListViewState) {
  return page !== null && page.total === 0 && !hasFilters(view)
}

function toFailure(error: unknown): QueryResult {
  if (error instanceof ApiError) {
    if (error.status === 403) {
      return { kind: 'forbidden' }
    }
    if (error.status === 400 && error.problem?.errors) {
      return { kind: 'invalid', errors: error.problem.errors }
    }
    // 401 is handled by apiClient, which clears the session; ProtectedRoute then redirects to /login.
    if (error.status === 401) {
      return { kind: 'error' }
    }
  }
  return { kind: 'error' }
}
