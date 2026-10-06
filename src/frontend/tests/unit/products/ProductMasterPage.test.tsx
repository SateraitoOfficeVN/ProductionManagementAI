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

function setup(path: string | { pathname: string; state: unknown } = '/products', roles = ['Operator'], unitLocked = false, isActive = true) {
  const requests: { method: string; url: string; body?: unknown }[] = []
  vi.stubGlobal('fetch', vi.fn(async (url: string, init: RequestInit = {}) => {
    const method = init.method ?? 'GET'
    const body = init.body ? JSON.parse(init.body as string) as unknown : undefined
    requests.push({ method, url, body })
    let status = 200
    let response: unknown = {}
    if (url.startsWith('/api/product-master?')) {
      const query = new URLSearchParams(url.split('?')[1])
      const hidden = query.get('q') === 'none'
      response = { items: hidden ? [] : [product], total: hidden ? 0 : 1, page: 1, pageSize: Number(query.get('pageSize')), sort: 'sku', dir: 'asc' }
    }
    else if (url === `/api/product-master/${product.id}` && method === 'GET') response = { ...product, unitLocked, isActive }
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
    <Route path="/products/:id/edit" element={<ProductFormPage />} />
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
  await user.type(screen.getByLabelText('製品コード・製品名で検索'), 'P-1001')
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
  const { requests } = setup(`/products/${product.id}/edit`, ['Operator'], true)
  const name = await screen.findByLabelText(/製品名/)
  expect(name).toHaveValue(product.name)
  expect(screen.getByLabelText(/単位/)).toHaveAttribute('readonly')
  expect(screen.getByLabelText(/単位/)).toHaveAccessibleDescription(/（変更不可）.*製造指示で使用されているため/)
  expect(screen.getByLabelText(/製品コード/)).toHaveAttribute('readonly')
  await user.clear(name)
  await user.type(name, '更新済み')
  await user.click(screen.getByRole('button', { name: '保存' }))
  await waitFor(() => expect(requests.some((r) => r.method === 'PUT')).toBe(true))
  expect(await screen.findByText('製品を保存しました。')).toBeInTheDocument()
})

it('hides the catalog from a signed-in user without an editor role', () => {
  const { requests } = setup('/products', [])
  expect(screen.getByRole('alert')).toHaveTextContent('この画面を表示する権限がありません。')
  expect(screen.getByRole('link', { name: 'ダッシュボードへ戻る' })).toHaveAttribute('href', '/')
  expect(screen.queryByRole('link', { name: '製品を登録' })).not.toBeInTheDocument()
  expect(requests).toHaveLength(0)
})

// WI-013 regression cases (BUG-006, BUG-007, ENH-001).
it('shows the designed list wording, status badge and actions column', async () => {
  setup()
  const table = await screen.findByRole('table', { name: '製品一覧' })
  expect(screen.getByRole('link', { name: '製品を登録' })).toHaveAttribute('href', '/products/new')
  expect(screen.getByText('製品情報を管理します。')).toBeInTheDocument()
  expect(within(table).getByRole('columnheader', { name: '操作' })).toBeInTheDocument()
  expect(within(table).getByRole('link', { name: 'P-1001を編集' })).toHaveAttribute('href', `/products/${product.id}/edit`)
  expect(screen.getByText('1件')).toBeInTheDocument()
  expect(screen.getByText('1 / 1')).toBeInTheDocument()
})

it('requests the chosen rows per page and returns to page 1', async () => {
  const user = userEvent.setup()
  const { requests } = setup('/products?q=&state=all&page=3&pageSize=50')
  await waitFor(() => expect(requests.some((r) => r.url.includes('pageSize=50'))).toBe(true))
  const select = await screen.findByLabelText('表示件数')
  expect([...select.querySelectorAll('option')].map((o) => o.value)).toEqual(['10', '20', '50', '100'])
  await user.selectOptions(select, '10')
  await waitFor(() => expect(requests.at(-1)?.url).toContain('page=1&pageSize=10'))
})

it('falls back to 20 rows for an unlisted page size', async () => {
  const { requests } = setup('/products?pageSize=15')
  await waitFor(() => expect(requests.some((r) => r.url.includes('pageSize=20'))).toBe(true))
  expect(requests.some((r) => r.url.includes('pageSize=15'))).toBe(false)
})

it('tells an empty search result apart from an empty catalogue', async () => {
  setup('/products?q=none')
  expect(await screen.findByText('条件に一致する製品はありません。')).toBeInTheDocument()
})

it('returns to the list after create and says when the filters hide the new product', async () => {
  const user = userEvent.setup()
  setup({ pathname: '/products/new', state: { returnTo: '/products?q=none&state=all&page=1&pageSize=20' } })
  expect(screen.getByRole('heading', { level: 1, name: '製品登録' })).toBeInTheDocument()
  await user.type(screen.getByLabelText(/製品コード/), 'NEW-2')
  await user.type(screen.getByLabelText(/製品名/), '試作品')
  await user.selectOptions(screen.getByLabelText(/単位/), '個')
  await user.click(screen.getByRole('button', { name: '保存' }))
  expect(await screen.findByText(/^製品を登録しました。現在の検索条件では表示されません。/)).toBeInTheDocument()
})

it('ignores an external return target', async () => {
  const user = userEvent.setup()
  setup({ pathname: '/products/new', state: { returnTo: '//evil.example/products' } })
  await user.click(screen.getByRole('button', { name: 'キャンセル' }))
  expect(await screen.findByRole('heading', { level: 1, name: '製品マスタ' })).toBeInTheDocument()
})

it('marks a retired product and shows the error summary on invalid input', async () => {
  const user = userEvent.setup()
  setup(`/products/${product.id}/edit`, ['Operator'], false, false)
  expect(await screen.findByText('この製品は新しい製造指示で選択できません。')).toBeInTheDocument()
  await user.clear(screen.getByLabelText(/製品名/))
  await user.click(screen.getByRole('button', { name: '保存' }))
  expect(screen.getByRole('status')).toHaveTextContent('入力内容を確認してください。')
  expect(screen.getByLabelText(/製品名/)).toHaveAttribute('aria-invalid', 'true')
})

it('names the product in the retire dialog', async () => {
  const user = userEvent.setup()
  setup()
  // jsdom renders both the PC table and the SP cards; the first button is the table's.
  await user.click((await screen.findAllByRole('button', { name: 'P-1001を使用停止' }))[0])
  const dialog = screen.getByRole('dialog', { name: '製品を使用停止にしますか？', hidden: true })
  expect(dialog).toHaveTextContent('ブレーキキャリパー（P-1001）は新しい製造指示で選択できなくなります。既存の製造指示は残ります。')
})

it('marks required fields with a red asterisk hidden from screen readers', () => {
  setup('/products/new')
  const marker = document.querySelector('label[for="sku"] [aria-hidden="true"]')
  expect(marker).toHaveTextContent('*')
  expect(marker).toHaveClass('text-[#a12424]')
  expect(screen.queryByText('必須')).not.toBeInTheDocument()
  // The accessible name excludes the aria-hidden asterisk.
  expect(screen.getByRole('textbox', { name: '製品コード' })).toBeRequired()
})
