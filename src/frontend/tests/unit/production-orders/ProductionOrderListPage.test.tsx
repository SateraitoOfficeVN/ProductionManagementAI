import { render, screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter, Route, Routes, useLocation } from 'react-router-dom'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { axe } from 'vitest-axe'
import { AuthContext, type AuthContextValue } from '../../../src/features/auth/authContext'
import { ProductionOrderListPage } from '../../../src/features/production-orders/ProductionOrderListPage'
import type { PagedResult, ProductionOrderListItem } from '../../../src/features/production-orders/types'

// DD-002 test viewpoints at unit level (U): TC-102, TC-103, TC-112, TC-115, TC-116, TC-117, TC-118.

const products = [
  { id: 'p1', sku: 'P-1001', name: 'Steel bracket' },
  { id: 'p4', sku: 'P-1004', name: 'Drive shaft' },
]

function item(overrides: Partial<ProductionOrderListItem> = {}): ProductionOrderListItem {
  return {
    id: 'o1',
    orderNumber: 'PO-2026-00042',
    product: products[1],
    quantity: 250,
    dueDate: '2026-09-30',
    status: 'InProgress',
    isOverdue: false,
    updatedAt: '2026-09-20T01:12:03Z',
    ...overrides,
  }
}

function page(overrides: Partial<PagedResult<ProductionOrderListItem>> = {}): PagedResult<ProductionOrderListItem> {
  return {
    items: [item()],
    total: 1,
    page: 1,
    pageSize: 20,
    sort: 'dueDate',
    dir: 'asc',
    ...overrides,
  }
}

type Reply = { status: number; body?: unknown }
let routes: Record<string, Reply>
let requested: string[]

function json(reply: Reply): Response {
  return new Response(reply.body === undefined ? null : JSON.stringify(reply.body), {
    status: reply.status,
    headers: { 'Content-Type': reply.status >= 400 ? 'application/problem+json' : 'application/json' },
  })
}

/** Replies to any /api/production-orders query; the exact query string is asserted through `requested`. */
function listReply(reply: Reply) {
  routes['GET /api/production-orders'] = reply
}

beforeEach(() => {
  requested = []
  routes = { 'GET /api/products': { status: 200, body: products } }
  listReply({ status: 200, body: page() })
  vi.stubGlobal(
    'fetch',
    vi.fn(async (url: string, init: RequestInit = {}) => {
      requested.push(url)
      const path = url.split('?')[0]
      return json(routes[`${init.method ?? 'GET'} ${path}`] ?? { status: 500 })
    }),
  )
})

afterEach(() => vi.unstubAllGlobals())

function LocationProbe() {
  const location = useLocation()
  return <div data-testid="location">{`${location.pathname}${location.search}`}</div>
}

function renderList(path = '/production-orders', roles = ['Operator']) {
  const auth: AuthContextValue = {
    user: { id: 'u1', userName: 'op', displayName: 'Op', roles },
    isLoading: false,
    login: vi.fn(),
    logout: vi.fn(),
  }
  return render(
    <MemoryRouter initialEntries={[path]}>
      <AuthContext.Provider value={auth}>
        <Routes>
          <Route path="/" element={<div>Home page</div>} />
          <Route path="/production-orders" element={<ProductionOrderListPage />} />
          <Route path="/production-orders/new" element={<div>Create screen</div>} />
          <Route path="/production-orders/:id" element={<div>Edit screen</div>} />
        </Routes>
        <LocationProbe />
      </AuthContext.Provider>
    </MemoryRouter>,
  )
}

const lastQuery = () => new URL(requested.filter((url) => url.startsWith('/api/production-orders')).at(-1)!, 'http://x')
const location = () => screen.getByTestId('location').textContent ?? ''
const table = () => screen.getByRole('table')

describe('ProductionOrderListPage — rows and states', () => {
  it('shows each row with its identifying and planning fields (REQ-021, TC-102)', async () => {
    renderList()

    const row = within(await screen.findByRole('table')).getAllByRole('row')[1]

    expect(within(row).getByRole('link', { name: 'PO-2026-00042' })).toHaveAttribute(
      'href',
      '/production-orders/o1',
    )
    expect(within(row).getByText('P-1004')).toBeInTheDocument()
    expect(within(row).getByText(/Drive shaft/)).toBeInTheDocument()
    expect(within(row).getByText('250')).toBeInTheDocument()
    expect(within(row).getByText(/2026-09-30/)).toBeInTheDocument()
    expect(within(row).getByText('In progress')).toBeInTheDocument()
    // No raw identifier or bare enum name is shown (REQ-021 failure criterion).
    expect(row.textContent).not.toContain('o1')
    expect(row.textContent).not.toContain('InProgress')
  })

  it('marks an overdue row with text, not colour alone (REQ-021, TC-103)', async () => {
    listReply({
      status: 200,
      body: page({
        items: [item({ isOverdue: true }), item({ id: 'o2', orderNumber: 'PO-2026-00043', isOverdue: false })],
        total: 2,
      }),
    })
    renderList()

    const rows = within(await screen.findByRole('table')).getAllByRole('row')

    expect(within(rows[1]).getByText('Overdue')).toBeInTheDocument()
    expect(within(rows[2]).queryByText('Overdue')).not.toBeInTheDocument()
  })

  it('shows the empty state, and no filter panel, when no order exists at all (REQ-020, TC-116)', async () => {
    listReply({ status: 200, body: page({ items: [], total: 0 }) })
    renderList()

    expect(await screen.findByText('No production orders yet. Create the first one.')).toBeInTheDocument()
    expect(screen.queryByRole('form', { name: 'Filter production orders' })).not.toBeInTheDocument()
    expect(screen.queryByRole('table')).not.toBeInTheDocument()
  })

  it('shows the no-match state and keeps the filter values when filters match nothing (REQ-022, TC-116)', async () => {
    listReply({ status: 200, body: page({ items: [], total: 0 }) })
    renderList('/production-orders?status=Draft&orderNumber=ZZZ')

    expect(await screen.findByText('No orders match your filters.')).toBeInTheDocument()
    expect(screen.getByLabelText('Order number')).toHaveValue('ZZZ')
    expect(screen.getByLabelText('Draft')).toBeChecked()
    expect(screen.getByRole('button', { name: 'Clear filters' })).toBeInTheDocument()
  })

  it('shows an error banner with Retry, and Retry re-runs the same query (TC-117)', async () => {
    const user = userEvent.setup()
    listReply({ status: 500, body: { code: 'MSG-E013' } })
    renderList('/production-orders?status=Draft')

    expect(await screen.findByRole('alert')).toHaveTextContent('Something went wrong. Try again.')
    const before = requested.length

    listReply({ status: 200, body: page() })
    await user.click(screen.getByRole('button', { name: 'Retry' }))

    await waitFor(() => expect(requested.length).toBeGreaterThan(before))
    expect(lastQuery().searchParams.get('status')).toBe('Draft')
    expect(await screen.findByRole('table')).toBeInTheDocument()
  })

  it('shows the forbidden panel without querying when the user has no role (REQ-026)', () => {
    renderList('/production-orders', [])

    expect(screen.getByText("You don't have permission to view production orders.")).toBeInTheDocument()
    expect(requested).toHaveLength(0)
  })
})

describe('ProductionOrderListPage — view state in the URL (REQ-027, TC-115)', () => {
  it('reads filters, sort and paging from the URL and queries with them', async () => {
    renderList('/production-orders?status=Draft&status=InProgress&sort=quantity&dir=desc&page=2&pageSize=50')

    await screen.findByRole('table')

    const query = lastQuery().searchParams
    expect(query.getAll('status')).toEqual(['Draft', 'InProgress'])
    expect(query.get('sort')).toBe('quantity')
    expect(query.get('dir')).toBe('desc')
    expect(query.get('page')).toBe('2')
    expect(query.get('pageSize')).toBe('50')
  })

  it('falls back to the default view for unreadable values instead of failing', async () => {
    renderList('/production-orders?sort=bogus&page=-1&pageSize=7&status=Nope&dueFrom=nonsense')

    await screen.findByRole('table')

    const query = lastQuery().searchParams
    expect(query.get('sort')).toBeNull()
    expect(query.get('page')).toBeNull()
    expect(query.get('pageSize')).toBeNull()
    expect(query.getAll('status')).toEqual([])
    expect(query.get('dueFrom')).toBeNull()
  })

  it('writes the applied filters to the URL and omits defaults', async () => {
    const user = userEvent.setup()
    renderList()
    await screen.findByRole('table')

    await user.click(screen.getByLabelText('Draft'))
    await user.click(screen.getByRole('button', { name: 'Search' }))

    await waitFor(() => expect(location()).toContain('status=Draft'))
    expect(location()).not.toContain('pageSize')
    expect(location()).not.toContain('sort')
  })
})

describe('ProductionOrderListPage — filtering, sorting and paging', () => {
  it('returns to page 1 when a filter, the sort or the page size changes (REQ-024, TC-112)', async () => {
    const user = userEvent.setup()
    listReply({ status: 200, body: page({ total: 80, page: 3, pageSize: 20 }) })
    renderList('/production-orders?page=3')
    await screen.findByRole('table')

    await user.click(within(table()).getByRole('button', { name: /Quantity|Qty/ }))
    await waitFor(() => expect(location()).toContain('sort=quantity'))
    expect(location()).not.toContain('page=3')

    await user.selectOptions(screen.getByLabelText('Rows'), '50')
    await waitFor(() => expect(location()).toContain('pageSize=50'))
    expect(location()).not.toContain('page=3')
  })

  it('toggles the direction when the active sort column is chosen again (REQ-023)', async () => {
    const user = userEvent.setup()
    // The table renders from the response's echoed controls, not from the request, so the stub must echo them.
    listReply({ status: 200, body: page({ sort: 'quantity', dir: 'asc' }) })
    renderList('/production-orders?sort=quantity')
    await screen.findByRole('table')

    await user.click(within(table()).getByRole('button', { name: /Qty/ }))

    await waitFor(() => expect(location()).toContain('dir=desc'))
  })

  it('marks the active column with aria-sort and no other column (REQ-023, TC-118)', async () => {
    listReply({ status: 200, body: page({ sort: 'quantity', dir: 'desc' }) })
    renderList('/production-orders?sort=quantity&dir=desc')

    const headers = within(await screen.findByRole('table')).getAllByRole('columnheader')
    const sorted = headers.filter((header) => header.getAttribute('aria-sort'))

    expect(sorted).toHaveLength(1)
    expect(sorted[0]).toHaveAttribute('aria-sort', 'descending')
    expect(sorted[0]).toHaveTextContent(/Qty/)
  })

  it('validates the filter panel before querying, and keeps the loaded rows (V-09, V-10)', async () => {
    const user = userEvent.setup()
    renderList()
    await screen.findByRole('table')
    const before = requested.length

    await user.type(screen.getByLabelText('Due from'), '2026-10-01')
    await user.type(screen.getByLabelText('Due to'), '2026-09-01')
    await user.click(screen.getByRole('button', { name: 'Search' }))

    expect(await screen.findByText("'Due from' must be on or before 'Due to'.")).toBeInTheDocument()
    expect(requested).toHaveLength(before)
    expect(screen.getByRole('table')).toBeInTheDocument()
    expect(screen.getByLabelText('Due from')).toHaveFocus()
  })

  it('disables the paging controls at the ends rather than hiding them (REQ-024)', async () => {
    listReply({ status: 200, body: page({ total: 40, page: 1, pageSize: 20 }) })
    renderList()

    await screen.findByRole('table')

    expect(screen.getByRole('button', { name: /Previous/ })).toBeDisabled()
    expect(screen.getByRole('button', { name: /Next/ })).toBeEnabled()
    expect(screen.getByText('Page 1 of 2')).toBeInTheDocument()
  })

  it('keeps the paging controls usable on a page past the last one (REQ-024, TC-111)', async () => {
    listReply({ status: 200, body: page({ items: [], total: 40, page: 9, pageSize: 20 }) })
    renderList('/production-orders?page=9')

    expect(await screen.findByText('Page 9 of 2')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /Previous/ })).toBeEnabled()
    expect(screen.queryByText('No orders match your filters.')).not.toBeInTheDocument()
  })

  it('announces the result range and total in a live region (M-09)', async () => {
    listReply({ status: 200, body: page({ total: 80, page: 2, pageSize: 20 }) })
    renderList('/production-orders?page=2')

    expect(await screen.findByText('21–40 of 80 orders')).toHaveAttribute('role', 'status')
  })
})

describe('ProductionOrderListPage — accessibility (TC-118)', () => {
  it('has no axe violations with rows rendered', async () => {
    const { container } = renderList()
    await screen.findByRole('table')

    expect(await axe(container)).toHaveNoViolations()
  })

  it('has no axe violations in the empty state', async () => {
    listReply({ status: 200, body: page({ items: [], total: 0 }) })
    const { container } = renderList()
    await screen.findByText('No production orders yet. Create the first one.')

    expect(await axe(container)).toHaveNoViolations()
  })
})
