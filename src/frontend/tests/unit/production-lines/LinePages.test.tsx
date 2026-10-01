import { fireEvent, render, screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { afterEach, expect, it, vi } from 'vitest'
import { axe } from 'vitest-axe'
import { AuthContext, type AuthContextValue } from '../../../src/features/auth/authContext'
import { NavigationGuardProvider } from '../../../src/components/NavigationGuardProvider'
import { ProductionLineListPage } from '../../../src/features/production-lines/ProductionLineListPage'
import { ProductionLineFormPage } from '../../../src/features/production-lines/ProductionLineFormPage'
import type { LineDetail } from '../../../src/features/production-lines/api'

const product = { id: '0197e4a0-0000-7000-8000-000000001001', sku: 'TEST-P', name: '製品', unit: 'kg', unitRevision: '9007199254740993', isActive: true }
const line: LineDetail = { id: '0197e4a0-0000-7000-8000-000000009001', code: 'LINE-1', name: 'ライン', workingHoursPerDay: '8', isActive: true, updatedAt: '2026-10-01T00:00:00Z', version: '4294967295', pairs: { items: [], total: 0, page: 1, pageSize: 50 } }
function setup(path = '/production-lines', roles = ['Operator'], failure?: 'stale' | 'lost', responder?: (url: string, method: string, body: unknown) => Response | Promise<Response>) {
  const calls: { method: string; url: string; body: unknown }[] = []
  vi.stubGlobal('fetch', vi.fn(async (url: string, init: RequestInit = {}) => {
    const method = init.method ?? 'GET'
    const body: unknown = init.body ? JSON.parse(String(init.body)) : undefined
    calls.push({ method, url, body })
    if (responder) return responder(url, method, body)
    if (method !== 'GET' && failure === 'lost') throw new TypeError('Connection lost')
    if (method !== 'GET' && failure === 'stale') return new Response(JSON.stringify({ code: 'LINE_STALE', errors: { version: ['LINE_STALE'] } }), { status: 409, headers: { 'Content-Type': 'application/problem+json' } })
    let value: unknown = line
    if (url.includes('/product-choices?')) value = { items: [product], total: 1, page: 1, pageSize: 50 }
    else if (url.startsWith('/api/production-lines?')) value = { items: [line], total: 1, page: 1, pageSize: 50 }
    return new Response(JSON.stringify(value), { status: method === 'POST' && !url.endsWith('/retire') ? 201 : 200, headers: { 'Content-Type': 'application/json' } })
  }))
  const auth: AuthContextValue = { user: { id: 'u', userName: 'operator', displayName: 'Operator', roles }, isLoading: false, login: vi.fn(), logout: vi.fn() }
  const view = render(<MemoryRouter initialEntries={[path]}><AuthContext.Provider value={auth}><NavigationGuardProvider><Routes>
    <Route path="/production-lines" element={<ProductionLineListPage />} /><Route path="/production-lines/new" element={<ProductionLineFormPage />} />
    <Route path="/production-lines/:id/edit" element={<ProductionLineFormPage />} />
  </Routes></NavigationGuardProvider></AuthContext.Provider></MemoryRouter>)
  return { ...view, calls }
}
afterEach(() => vi.unstubAllGlobals())
it('renders URL-applied active filters, a distinct Factory nav and accessible result table', async () => {
  const user = userEvent.setup(); const { container, calls } = setup()
  await screen.findByRole('rowheader', { name: line.code })
  expect(screen.getByRole('link', { name: '生産ライン・工程' }).querySelector('svg')).toHaveClass('lucide-factory')
  await user.type(screen.getByLabelText('コード・名称で検索'), '%_')
  expect(calls.filter(call => call.method === 'GET' && call.url.includes('q=%25_'))).toHaveLength(0)
  await user.click(screen.getByRole('button', { name: '検索' }))
  await waitFor(() => expect(calls.some(call => call.url.includes('q=%25_') && call.url.includes('state=active'))).toBe(true))
  expect(await axe(container)).toHaveNoViolations()
})
it('blocks malformed URL without a feature request and denies roles without protected rows', () => {
  const view = setup('/production-lines?page=10001')
  expect(screen.getByRole('alert')).toHaveTextContent('入力内容を確認してください。')
  expect(view.calls).toHaveLength(0); view.unmount()
  const forbidden = setup('/production-lines', [])
  expect(screen.getByRole('alert')).toHaveTextContent('この機能を利用する権限がありません。')
  expect(forbidden.calls).toHaveLength(0)
})
it('retirement Cancel and Escape preserve invoker focus and send no mutation', async () => {
  const user = userEvent.setup(); const { calls } = setup()
  const retire = (await screen.findAllByRole('button', { name: 'LINE-1を使用停止' }))[0]
  await user.click(retire)
  const dialog = screen.getByRole('dialog')
  expect(within(dialog).getByRole('button', { name: 'キャンセル' })).toHaveFocus()
  fireEvent(dialog, new Event('cancel', { cancelable: true }))
  await waitFor(() => expect(retire).toHaveFocus())
  expect(calls.filter(call => call.method !== 'GET')).toHaveLength(0)
})
it('creates explicitly confirmed timing with exact bigint generation and no numeric conversion', async () => {
  const user = userEvent.setup(); const { calls, container } = setup('/production-lines/new')
  expect(screen.getByRole('button', { name: '保存' })).toBeDisabled()
  await user.type(screen.getByLabelText('ラインコード'), ' NEW ')
  await user.type(screen.getByLabelText('ライン名'), '新規ライン')
  await user.type(screen.getByLabelText('稼働時間／日'), '7.500')
  await user.click(await screen.findByRole('button', { name: '製品を追加' }))
  await user.type(screen.getByLabelText('生産時間 (分／1単位)'), '0.001')
  await user.click(screen.getByLabelText('表示単位で生産時間を確認しました'))
  expect(await axe(container)).toHaveNoViolations()
  await user.click(screen.getByRole('button', { name: '保存' }))
  await waitFor(() => expect(calls.some(call => call.method === 'POST')).toBe(true))
  expect(calls.find(call => call.method === 'POST')?.body).toEqual({ code: 'NEW', name: '新規ライン', workingHoursPerDay: '7.500', products: [{ productId: product.id, minutesPerUnit: '0.001', expectedUnit: 'kg', expectedUnitRevision: '9007199254740993', confirmUnit: true }] })
})
it.each(['stale', 'lost'] as const)('retains edit values and blocks replay after %s until explicit verification/discard', async failure => {
  const user = userEvent.setup(); const { calls } = setup(`/production-lines/${line.id}/edit`, ['Operator'], failure)
  const name = await screen.findByLabelText('ライン名'); await user.clear(name); await user.type(name, '保持する入力')
  expect(screen.getByLabelText('ラインコード')).toHaveAttribute('readonly')
  await user.click(screen.getByRole('button', { name: '保存' }))
  await screen.findByText(failure === 'stale' ? '変更が競合しています。' : '保存結果を確認できません。再送信せず、現在の状態を確認してください。')
  expect(name).toHaveValue('保持する入力'); expect(screen.getByRole('button', { name: '保存' })).toBeDisabled()
  expect(calls.filter(call => call.method === 'PUT')).toHaveLength(1)
  await user.click(screen.getByRole('button', { name: '再読み込み' }))
  expect(screen.getByRole('dialog')).toBeInTheDocument()
  await user.click(within(screen.getByRole('dialog')).getByRole('button', { name: 'キャンセル' }))
  expect(name).toHaveValue('保持する入力')
})

it('clears explicit confirmation when a new coefficient changes, including a change back', async () => {
  const user = userEvent.setup(); setup('/production-lines/new')
  await user.click(await screen.findByRole('button', { name: '製品を追加' }))
  const minutes = screen.getByLabelText('生産時間 (分／1単位)')
  const confirmation = screen.getByLabelText('表示単位で生産時間を確認しました')
  await user.type(minutes, '0.125'); await user.click(confirmation)
  expect(confirmation).toBeChecked()
  await user.clear(minutes); await user.type(minutes, '0.250')
  expect(confirmation).not.toBeChecked()
  await user.click(confirmation); await user.clear(minutes); await user.type(minutes, '0.125')
  expect(confirmation).not.toBeChecked()
})

it('preserves off-page timing intents and reveals server indexed errors by stable product id', async () => {
  const user = userEvent.setup()
  const second = { ...product, id: '0197e4a0-0000-7000-8000-000000001002', sku: 'SECOND-P' }
  const pair = (item: typeof product) => ({ product: item, minutesPerUnit: '1', confirmedUnit: 'kg', confirmedUnitRevision: product.unitRevision, isActive: true, requiresUnitConfirmation: false, updatedAt: line.updatedAt })
  const { calls } = setup(`/production-lines/${line.id}/edit`, ['Operator'], undefined, (url, method) => {
    if (method === 'PUT') return new Response(JSON.stringify({ code: 'VALIDATION', errors: { 'productChanges[0].minutesPerUnit': ['VALIDATION'] } }), { status: 400, headers: { 'Content-Type': 'application/problem+json' } })
    const value = url.includes('product-choices') ? { items: [], total: 0, page: 1, pageSize: 50 }
      : { ...line, pairs: { items: [pair(url.includes('pairsPage=2') ? second : product)], total: 51, page: url.includes('pairsPage=2') ? 2 : 1, pageSize: 50 } }
    return new Response(JSON.stringify(value), { headers: { 'Content-Type': 'application/json' } })
  })
  const firstInput = await screen.findByLabelText('生産時間 (分／1単位)')
  await user.clear(firstInput); await user.type(firstInput, '0.125')
  await user.click(within(screen.getByRole('navigation', { name: '製品のページ切替' })).getByRole('button', { name: '次へ' }))
  await screen.findByText('SECOND-P — 製品')
  await user.click(screen.getByRole('button', { name: '保存' }))
  await waitFor(() => expect(screen.getByLabelText('生産時間 (分／1単位)')).toHaveFocus())
  const revealed = screen.getByLabelText('生産時間 (分／1単位)')
  expect(revealed).toHaveValue('0.125'); expect(revealed).toHaveAttribute('aria-invalid', 'true')
  expect(calls.find(call => call.method === 'PUT')?.body).toMatchObject({ productChanges: [{ action: 'setTiming', productId: product.id, minutesPerUnit: '0.125', confirmUnit: false }] })
})

it('refreshes unit observations on revisiting a page and clears previous confirmation without losing timing', async () => {
  const user = userEvent.setup(); let firstReads = 0
  const pair = { product, minutesPerUnit: '1', confirmedUnit: 'kg', confirmedUnitRevision: product.unitRevision, isActive: true, requiresUnitConfirmation: false, updatedAt: line.updatedAt }
  setup(`/production-lines/${line.id}/edit`, ['Operator'], undefined, (url) => {
    let value: unknown = { items: [], total: 0, page: 1, pageSize: 50 }
    if (!url.includes('product-choices')) {
      const page = url.includes('pairsPage=2') ? 2 : 1
      if (page === 1) firstReads++
      value = { ...line, pairs: { items: page === 2 ? [] : [{ ...pair, product: firstReads > 1 ? { ...product, unit: 'm', unitRevision: '9007199254740994' } : product, requiresUnitConfirmation: firstReads > 1 }], total: 51, page, pageSize: 50 } }
    }
    return new Response(JSON.stringify(value), { headers: { 'Content-Type': 'application/json' } })
  })
  const minutes = await screen.findByLabelText('生産時間 (分／1単位)')
  await user.clear(minutes); await user.type(minutes, '0.125')
  await user.click(screen.getByLabelText('表示単位で生産時間を確認しました'))
  const paging = screen.getByRole('navigation', { name: '製品のページ切替' })
  await user.click(within(paging).getByRole('button', { name: '次へ' }))
  await waitFor(() => expect(screen.queryByLabelText('生産時間 (分／1単位)')).not.toBeInTheDocument())
  await user.click(within(paging).getByRole('button', { name: '前へ' }))
  await screen.findByText('単位: m')
  expect(screen.getByLabelText('生産時間 (分／1単位)')).toHaveValue('0.125')
  expect(screen.getByLabelText('表示単位で生産時間を確認しました')).not.toBeChecked()
})

it('blocks a changed parent version on a new pair page without merging or losing the draft', async () => {
  const user = userEvent.setup()
  setup(`/production-lines/${line.id}/edit`, ['Operator'], undefined, url => new Response(JSON.stringify(url.includes('product-choices') ? { items: [], total: 0, page: 1, pageSize: 50 } : { ...line, version: url.includes('pairsPage=2') ? '12' : line.version, pairs: { items: [], total: 51, page: url.includes('pairsPage=2') ? 2 : 1, pageSize: 50 } }), { headers: { 'Content-Type': 'application/json' } }))
  const name = await screen.findByLabelText('ライン名'); await user.clear(name); await user.type(name, 'keep draft')
  await user.click(within(screen.getByRole('navigation', { name: '製品のページ切替' })).getByRole('button', { name: '次へ' }))
  await screen.findByText('変更が競合しています。')
  expect(name).toHaveValue('keep draft'); expect(screen.getByRole('button', { name: '保存' })).toBeDisabled()
})

it('removes an unsaved pair locally and does not send a retirement request', async () => {
  const user = userEvent.setup(); const { calls } = setup('/production-lines/new')
  await user.click(await screen.findByRole('button', { name: '製品を追加' }))
  await user.click(screen.getByRole('button', { name: '削除' }))
  expect(screen.queryByLabelText('生産時間 (分／1単位)')).not.toBeInTheDocument()
  expect(calls.every(call => call.method === 'GET')).toBe(true)
})

it('rejects oversized Unicode identity and focuses the invalid field without a mutation', async () => {
  const user = userEvent.setup(); const { calls } = setup('/production-lines/new')
  await user.type(screen.getByLabelText('ラインコード'), 'valid')
  fireEvent.change(screen.getByLabelText('ライン名'), { target: { value: '😀'.repeat(201) } })
  await user.type(screen.getByLabelText('稼働時間／日'), '8')
  await user.click(screen.getByRole('button', { name: '保存' }))
  await waitFor(() => expect(screen.getByLabelText('ライン名')).toHaveFocus())
  expect(screen.getByLabelText('ライン名')).toHaveAttribute('aria-invalid', 'true')
  expect(calls.every(call => call.method === 'GET')).toBe(true)
})

it('ignores a late list response after applying a new filter and shows read errors without fabricated empty rows', async () => {
  const user = userEvent.setup(); let release: ((value: Response) => void) | undefined
  const view = setup('/production-lines', ['Operator'], undefined, url => {
    if (!url.includes('q=fresh')) return new Promise<Response>(resolve => { release = resolve })
    return new Response(JSON.stringify({ items: [{ ...line, code: 'FRESH' }], total: 1, page: 1, pageSize: 50 }), { headers: { 'Content-Type': 'application/json' } })
  })
  await waitFor(() => expect(release).toBeDefined())
  await user.type(screen.getByLabelText('コード・名称で検索'), 'fresh'); await user.click(screen.getByRole('button', { name: '検索' }))
  await screen.findByRole('rowheader', { name: 'FRESH' })
  release?.(new Response(JSON.stringify({ items: [line], total: 1, page: 1, pageSize: 50 }), { headers: { 'Content-Type': 'application/json' } }))
  await waitFor(() => expect(screen.queryByRole('rowheader', { name: 'LINE-1' })).not.toBeInTheDocument())
  view.unmount()
  setup('/production-lines', ['Operator'], undefined, () => new Response(null, { status: 500 }))
  await screen.findByText('読み込みに失敗しました。')
  expect(screen.queryByText('一致する生産ラインはありません。')).not.toBeInTheDocument()
})

it('prevents repeat submit and navigation while a mutation is pending', async () => {
  const user = userEvent.setup(); let release: ((value: Response) => void) | undefined
  const { calls } = setup('/production-lines/new', ['Operator'], undefined, (url, method) => method === 'POST' ? new Promise<Response>(resolve => { release = resolve }) : new Response(JSON.stringify({ items: [], total: 0, page: 1, pageSize: 50 }), { headers: { 'Content-Type': 'application/json' } }))
  await user.type(screen.getByLabelText('ラインコード'), 'pending'); await user.type(screen.getByLabelText('ライン名'), 'fixture'); await user.type(screen.getByLabelText('稼働時間／日'), '8')
  await user.click(screen.getByRole('button', { name: '保存' }))
  expect(screen.getByRole('button', { name: '保存中…' })).toBeDisabled()
  const form = screen.getByRole('button', { name: '保存中…' }).closest('form')
  if (!form) throw new Error('Pending form missing')
  fireEvent.submit(form)
  await user.click(screen.getByRole('link', { name: 'キャンセル' }))
  expect(screen.getByLabelText('ラインコード')).toHaveValue('pending')
  expect(calls.filter(call => call.method === 'POST')).toHaveLength(1)
  release?.(new Response(JSON.stringify(line), { status: 201, headers: { 'Content-Type': 'application/json' } }))
  await screen.findByRole('heading', { name: '生産ライン・工程' })
})
