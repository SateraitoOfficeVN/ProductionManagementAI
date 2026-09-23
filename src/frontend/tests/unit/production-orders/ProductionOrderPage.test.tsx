import { render, screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter, Route, Routes, useLocation } from 'react-router-dom'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { axe } from 'vitest-axe'
import { AuthContext, type AuthContextValue } from '../../../src/features/auth/authContext'
import { ProductionOrderPage } from '../../../src/features/production-orders/ProductionOrderPage'
import type { ProductionOrder } from '../../../src/features/production-orders/types'
import { localToday } from '../../../src/features/production-orders/validation'

const products = [
  { id: 'p1', sku: 'P-1001', name: 'ブレーキキャリパー' },
  { id: 'p4', sku: 'P-1004', name: 'ドライブシャフト' },
]

const future = '2099-10-01'

function order(overrides: Partial<ProductionOrder> = {}): ProductionOrder {
  return {
    id: 'o1',
    orderNumber: 'PO-2026-00042',
    productId: 'p4',
    quantity: 250,
    dueDate: future,
    status: 'Draft',
    allowedNextStatuses: ['InProgress', 'Cancelled'],
    isProductQuantityEditable: true,
    notes: 'note',
    createdAt: '2026-09-18T01:02:03Z',
    updatedAt: '2026-09-18T01:02:03Z',
    version: 7,
    ...overrides,
  }
}

type Reply = { status: number; body?: unknown }
let routes: Record<string, Reply | Reply[]>
let calls: { method: string; url: string; body: unknown }[]

function json(reply: Reply): Response {
  const isProblem = reply.status >= 400
  return new Response(reply.body === undefined ? null : JSON.stringify(reply.body), {
    status: reply.status,
    headers: { 'Content-Type': isProblem ? 'application/problem+json' : 'application/json' },
  })
}

beforeEach(() => {
  calls = []
  routes = { 'GET /api/products': { status: 200, body: products } }
  vi.stubGlobal(
    'fetch',
    vi.fn(async (url: string, init: RequestInit = {}) => {
      const method = init.method ?? 'GET'
      calls.push({ method, url, body: init.body ? JSON.parse(init.body as string) : undefined })
      const entry = routes[`${method} ${url}`]
      const reply = Array.isArray(entry) ? (entry.length > 1 ? entry.shift()! : entry[0]) : entry
      return json(reply ?? { status: 500 })
    }),
  )
})

afterEach(() => vi.unstubAllGlobals())

function LocationProbe() {
  const location = useLocation()
  return <div data-testid="location">{location.pathname}</div>
}

function renderPage(path: string, roles = ['Operator']) {
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
          {/* Cancel returns to Screen B now that the list exists (002_BD screen transition, WI-003). */}
          <Route path="/production-orders" element={<div>Production order list</div>} />
          <Route path="/production-orders/new" element={<ProductionOrderPage />} />
          <Route path="/production-orders/:id" element={<ProductionOrderPage />} />
        </Routes>
        <LocationProbe />
      </AuthContext.Provider>
    </MemoryRouter>,
  )
}

const saveButton = () => screen.getByRole('button', { name: '保存' })

describe('ProductionOrderPage — create mode', () => {
  it('shows an empty form with Draft status, "Assigned on save", and the product list, with no axe violations', async () => {
    const { container } = renderPage('/production-orders/new')

    expect(await screen.findByLabelText(/製品/)).toBeInTheDocument()
    expect(screen.getByRole('heading', { level: 1, name: '新規製造指示' })).toBeInTheDocument()
    expect(screen.getByText('保存時に採番')).toBeInTheDocument()
    expect(screen.getByText('下書き')).toBeInTheDocument()
    expect(screen.getByRole('option', { name: 'P-1004 — ドライブシャフト' })).toBeInTheDocument()
    expect(document.title).toBe('新規製造指示 — ProductionManagementAI')
    expect(await axe(container)).toHaveNoViolations()
  })

  it('blocks Save with inline errors and focuses the first invalid field', async () => {
    const user = userEvent.setup()
    renderPage('/production-orders/new')
    await screen.findByLabelText(/製品/)

    await user.click(saveButton())

    expect(screen.getByText('製品を選択してください。')).toBeInTheDocument()
    expect(screen.getByText('1以上の整数を入力してください。')).toBeInTheDocument()
    expect(screen.getByText('納期を入力してください。')).toBeInTheDocument()
    expect(screen.getByLabelText(/製品/)).toHaveFocus()
    expect(screen.getByLabelText(/製品/)).toHaveAttribute('aria-invalid', 'true')
    expect(calls.some((c) => c.method === 'POST')).toBe(false)
  })

  it('rejects a past due date on create', async () => {
    const user = userEvent.setup()
    renderPage('/production-orders/new')
    const due = await screen.findByLabelText(/納期/)

    await user.type(due, '2000-01-01')
    await user.tab()

    expect(screen.getByText('納期に過去の日付は指定できません。')).toBeInTheDocument()
  })

  it('creates the order, then shows it in edit mode with the created message', async () => {
    const user = userEvent.setup()
    const created = order({ orderNumber: 'PO-2026-00001', notes: null })
    routes['POST /api/production-orders'] = { status: 201, body: created }
    routes['GET /api/production-orders/o1'] = { status: 200, body: created }
    renderPage('/production-orders/new')

    await user.selectOptions(await screen.findByLabelText(/製品/), 'p4')
    await user.type(screen.getByLabelText(/数量/), '250')
    await user.type(screen.getByLabelText(/納期/), future)
    await user.click(saveButton())

    expect(await screen.findByRole('status')).toHaveTextContent('製造指示 PO-2026-00001 を登録しました。')
    expect(screen.getByTestId('location')).toHaveTextContent('/production-orders/o1')
    expect(calls.find((c) => c.method === 'POST')?.body).toEqual({
      productId: 'p4',
      quantity: 250,
      dueDate: future,
      notes: null,
    })
  })

  it('maps server field errors inline', async () => {
    const user = userEvent.setup()
    routes['POST /api/production-orders'] = {
      status: 400,
      body: { code: 'VALIDATION', errors: { productId: ['MSG-E002'] } },
    }
    renderPage('/production-orders/new')

    await user.selectOptions(await screen.findByLabelText(/製品/), 'p1')
    await user.type(screen.getByLabelText(/数量/), '5')
    await user.type(screen.getByLabelText(/納期/), future)
    await user.click(saveButton())

    expect(await screen.findByText('選択した製品は存在しません。')).toBeInTheDocument()
  })

  it('shows "No products available" when the list is empty', async () => {
    routes['GET /api/products'] = { status: 200, body: [] }
    renderPage('/production-orders/new')

    const select = await screen.findByLabelText(/製品/)
    expect(select).toBeDisabled()
    expect(within(select).getByRole('option', { name: '利用可能な製品がありません。' })).toBeInTheDocument()
  })
})

describe('ProductionOrderPage — edit mode', () => {
  it('locks product and quantity after Draft, keeps them focusable, and offers only allowed statuses', async () => {
    routes['GET /api/production-orders/o1'] = {
      status: 200,
      body: order({ status: 'InProgress', allowedNextStatuses: ['Completed', 'Cancelled'], isProductQuantityEditable: false }),
    }
    const { container } = renderPage('/production-orders/o1')

    const product = await screen.findByLabelText(/製品/)
    expect(product).toHaveAttribute('readonly')
    expect(product).toHaveValue('P-1004 — ドライブシャフト')
    expect(product).toHaveAccessibleDescription('下書き以外の製造指示では変更できません。')
    expect(screen.getByLabelText(/数量/)).toHaveAttribute('readonly')
    expect(
      within(screen.getByLabelText('ステータス'))
        .getAllByRole('option')
        .map((o) => o.textContent),
    ).toEqual(['進行中', '完了', '取消'])
    expect(document.title).toBe('製造指示 PO-2026-00042 — ProductionManagementAI')
    expect(await axe(container)).toHaveNoViolations()
  })

  it('disables the status select for a terminal order', async () => {
    routes['GET /api/production-orders/o1'] = {
      status: 200,
      body: order({ status: 'Completed', allowedNextStatuses: [], isProductQuantityEditable: false }),
    }
    renderPage('/production-orders/o1')

    expect(await screen.findByLabelText('ステータス')).toBeDisabled()
  })

  it('lets an overdue order save without moving its due date (DEC-009)', async () => {
    const user = userEvent.setup()
    const overdue = order({ dueDate: '2000-01-01' })
    routes['GET /api/production-orders/o1'] = { status: 200, body: overdue }
    routes['PUT /api/production-orders/o1'] = { status: 200, body: { ...overdue, notes: 'x', version: 8 } }
    renderPage('/production-orders/o1')

    const notes = await screen.findByLabelText('備考')
    await user.clear(notes)
    await user.type(notes, 'x')
    await user.click(saveButton())

    expect(await screen.findByRole('status')).toHaveTextContent('製造指示 PO-2026-00042 を保存しました。')
    expect(calls.find((c) => c.method === 'PUT')?.body).toMatchObject({ dueDate: '2000-01-01', version: 7, status: 'Draft' })
  })

  it('shows a Reload banner on a stale save and reloads the latest version', async () => {
    const user = userEvent.setup()
    routes['GET /api/production-orders/o1'] = [
      { status: 200, body: order() },
      { status: 200, body: order({ notes: 'someone else', version: 9 }) },
    ]
    routes['PUT /api/production-orders/o1'] = { status: 409, body: { code: 'MSG-E009' } }
    renderPage('/production-orders/o1')

    await user.type(await screen.findByLabelText('備考'), ' mine')
    await user.click(saveButton())

    const alert = await screen.findByRole('alert')
    expect(alert).toHaveTextContent('この製造指示は他のユーザーによって変更されました。再読み込みして最新の内容を確認してください。')
    expect(screen.getByLabelText('備考')).toHaveValue('note mine') // values kept

    await user.click(within(alert).getByRole('button', { name: '再読み込み' }))

    await waitFor(() => expect(screen.getByLabelText('備考')).toHaveValue('someone else'))
  })

  it('shows the rule-violation message from a 422', async () => {
    const user = userEvent.setup()
    routes['GET /api/production-orders/o1'] = { status: 200, body: order() }
    routes['PUT /api/production-orders/o1'] = { status: 422, body: { code: 'MSG-E008' } }
    renderPage('/production-orders/o1')

    await user.type(await screen.findByLabelText('備考'), '!')
    await user.click(saveButton())

    expect(await screen.findByRole('alert')).toHaveTextContent(
      "下書き以外の製造指示では、製品と数量を変更できません。",
    )
  })

  it('shows a not-found panel for an unknown order', async () => {
    routes['GET /api/production-orders/missing'] = { status: 404, body: { code: 'MSG-E011' } }
    renderPage('/production-orders/missing')

    expect(await screen.findByRole('alert')).toHaveTextContent('この製造指示は存在しません。')
    expect(screen.queryByRole('button', { name: '保存' })).not.toBeInTheDocument()
  })

  it('flags notes over 500 code points', async () => {
    const user = userEvent.setup()
    routes['GET /api/production-orders/o1'] = { status: 200, body: order({ notes: 'a'.repeat(500) }) }
    renderPage('/production-orders/o1')

    const notes = await screen.findByLabelText('備考')
    expect(screen.getByText('500/500')).toBeInTheDocument()
    await user.type(notes, 'b')
    await user.tab()

    expect(screen.getByText('501/500')).toBeInTheDocument()
    expect(screen.getByText('備考は500文字以内で入力してください。')).toBeInTheDocument()
  })
})

describe('ProductionOrderPage — access and cancel', () => {
  it('shows the permission panel without calling the API when the user has neither role', () => {
    renderPage('/production-orders/new', [])

    expect(screen.getByRole('alert')).toHaveTextContent('製造指示を管理する権限がありません。')
    expect(calls).toHaveLength(0)
  })

  it('leaves straight away when nothing changed', async () => {
    const user = userEvent.setup()
    routes['GET /api/production-orders/o1'] = { status: 200, body: order() }
    renderPage('/production-orders/o1')
    await screen.findByLabelText('備考')

    await user.click(screen.getByRole('button', { name: 'キャンセル' }))

    expect(screen.getByText('Production order list')).toBeInTheDocument()
  })

  it('asks before discarding changes; Keep editing keeps values and returns focus to Cancel (REQ-019)', async () => {
    const user = userEvent.setup()
    routes['GET /api/production-orders/o1'] = { status: 200, body: order() }
    renderPage('/production-orders/o1')

    await user.type(await screen.findByLabelText('備考'), ' edited')
    await user.click(screen.getByRole('button', { name: 'キャンセル' }))

    const dialog = screen.getByRole('dialog', { name: '変更を破棄しますか？' })
    expect(within(dialog).getByRole('button', { name: '編集を続ける' })).toHaveFocus()

    await user.click(within(dialog).getByRole('button', { name: '編集を続ける' }))

    expect(screen.queryByRole('dialog')).not.toBeInTheDocument()
    expect(screen.getByLabelText('備考')).toHaveValue('note edited')
    expect(screen.getByRole('button', { name: 'キャンセル' })).toHaveFocus()
  })

  it('Discard leaves without saving', async () => {
    const user = userEvent.setup()
    renderPage('/production-orders/new')

    await user.type(await screen.findByLabelText(/数量/), '5')
    await user.click(screen.getByRole('button', { name: 'キャンセル' }))
    await user.click(screen.getByRole('button', { name: '破棄' }))

    expect(screen.getByText('Production order list')).toBeInTheDocument()
    expect(calls.some((c) => c.method === 'POST')).toBe(false)
  })
})

describe('localToday usage', () => {
  it('uses YYYY-MM-DD', () => expect(localToday()).toMatch(/^\d{4}-\d{2}-\d{2}$/))
})
