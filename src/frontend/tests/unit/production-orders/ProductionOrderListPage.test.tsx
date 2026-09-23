import { render, screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter, Route, Routes, useLocation } from 'react-router-dom'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { axe } from 'vitest-axe'
import { AuthContext, type AuthContextValue } from '../../../src/features/auth/authContext'
import { ProductionOrderListPage } from '../../../src/features/production-orders/ProductionOrderListPage'
import type { PagedResult, ProductionOrderListItem } from '../../../src/features/production-orders/types'

// 002_DD test viewpoints at unit level (U): TC-102, TC-103, TC-112, TC-115, TC-116, TC-117, TC-118.

const products = [
  { id: 'p1', sku: 'P-1001', name: 'ブレーキキャリパー' },
  { id: 'p4', sku: 'P-1004', name: 'ドライブシャフト' },
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
    expect(within(row).getByText(/ドライブシャフト/)).toBeInTheDocument()
    expect(within(row).getByText('250')).toBeInTheDocument()
    expect(within(row).getByText('2026/09/30')).toBeInTheDocument()
    expect(within(row).getByText('進行中')).toBeInTheDocument()
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

    expect(within(rows[1]).getByText('納期遅れ')).toBeInTheDocument()
    expect(within(rows[2]).queryByText('納期遅れ')).not.toBeInTheDocument()
  })

  it('shows the empty state, and no filter panel, when no order exists at all (REQ-020, TC-116)', async () => {
    listReply({ status: 200, body: page({ items: [], total: 0 }) })
    renderList()

    expect(await screen.findByText('製造指示はまだありません。最初の1件を登録してください。')).toBeInTheDocument()
    expect(screen.queryByRole('form', { name: '製造指示の絞り込み' })).not.toBeInTheDocument()
    expect(screen.queryByRole('table')).not.toBeInTheDocument()
  })

  it('shows the no-match state and keeps the filter values when filters match nothing (REQ-022, TC-116)', async () => {
    listReply({ status: 200, body: page({ items: [], total: 0 }) })
    renderList('/production-orders?status=Draft&orderNumber=ZZZ')

    expect(await screen.findByText('絞り込みに一致する製造指示はありません。')).toBeInTheDocument()
    expect(screen.getByLabelText('指示番号')).toHaveValue('ZZZ')
    expect(screen.getByLabelText('下書き')).toBeChecked()
    expect(screen.getByRole('button', { name: '絞り込みをクリア' })).toBeInTheDocument()
  })

  it('shows an error banner with Retry, and Retry re-runs the same query (TC-117)', async () => {
    const user = userEvent.setup()
    listReply({ status: 500, body: { code: 'MSG-E013' } })
    renderList('/production-orders?status=Draft')

    expect(await screen.findByRole('alert')).toHaveTextContent('問題が発生しました。もう一度お試しください。')
    const before = requested.length

    listReply({ status: 200, body: page() })
    await user.click(screen.getByRole('button', { name: '再試行' }))

    await waitFor(() => expect(requested.length).toBeGreaterThan(before))
    expect(lastQuery().searchParams.get('status')).toBe('Draft')
    expect(await screen.findByRole('table')).toBeInTheDocument()
  })

  it('shows the forbidden panel without querying when the user has no role (REQ-026)', () => {
    renderList('/production-orders', [])

    expect(screen.getByText('製造指示を閲覧する権限がありません。')).toBeInTheDocument()
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

    await user.click(screen.getByLabelText('下書き'))
    await user.click(screen.getByRole('button', { name: '検索' }))

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

    await user.click(within(table()).getByRole('button', { name: /数量/ }))
    await waitFor(() => expect(location()).toContain('sort=quantity'))
    expect(location()).not.toContain('page=3')

    await user.selectOptions(screen.getByLabelText('表示件数'), '50')
    await waitFor(() => expect(location()).toContain('pageSize=50'))
    expect(location()).not.toContain('page=3')
  })

  it('toggles the direction when the active sort column is chosen again (REQ-023)', async () => {
    const user = userEvent.setup()
    // The table renders from the response's echoed controls, not from the request, so the stub must echo them.
    listReply({ status: 200, body: page({ sort: 'quantity', dir: 'asc' }) })
    renderList('/production-orders?sort=quantity')
    await screen.findByRole('table')

    await user.click(within(table()).getByRole('button', { name: /数量/ }))

    await waitFor(() => expect(location()).toContain('dir=desc'))
  })

  it('marks the active column with aria-sort and no other column (REQ-023, TC-118)', async () => {
    listReply({ status: 200, body: page({ sort: 'quantity', dir: 'desc' }) })
    renderList('/production-orders?sort=quantity&dir=desc')

    const headers = within(await screen.findByRole('table')).getAllByRole('columnheader')
    const sorted = headers.filter((header) => header.getAttribute('aria-sort'))

    expect(sorted).toHaveLength(1)
    expect(sorted[0]).toHaveAttribute('aria-sort', 'descending')
    expect(sorted[0]).toHaveTextContent(/数量/)
  })

  it('validates the filter panel before querying, and keeps the loaded rows (V-09, V-10)', async () => {
    const user = userEvent.setup()
    renderList()
    await screen.findByRole('table')
    const before = requested.length

    await user.type(screen.getByLabelText('納期（開始）'), '2026-10-01')
    await user.type(screen.getByLabelText('納期（終了）'), '2026-09-01')
    await user.click(screen.getByRole('button', { name: '検索' }))

    expect(await screen.findByText('納期（開始）は納期（終了）以前の日付にしてください。')).toBeInTheDocument()
    expect(requested).toHaveLength(before)
    expect(screen.getByRole('table')).toBeInTheDocument()
    expect(screen.getByLabelText('納期（開始）')).toHaveFocus()
  })

  it('disables the paging controls at the ends rather than hiding them (REQ-024)', async () => {
    listReply({ status: 200, body: page({ total: 40, page: 1, pageSize: 20 }) })
    renderList()

    await screen.findByRole('table')

    expect(screen.getByRole('button', { name: '前のページ' })).toBeDisabled()
    expect(screen.getByRole('button', { name: '次のページ' })).toBeEnabled()
    expect(screen.getByText('1 / 2 ページ')).toBeInTheDocument()
  })

  it('keeps the paging controls usable on a page past the last one (REQ-024, TC-111)', async () => {
    listReply({ status: 200, body: page({ items: [], total: 40, page: 9, pageSize: 20 }) })
    renderList('/production-orders?page=9')

    expect(await screen.findByText('9 / 2 ページ')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: '前のページ' })).toBeEnabled()
    expect(screen.queryByText('絞り込みに一致する製造指示はありません。')).not.toBeInTheDocument()
  })

  it('announces the result range and total in a live region (M-09)', async () => {
    listReply({ status: 200, body: page({ total: 80, page: 2, pageSize: 20 }) })
    renderList('/production-orders?page=2')

    expect(await screen.findByText('80件中 21〜40件')).toHaveAttribute('role', 'status')
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
    await screen.findByText('製造指示はまだありません。最初の1件を登録してください。')

    expect(await axe(container)).toHaveNoViolations()
  })
})
