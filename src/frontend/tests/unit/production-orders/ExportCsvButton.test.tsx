import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { axe } from 'vitest-axe'
import { AuthContext, type AuthContextValue } from '../../../src/features/auth/authContext'
import { exportFileName } from '../../../src/features/production-orders/api'
import { ProductionOrderListPage } from '../../../src/features/production-orders/ProductionOrderListPage'
import type { PagedResult, ProductionOrderListItem } from '../../../src/features/production-orders/types'
import { setUnauthorizedHandler } from '../../../src/lib/apiClient'
import { saveFile } from '../../../src/lib/download'

// WI-016 test plan, unit level: TC-458 to TC-466 (002_DD-CSV test viewpoints, 002_DD-SPD-CSV P-16 to P-18).

vi.mock('../../../src/lib/download', () => ({ saveFile: vi.fn() }))

const EXPORT = '/api/production-orders/export'
const products = [{ id: 'p4', sku: 'P-1004', name: 'ドライブシャフト', unit: '個' }]

function item(overrides: Partial<ProductionOrderListItem> = {}): ProductionOrderListItem {
  return {
    id: 'o1',
    line: null,
    orderNumber: 'PO-2026-00042',
    product: products[0],
    quantity: 250,
    dueDate: '2026-09-30',
    status: 'InProgress',
    isOverdue: false,
    updatedAt: '2026-09-20T01:12:03Z',
    ...overrides,
  }
}

function page(total: number): PagedResult<ProductionOrderListItem> {
  return { items: total === 0 ? [] : [item()], total, page: 1, pageSize: 20, sort: 'dueDate', dir: 'asc' }
}

type Handler = (url: string, init: RequestInit) => Response | Promise<Response>
let routes: Record<string, Handler>
let requested: { url: string; init: RequestInit }[]

const json = (status: number, body?: unknown) =>
  new Response(body === undefined ? null : JSON.stringify(body), {
    status,
    headers: { 'Content-Type': status >= 400 ? 'application/problem+json' : 'application/json' },
  })

const csv = (count: string | null, disposition: string | null) => {
  const headers: Record<string, string> = { 'Content-Type': 'text/csv; charset=utf-8' }
  if (count !== null) headers['X-Total-Count'] = count
  if (disposition !== null) headers['Content-Disposition'] = disposition
  return new Response('﻿指示番号\r\n', { status: 200, headers })
}

const DISPOSITION =
  "attachment; filename=\"production-orders_20261007-1345.csv\"; filename*=UTF-8''%E8%A3%BD%E9%80%A0%E6%8C%87%E7%A4%BA%E4%B8%80%E8%A6%A7_20261007-1345.csv"

function listTotal(total: number) {
  routes['GET /api/production-orders'] = () => json(200, page(total))
}

function exportReply(handler: Handler) {
  routes[`GET ${EXPORT}`] = handler
}

beforeEach(() => {
  requested = []
  routes = { 'GET /api/products': () => json(200, products) }
  listTotal(87)
  exportReply(() => csv('87', DISPOSITION))
  vi.mocked(saveFile).mockClear()
  vi.stubGlobal(
    'fetch',
    vi.fn(async (url: string, init: RequestInit = {}) => {
      requested.push({ url, init })
      const handler = routes[`${init.method ?? 'GET'} ${url.split('?')[0]}`]
      return handler ? handler(url, init) : json(500)
    }),
  )
})

afterEach(() => {
  vi.unstubAllGlobals()
  setUnauthorizedHandler(null)
})

function renderList(path = '/production-orders?status=Draft') {
  const auth: AuthContextValue = {
    user: { id: 'u1', userName: 'op', displayName: 'Op', roles: ['Operator'] },
    isLoading: false,
    login: vi.fn(),
    logout: vi.fn(),
  }
  return render(
    <MemoryRouter initialEntries={[path]}>
      <AuthContext.Provider value={auth}>
        <Routes>
          <Route path="/production-orders" element={<ProductionOrderListPage />} />
        </Routes>
      </AuthContext.Provider>
    </MemoryRouter>,
  )
}

const exportButton = () => screen.findByRole('button', { name: /CSV出力|出力中…/ })
const exportRequests = () => requested.filter((r) => r.url.startsWith(EXPORT))

describe('CSV export — hint and visibility (TC-458, TC-459)', () => {
  it('describes the button with the hint for the searched filters, and passes axe', async () => {
    const { container } = renderList()

    const button = await exportButton()

    expect(button).toHaveAccessibleDescription('検索した絞り込み条件の87件を出力します。')
    expect(screen.getByText('検索した絞り込み条件の87件を出力します。')).toHaveAttribute('id', 'export-hint')
    expect(await axe(container)).toHaveNoViolations()
  })

  it('says every order is exported when no filter is applied', async () => {
    listTotal(1240)
    renderList('/production-orders')

    expect(await exportButton()).toHaveAccessibleDescription('すべての製造指示1,240件を出力します。')
  })

  it.each([
    ['empty', '/production-orders'],
    ['no match', '/production-orders?status=Draft'],
  ])('is not offered on the %s state', async (_, path) => {
    listTotal(0)
    renderList(path)

    await screen.findByText(/製造指示はまだありません|絞り込みに一致する製造指示はありません/)
    expect(screen.queryByRole('button', { name: 'CSV出力' })).not.toBeInTheDocument()
  })

  it('is not offered while the list is loading', async () => {
    routes['GET /api/production-orders'] = () => new Promise<Response>(() => {})
    renderList()

    await screen.findByText('製造指示を読み込み中…')
    expect(screen.queryByRole('button', { name: 'CSV出力' })).not.toBeInTheDocument()
  })
})

describe('CSV export — request and success (TC-460, TC-462)', () => {
  it('sends the applied filters and sort, never page or pageSize', async () => {
    const user = userEvent.setup()
    renderList('/production-orders?status=Draft&status=InProgress&sort=quantity&dir=desc&page=3&pageSize=50')

    await user.click(await exportButton())

    await waitFor(() => expect(exportRequests()).toHaveLength(1))
    const query = new URL(exportRequests()[0].url, 'http://x').searchParams
    expect(query.getAll('status')).toEqual(['Draft', 'InProgress'])
    expect(query.get('sort')).toBe('quantity')
    expect(query.get('dir')).toBe('desc')
    expect(query.has('page')).toBe(false)
    expect(query.has('pageSize')).toBe(false)
  })

  it('saves the file under the server name and announces the server count', async () => {
    const user = userEvent.setup()
    exportReply(() => csv('86', DISPOSITION))
    renderList()

    await user.click(await exportButton())

    expect(await screen.findByText('CSVファイルを出力しました（86件）。')).toHaveAttribute('role', 'status')
    expect(saveFile).toHaveBeenCalledTimes(1)
    const [blob, name] = vi.mocked(saveFile).mock.calls[0]
    expect(name).toBe('製造指示一覧_20261007-1345.csv')
    expect(blob).toBeInstanceOf(Blob)
  })
})

describe('CSV export — busy state (TC-461)', () => {
  it('stays focused, aria-disabled and aria-busy while exporting, and ignores a second activation', async () => {
    const user = userEvent.setup()
    let finish: (response: Response) => void = () => {}
    exportReply(() => new Promise<Response>((resolve) => (finish = resolve)))
    renderList()

    const button = await exportButton()
    await user.click(button)

    await waitFor(() => expect(button).toHaveTextContent('出力中…'))
    expect(button).toHaveAttribute('aria-disabled', 'true')
    expect(button).toHaveAttribute('aria-busy', 'true')
    expect(button).not.toHaveAttribute('disabled')
    expect(button).toHaveFocus()

    await user.click(button)
    await user.keyboard('{Enter}')
    expect(exportRequests()).toHaveLength(1)

    finish(csv('87', DISPOSITION))
    await waitFor(() => expect(button).toHaveTextContent('CSV出力'))
    expect(button).not.toHaveAttribute('aria-disabled')
    expect(button).toHaveFocus()
  })
})

describe('CSV export — limit and errors (TC-463, TC-464)', () => {
  it('refuses over 10,000 rows without a request', async () => {
    const user = userEvent.setup()
    listTotal(10_001)
    renderList()

    await user.click(await exportButton())

    expect(await screen.findByRole('alert')).toHaveTextContent(
      '出力件数が上限（10,000件）を超えています。条件を絞り込んでから、もう一度お試しください。',
    )
    expect(exportRequests()).toHaveLength(0)
  })

  it.each([
    ['403', () => json(403), '製造指示を閲覧する権限がありません。'],
    ['422 MSG-E024', () => json(422, { code: 'MSG-E024' }), '出力件数が上限（10,000件）を超えています。'],
    ['400', () => json(400, { code: 'VALIDATION', errors: { status: ['MSG-E018'] } }), '不明なステータスです。'],
    ['500', () => json(500), 'CSV出力に失敗しました。もう一度お試しください。'],
    ['network', () => Promise.reject(new TypeError('Failed to fetch')), 'CSV出力に失敗しました。'],
    ['missing X-Total-Count', () => csv(null, DISPOSITION), 'CSV出力に失敗しました。'],
  ] as [string, Handler, string][])('shows the mapped alert for %s and re-enables the button', async (_, handler, text) => {
    const user = userEvent.setup()
    exportReply(handler)
    renderList()

    const button = await exportButton()
    await user.click(button)

    expect(await screen.findByRole('alert')).toHaveTextContent(text)
    expect(saveFile).not.toHaveBeenCalled()
    expect(button).toHaveTextContent('CSV出力')
    expect(button).not.toHaveAttribute('aria-disabled')
  })

  it('hands a 401 to the session handler and shows no alert', async () => {
    const user = userEvent.setup()
    const unauthorized = vi.fn()
    setUnauthorizedHandler(unauthorized)
    exportReply(() => json(401))
    renderList()

    await user.click(await exportButton())

    await waitFor(() => expect(unauthorized).toHaveBeenCalledTimes(1))
    expect(screen.queryByRole('alert')).not.toBeInTheDocument()
  })
})

describe('CSV export — reset on view change (TC-465)', () => {
  it('clears the message when the view changes', async () => {
    const user = userEvent.setup()
    renderList()

    await user.click(await exportButton())
    await screen.findByText('CSVファイルを出力しました（87件）。')
    await user.selectOptions(screen.getByLabelText('表示件数'), '50')

    await waitFor(() => expect(screen.queryByText('CSVファイルを出力しました（87件）。')).not.toBeInTheDocument())
    expect(await exportButton()).toHaveAccessibleDescription('検索した絞り込み条件の87件を出力します。')
  })

  it('aborts an export in flight when the view changes', async () => {
    const user = userEvent.setup()
    exportReply((_, init) => new Promise<Response>((_, reject) =>
      init.signal?.addEventListener('abort', () => reject(new DOMException('aborted', 'AbortError')))))
    renderList()

    await user.click(await exportButton())
    await waitFor(() => expect(exportRequests()).toHaveLength(1))
    await user.selectOptions(screen.getByLabelText('表示件数'), '50')

    await waitFor(() => expect(exportRequests()[0].init.signal?.aborted).toBe(true))
    expect(screen.queryByRole('alert')).not.toBeInTheDocument()
  })
})

describe('CSV export — file name (TC-466)', () => {
  const now = new Date(2026, 9, 7, 13, 45)

  it.each([
    ['filename* wins', DISPOSITION, '製造指示一覧_20261007-1345.csv'],
    ['filename only', 'attachment; filename="production-orders_20261007-1345.csv"', 'production-orders_20261007-1345.csv'],
    ['no header', null, '製造指示一覧_20261007-1345.csv'],
    ['unsafe characters removed', "attachment; filename*=UTF-8''..%2F..%5Cevil%0A.csv", '....evil.csv'],
    ['broken encoding falls back to filename', "attachment; filename=\"plain.csv\"; filename*=UTF-8''%E0%A4%A", 'plain.csv'],
  ])('%s', (_, header, expected) => {
    expect(exportFileName(header, now)).toBe(expected)
  })
})
