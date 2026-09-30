import { render, screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { afterEach, expect, it, vi } from 'vitest'
import { axe } from 'vitest-axe'
import { AuthContext, type AuthContextValue } from '../../../src/features/auth/authContext'
import { ProductFormPage, ProductMasterPage } from '../../../src/features/products/ProductMasterPage'

const product = {
  id: '0197e4a0-0000-7000-8000-000000001001', sku: 'P-1001', name: 'ブレーキキャリパー',
  unit: '個', drawingNumber: null, isActive: true, updatedAt: '2026-09-30T00:00:00Z',
  version: 7, unitLocked: false,
}

function setup(path = '/products', roles = ['Operator'], unitLocked = false) {
  const requests: { method: string; url: string; body?: unknown }[] = []
  vi.stubGlobal('fetch', vi.fn(async (url: string, init: RequestInit = {}) => {
    const method = init.method ?? 'GET'
    const body = init.body ? JSON.parse(init.body as string) as unknown : undefined
    requests.push({ method, url, body })
    let status = 200
    let response: unknown = {}
    if (url.startsWith('/api/product-master?')) response = {
      items: [product], total: 1, page: 1, pageSize: 20, sort: 'sku', dir: 'asc',
    }
    else if (url === `/api/product-master/${product.id}` && method === 'GET') response = { ...product, unitLocked }
    else if (url === '/api/product-master' && method === 'POST') {
      status = 201
      response = { ...product, ...(body as object), version: 8 }
    } else if (url === `/api/product-master/${product.id}` && method === 'PUT') response = {
      ...product, ...(body as object), version: 8,
    }
    else if (url.endsWith('/retire')) response = { ...product, isActive: false, version: 8 }
    else status = 404
    return new Response(JSON.stringify(response), { status, headers: { 'Content-Type': 'application/json' } })
  }))
  const auth: AuthContextValue = {
    user: { id: 'u1', userName: 'operator', displayName: 'Operator', roles },
    isLoading: false, login: vi.fn(), logout: vi.fn(),
  }
  const view = render(<MemoryRouter initialEntries={[path]}><AuthContext.Provider value={auth}><Routes>
    <Route path="/products" element={<ProductMasterPage />} />
    <Route path="/products/new" element={<ProductFormPage />} />
    <Route path="/products/:id" element={<ProductFormPage />} />
  </Routes></AuthContext.Provider></MemoryRouter>)
  return { ...view, requests }
}

afterEach(() => vi.unstubAllGlobals())

it('renders a searchable product table and cards with no axe violations', async () => {
  const user = userEvent.setup()
  const { container, requests } = setup()
  expect(await screen.findByRole('rowheader', { name: 'P-1001' })).toBeInTheDocument()
  const table = screen.getByRole('table', { name: '製品一覧' })
  expect(within(table).getByRole('columnheader', { name: '単位' })).toBeInTheDocument()
  await user.type(screen.getByLabelText('製品コード・製品名'), 'P-1001')
  await user.click(screen.getByRole('button', { name: '検索' }))
  await waitFor(() => expect(requests.some((r) => r.url.includes('q=P-1001'))).toBe(true))
  expect(await axe(container)).toHaveNoViolations()
})

it('saves a new product with the selected unit and no client-supplied state', async () => {
  const user = userEvent.setup()
  const { requests } = setup('/products/new')
  await user.type(screen.getByLabelText(/製品コード/), ' NEW-1 ')
  await user.type(screen.getByLabelText(/製品名/), '試作品')
  await user.selectOptions(screen.getByLabelText(/単位/), 'kg')
  await user.click(screen.getByRole('button', { name: '保存' }))
  await waitFor(() => expect(requests.some((r) => r.method === 'POST')).toBe(true))
  expect(requests.find((r) => r.method === 'POST')?.body).toEqual({
    sku: ' NEW-1 ', name: '試作品', unit: 'kg', drawingNumber: '',
  })
})

it('keeps a referenced product unit unavailable while allowing name edits', async () => {
  const user = userEvent.setup()
  setup(`/products/${product.id}`, ['Operator'], true)
  const name = await screen.findByLabelText(/製品名/)
  expect(name).toHaveValue(product.name)
  expect(screen.getByLabelText(/単位/)).toHaveAttribute('readonly')
  await user.clear(name)
  await user.type(name, '更新済み')
  await user.click(screen.getByRole('button', { name: '保存' }))
  await waitFor(() => expect(name).toHaveValue('更新済み'))
})

it('hides the catalog from a signed-in user without an editor role', () => {
  const { requests } = setup('/products', [])
  expect(screen.getByRole('alert')).toHaveTextContent('製造指示を管理する権限がありません。')
  expect(requests).toHaveLength(0)
})
