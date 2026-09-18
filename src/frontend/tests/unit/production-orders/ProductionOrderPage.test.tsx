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
  { id: 'p1', sku: 'P-1001', name: 'Steel bracket' },
  { id: 'p4', sku: 'P-1004', name: 'Drive shaft' },
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
          <Route path="/production-orders/new" element={<ProductionOrderPage />} />
          <Route path="/production-orders/:id" element={<ProductionOrderPage />} />
        </Routes>
        <LocationProbe />
      </AuthContext.Provider>
    </MemoryRouter>,
  )
}

const saveButton = () => screen.getByRole('button', { name: 'Save' })

describe('ProductionOrderPage — create mode', () => {
  it('shows an empty form with Draft status, "Assigned on save", and the product list, with no axe violations', async () => {
    const { container } = renderPage('/production-orders/new')

    expect(await screen.findByLabelText(/Product/)).toBeInTheDocument()
    expect(screen.getByRole('heading', { level: 1, name: 'New production order' })).toBeInTheDocument()
    expect(screen.getByText('Assigned on save')).toBeInTheDocument()
    expect(screen.getByText('Draft')).toBeInTheDocument()
    expect(screen.getByRole('option', { name: 'P-1004 — Drive shaft' })).toBeInTheDocument()
    expect(document.title).toBe('New production order — ProductionManagementAI')
    expect(await axe(container)).toHaveNoViolations()
  })

  it('blocks Save with inline errors and focuses the first invalid field', async () => {
    const user = userEvent.setup()
    renderPage('/production-orders/new')
    await screen.findByLabelText(/Product/)

    await user.click(saveButton())

    expect(screen.getByText('Select a product.')).toBeInTheDocument()
    expect(screen.getByText('Enter a whole number of 1 or more.')).toBeInTheDocument()
    expect(screen.getByText('Enter a due date.')).toBeInTheDocument()
    expect(screen.getByLabelText(/Product/)).toHaveFocus()
    expect(screen.getByLabelText(/Product/)).toHaveAttribute('aria-invalid', 'true')
    expect(calls.some((c) => c.method === 'POST')).toBe(false)
  })

  it('rejects a past due date on create', async () => {
    const user = userEvent.setup()
    renderPage('/production-orders/new')
    const due = await screen.findByLabelText(/Due date/)

    await user.type(due, '2000-01-01')
    await user.tab()

    expect(screen.getByText("Due date can't be in the past.")).toBeInTheDocument()
  })

  it('creates the order, then shows it in edit mode with the created message', async () => {
    const user = userEvent.setup()
    const created = order({ orderNumber: 'PO-2026-00001', notes: null })
    routes['POST /api/production-orders'] = { status: 201, body: created }
    routes['GET /api/production-orders/o1'] = { status: 200, body: created }
    renderPage('/production-orders/new')

    await user.selectOptions(await screen.findByLabelText(/Product/), 'p4')
    await user.type(screen.getByLabelText(/Quantity/), '250')
    await user.type(screen.getByLabelText(/Due date/), future)
    await user.click(saveButton())

    expect(await screen.findByRole('status')).toHaveTextContent('Production order PO-2026-00001 created.')
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

    await user.selectOptions(await screen.findByLabelText(/Product/), 'p1')
    await user.type(screen.getByLabelText(/Quantity/), '5')
    await user.type(screen.getByLabelText(/Due date/), future)
    await user.click(saveButton())

    expect(await screen.findByText('The selected product no longer exists.')).toBeInTheDocument()
  })

  it('shows "No products available" when the list is empty', async () => {
    routes['GET /api/products'] = { status: 200, body: [] }
    renderPage('/production-orders/new')

    const select = await screen.findByLabelText(/Product/)
    expect(select).toBeDisabled()
    expect(within(select).getByRole('option', { name: 'No products available.' })).toBeInTheDocument()
  })
})

describe('ProductionOrderPage — edit mode', () => {
  it('locks product and quantity after Draft, keeps them focusable, and offers only allowed statuses', async () => {
    routes['GET /api/production-orders/o1'] = {
      status: 200,
      body: order({ status: 'InProgress', allowedNextStatuses: ['Completed', 'Cancelled'], isProductQuantityEditable: false }),
    }
    const { container } = renderPage('/production-orders/o1')

    const product = await screen.findByLabelText(/Product/)
    expect(product).toHaveAttribute('readonly')
    expect(product).toHaveValue('P-1004 — Drive shaft')
    expect(product).toHaveAccessibleDescription('Locked after the order leaves Draft.')
    expect(screen.getByLabelText(/Quantity/)).toHaveAttribute('readonly')
    expect(
      within(screen.getByLabelText('Status'))
        .getAllByRole('option')
        .map((o) => o.textContent),
    ).toEqual(['In progress', 'Completed', 'Cancelled'])
    expect(document.title).toBe('Production order PO-2026-00042 — ProductionManagementAI')
    expect(await axe(container)).toHaveNoViolations()
  })

  it('disables the status select for a terminal order', async () => {
    routes['GET /api/production-orders/o1'] = {
      status: 200,
      body: order({ status: 'Completed', allowedNextStatuses: [], isProductQuantityEditable: false }),
    }
    renderPage('/production-orders/o1')

    expect(await screen.findByLabelText('Status')).toBeDisabled()
  })

  it('lets an overdue order save without moving its due date (DEC-009)', async () => {
    const user = userEvent.setup()
    const overdue = order({ dueDate: '2000-01-01' })
    routes['GET /api/production-orders/o1'] = { status: 200, body: overdue }
    routes['PUT /api/production-orders/o1'] = { status: 200, body: { ...overdue, notes: 'x', version: 8 } }
    renderPage('/production-orders/o1')

    const notes = await screen.findByLabelText('Notes')
    await user.clear(notes)
    await user.type(notes, 'x')
    await user.click(saveButton())

    expect(await screen.findByRole('status')).toHaveTextContent('Production order PO-2026-00042 saved.')
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

    await user.type(await screen.findByLabelText('Notes'), ' mine')
    await user.click(saveButton())

    const alert = await screen.findByRole('alert')
    expect(alert).toHaveTextContent('This order was changed by someone else. Reload to see the latest version.')
    expect(screen.getByLabelText('Notes')).toHaveValue('note mine') // values kept

    await user.click(within(alert).getByRole('button', { name: 'Reload' }))

    await waitFor(() => expect(screen.getByLabelText('Notes')).toHaveValue('someone else'))
  })

  it('shows the rule-violation message from a 422', async () => {
    const user = userEvent.setup()
    routes['GET /api/production-orders/o1'] = { status: 200, body: order() }
    routes['PUT /api/production-orders/o1'] = { status: 422, body: { code: 'MSG-E008' } }
    renderPage('/production-orders/o1')

    await user.type(await screen.findByLabelText('Notes'), '!')
    await user.click(saveButton())

    expect(await screen.findByRole('alert')).toHaveTextContent(
      "Product and quantity can't be changed after the order leaves Draft.",
    )
  })

  it('shows a not-found panel for an unknown order', async () => {
    routes['GET /api/production-orders/missing'] = { status: 404, body: { code: 'MSG-E011' } }
    renderPage('/production-orders/missing')

    expect(await screen.findByRole('alert')).toHaveTextContent("This production order doesn't exist.")
    expect(screen.queryByRole('button', { name: 'Save' })).not.toBeInTheDocument()
  })

  it('flags notes over 500 code points', async () => {
    const user = userEvent.setup()
    routes['GET /api/production-orders/o1'] = { status: 200, body: order({ notes: 'a'.repeat(500) }) }
    renderPage('/production-orders/o1')

    const notes = await screen.findByLabelText('Notes')
    expect(screen.getByText('500/500')).toBeInTheDocument()
    await user.type(notes, 'b')
    await user.tab()

    expect(screen.getByText('501/500')).toBeInTheDocument()
    expect(screen.getByText("Notes can't exceed 500 characters.")).toBeInTheDocument()
  })
})

describe('ProductionOrderPage — access and cancel', () => {
  it('shows the permission panel without calling the API when the user has neither role', () => {
    renderPage('/production-orders/new', [])

    expect(screen.getByRole('alert')).toHaveTextContent("You don't have permission to manage production orders.")
    expect(calls).toHaveLength(0)
  })

  it('leaves straight away when nothing changed', async () => {
    const user = userEvent.setup()
    routes['GET /api/production-orders/o1'] = { status: 200, body: order() }
    renderPage('/production-orders/o1')
    await screen.findByLabelText('Notes')

    await user.click(screen.getByRole('button', { name: 'Cancel' }))

    expect(screen.getByText('Home page')).toBeInTheDocument()
  })

  it('asks before discarding changes; Keep editing keeps values and returns focus to Cancel (REQ-019)', async () => {
    const user = userEvent.setup()
    routes['GET /api/production-orders/o1'] = { status: 200, body: order() }
    renderPage('/production-orders/o1')

    await user.type(await screen.findByLabelText('Notes'), ' edited')
    await user.click(screen.getByRole('button', { name: 'Cancel' }))

    const dialog = screen.getByRole('dialog', { name: 'Discard your changes?' })
    expect(within(dialog).getByRole('button', { name: 'Keep editing' })).toHaveFocus()

    await user.click(within(dialog).getByRole('button', { name: 'Keep editing' }))

    expect(screen.queryByRole('dialog')).not.toBeInTheDocument()
    expect(screen.getByLabelText('Notes')).toHaveValue('note edited')
    expect(screen.getByRole('button', { name: 'Cancel' })).toHaveFocus()
  })

  it('Discard leaves without saving', async () => {
    const user = userEvent.setup()
    renderPage('/production-orders/new')

    await user.type(await screen.findByLabelText(/Quantity/), '5')
    await user.click(screen.getByRole('button', { name: 'Cancel' }))
    await user.click(screen.getByRole('button', { name: 'Discard' }))

    expect(screen.getByText('Home page')).toBeInTheDocument()
    expect(calls.some((c) => c.method === 'POST')).toBe(false)
  })
})

describe('localToday usage', () => {
  it('uses YYYY-MM-DD', () => expect(localToday()).toMatch(/^\d{4}-\d{2}-\d{2}$/))
})
