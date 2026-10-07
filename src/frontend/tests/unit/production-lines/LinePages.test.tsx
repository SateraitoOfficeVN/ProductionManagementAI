import { fireEvent, render, screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter, Route, Routes, useLocation } from 'react-router-dom'
import { afterEach, expect, it, vi } from 'vitest'
import { axe } from 'vitest-axe'
import { AuthContext, type AuthContextValue } from '../../../src/features/auth/authContext'
import { NavigationGuardProvider } from '../../../src/components/NavigationGuardProvider'
import { ProductionLineListPage } from '../../../src/features/production-lines/ProductionLineListPage'
import { ProductionLineFormPage } from '../../../src/features/production-lines/ProductionLineFormPage'
import type { LineDetail, PairDetail, ProductChoice } from '../../../src/features/production-lines/api'

// WI-015 TC-421–429, TC-433 at unit level, plus the WI-009 cases (TC-335, TC-360–364) with the redesigned wording.

const product: ProductChoice = { id: '0197e4a0-0000-7000-8000-000000001001', sku: 'TEST-P', name: '製品', unit: 'kg', unitRevision: '9007199254740993', isActive: true }
const line: LineDetail = { id: '0197e4a0-0000-7000-8000-000000009001', code: 'LINE-1', name: 'ライン', workingHoursPerDay: '8', isActive: true, updatedAt: '2026-10-01T00:00:00Z', version: '4294967295', pairs: { items: [], total: 0, page: 1, pageSize: 20 } }
const retiredLine = { ...line, id: '0197e4a0-0000-7000-8000-000000009002', code: 'LINE-2', isActive: false }
const pair = (item: ProductChoice, extra: Partial<PairDetail> = {}): PairDetail => ({ product: item, minutesPerUnit: '1', confirmedUnit: item.unit, confirmedUnitRevision: item.unitRevision, isActive: true, requiresUnitConfirmation: false, updatedAt: line.updatedAt, ...extra })
const json = (value: unknown, status = 200) => new Response(JSON.stringify(value), { status, headers: { 'Content-Type': status >= 400 ? 'application/problem+json' : 'application/json' } })
const timing = (sku = 'TEST-P', unit = 'kg') => `${sku} の製造時間（1 ${unit} あたりの分）`

function LocationProbe() { const location = useLocation(); return <output data-testid="location">{location.pathname + location.search}</output> }
function setup(path = '/production-lines', roles = ['Operator'], failure?: 'stale' | 'lost', responder?: (url: string, method: string, body: unknown) => Response | Promise<Response>) {
  const calls: { method: string; url: string; body: unknown }[] = []
  vi.stubGlobal('fetch', vi.fn(async (url: string, init: RequestInit = {}) => {
    const method = init.method ?? 'GET'
    const body: unknown = init.body ? JSON.parse(String(init.body)) : undefined
    calls.push({ method, url, body })
    if (responder) return responder(url, method, body)
    if (method !== 'GET' && failure === 'lost') throw new TypeError('Connection lost')
    if (method !== 'GET' && failure === 'stale') return json({ code: 'LINE_STALE', errors: { version: ['LINE_STALE'] } }, 409)
    let value: unknown = line
    if (url.includes('/product-choices?')) value = { items: [product], total: 1, page: 1, pageSize: 20 }
    else if (url.startsWith('/api/production-lines?')) value = { items: [line, retiredLine], total: 2, page: 1, pageSize: 20 }
    return json(value, method === 'POST' && !url.endsWith('/retire') ? 201 : 200)
  }))
  const auth: AuthContextValue = { user: { id: 'u', userName: 'operator', displayName: 'Operator', roles }, isLoading: false, login: vi.fn(), logout: vi.fn() }
  const view = render(<MemoryRouter initialEntries={[path]}><AuthContext.Provider value={auth}><NavigationGuardProvider><LocationProbe /><Routes>
    <Route path="/production-lines" element={<ProductionLineListPage />} /><Route path="/production-lines/new" element={<ProductionLineFormPage />} />
    <Route path="/production-lines/:id/edit" element={<ProductionLineFormPage />} />
  </Routes></NavigationGuardProvider></AuthContext.Provider></MemoryRouter>)
  return { ...view, calls }
}
const location = () => screen.getByTestId('location').textContent ?? ''
async function addProduct(user: ReturnType<typeof userEvent.setup>, name = /TEST-P/) {
  await user.click(await screen.findByRole('button', { name: '製品を追加' }))
  const dialog = await screen.findByRole('dialog', { name: '製品を追加' })
  await user.click(await within(dialog).findByRole('radio', { name }))
  await user.click(within(dialog).getByRole('button', { name: '追加' }))
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

it('shows the redesigned list: description, primary New, badges, red retire on active lines only and a count (TC-422)', async () => {
  setup()
  await screen.findByRole('rowheader', { name: 'LINE-1' })
  expect(screen.getByText('生産ラインと、製品ごとの製造時間を管理します。')).toBeInTheDocument()
  expect(screen.getByRole('link', { name: '新規ライン' })).toHaveClass('bg-[#1f5fa8]')
  expect(screen.getByText('2 件')).toBeInTheDocument()
  const table = screen.getByRole('table', { name: '生産ライン一覧' })
  expect(within(table).getByText('使用中')).toHaveClass('rounded-full')
  expect(within(table).getByText('使用停止', { selector: 'span' })).toHaveClass('rounded-full')
  expect(within(table).getByRole('button', { name: 'LINE-1を使用停止' })).toHaveClass('bg-[#a33232]')
  expect(within(table).queryByRole('button', { name: 'LINE-2を使用停止' })).not.toBeInTheDocument()
  expect(screen.getByRole('region', { name: '生産ライン一覧' })).toHaveAttribute('tabindex', '0')
  expect(screen.queryByRole('navigation', { name: '生産ラインのページ切替' })).not.toBeInTheDocument()
})

it('keeps 「表示件数」 in the URL: default 20, replaced when invalid, page 1 on change (TC-421)', async () => {
  const user = userEvent.setup(); const { calls } = setup('/production-lines?pageSize=25&page=1')
  await screen.findByRole('rowheader', { name: 'LINE-1' })
  await waitFor(() => expect(location()).toContain('pageSize=20'))
  expect(calls.every(call => !call.url.includes('pageSize=25'))).toBe(true)
  expect(screen.getByLabelText('表示件数')).toHaveValue('20')
  await user.selectOptions(screen.getByLabelText('表示件数'), '50')
  await waitFor(() => expect(location()).toContain('pageSize=50'))
  expect(location()).toContain('page=1')
  await waitFor(() => expect(calls.some(call => call.url.includes('pageSize=50'))).toBe(true))
  await user.type(screen.getByLabelText('コード・名称で検索'), 'L'); await user.click(screen.getByRole('button', { name: '検索' }))
  await waitFor(() => expect(location()).toMatch(/q=L.*pageSize=50|pageSize=50.*q=L/))
  await user.click(screen.getByRole('button', { name: 'クリア' }))
  await waitFor(() => expect(location()).toBe('/production-lines?pageSize=50'))
})

it('shows the pager only for more than one page and moves past-the-end pages to the last page', async () => {
  const user = userEvent.setup()
  setup('/production-lines?page=9&pageSize=20', ['Operator'], undefined, url => {
    const current = Number(new URL(url, 'http://x').searchParams.get('page'))
    return json({ items: current > 3 ? [] : [line], total: 45, page: current, pageSize: 20 })
  })
  await waitFor(() => expect(location()).toContain('page=3'))
  const pager = await screen.findByRole('navigation', { name: '生産ラインのページ切替' })
  expect(within(pager).getByText('3 / 3')).toBeInTheDocument()
  expect(within(pager).getByRole('button', { name: '次へ' })).toBeDisabled()
  await user.click(within(pager).getByRole('button', { name: '前へ' }))
  await waitFor(() => expect(location()).toContain('page=2'))
})

it.each([
  ['/production-lines', '生産ラインはまだ登録されていません。', false],
  ['/production-lines?q=none', '該当する生産ラインはありません。', true],
])('distinguishes no lines from no match (%s) (TC-423)', async (path, text, hint) => {
  setup(path, ['Operator'], undefined, () => json({ items: [], total: 0, page: 1, pageSize: 20 }))
  expect(await screen.findByText(text)).toBeInTheDocument()
  expect(Boolean(screen.queryByText('検索条件を変更するか、クリアしてください。'))).toBe(hint)
  expect(screen.queryByLabelText('表示件数')).not.toBeInTheDocument()
})

it('blocks malformed URL without a feature request and denies roles without protected rows', () => {
  const view = setup('/production-lines?page=10001')
  expect(screen.getByRole('alert')).toHaveTextContent('入力内容を確認してください。')
  expect(view.calls).toHaveLength(0); view.unmount()
  const forbidden = setup('/production-lines', [])
  expect(screen.getByRole('alert')).toHaveTextContent('この機能を利用する権限がありません。')
  expect(screen.getByRole('link', { name: 'ダッシュボードへ戻る' })).toBeInTheDocument()
  expect(forbidden.calls).toHaveLength(0)
})

it('retirement Cancel and Escape preserve invoker focus and send no mutation; the dialog names the line', async () => {
  const user = userEvent.setup(); const { calls } = setup()
  const retire = (await screen.findAllByRole('button', { name: 'LINE-1を使用停止' }))[0]
  await user.click(retire)
  const dialog = screen.getByRole('dialog', { name: 'このラインを使用停止にしますか？' })
  expect(dialog).toHaveTextContent('LINE-1 ／ ライン')
  expect(dialog).toHaveTextContent('過去の製造指示は保持されます。')
  expect(within(dialog).getByRole('button', { name: 'キャンセル' })).toHaveFocus()
  fireEvent(dialog, new Event('cancel', { cancelable: true }))
  await waitFor(() => expect(retire).toHaveFocus())
  expect(calls.filter(call => call.method !== 'GET')).toHaveLength(0)
})

it('shows the register form with required marks, hints and Save before Cancel (TC-424)', async () => {
  const { container } = setup('/production-lines/new')
  expect(await screen.findByRole('heading', { level: 1, name: '生産ライン登録' })).toHaveFocus()
  expect(screen.getByRole('heading', { name: '基本情報' })).toBeInTheDocument()
  for (const label of ['ラインコード', 'ライン名', '稼働時間／日']) expect(screen.getByLabelText(label)).toHaveAttribute('aria-required', 'true')
  expect(screen.getAllByText('*')).toHaveLength(3)
  expect(screen.getByLabelText('ラインコード')).toHaveAccessibleDescription('登録後は変更できません。')
  expect(screen.getByLabelText('稼働時間／日')).toHaveAccessibleDescription('0より大きく24以下、小数3桁まで')
  expect(screen.getByText('製品はまだ追加されていません。')).toBeInTheDocument()
  const save = screen.getByRole('button', { name: '保存' }); const cancel = screen.getByRole('link', { name: 'キャンセル' })
  expect(save.compareDocumentPosition(cancel) & Node.DOCUMENT_POSITION_FOLLOWING).toBeTruthy()
  expect(await axe(container)).toHaveNoViolations()
})

it('shows the edit form with a read-only code and a retired-line notice', async () => {
  setup(`/production-lines/${retiredLine.id}/edit`, ['Operator'], undefined, url => json(url.includes('product-choices') ? { items: [], total: 0, page: 1, pageSize: 20 } : retiredLine))
  expect(await screen.findByRole('heading', { level: 1, name: '生産ライン編集' })).toBeInTheDocument()
  expect(await screen.findByLabelText('ラインコード')).toHaveAttribute('readonly')
  expect(screen.getByText('（変更不可）')).toBeInTheDocument()
  expect(screen.getByText('このラインは使用停止中です。ライン名と稼働時間は修正できますが、使用中には戻せません。')).toBeInTheDocument()
})

it('adds a product through the dialog, which confirms its unit, and sends exact text without numeric conversion (TC-425)', async () => {
  const user = userEvent.setup(); const { calls, container } = setup('/production-lines/new')
  expect(screen.getByRole('button', { name: '保存' })).toBeDisabled()
  await user.type(screen.getByLabelText('ラインコード'), ' NEW ')
  await user.type(screen.getByLabelText('ライン名'), '新規ライン')
  await user.type(screen.getByLabelText('稼働時間／日'), '7.500')
  await addProduct(user)
  await waitFor(() => expect(screen.getByRole('textbox', { name: timing() })).toHaveFocus())
  expect(screen.getByText('追加予定')).toBeInTheDocument()
  expect(screen.queryByRole('checkbox')).not.toBeInTheDocument()
  await user.type(screen.getByRole('textbox', { name: timing() }), '0.001')
  expect(await axe(container)).toHaveNoViolations()
  await user.click(screen.getByRole('button', { name: '保存' }))
  await waitFor(() => expect(calls.some(call => call.method === 'POST')).toBe(true))
  expect(calls.find(call => call.method === 'POST')?.body).toEqual({ code: 'NEW', name: '新規ライン', workingHoursPerDay: '7.500', products: [{ productId: product.id, minutesPerUnit: '0.001', expectedUnit: 'kg', expectedUnitRevision: '9007199254740993', confirmUnit: true }] })
})

it('reads add-dialog choices 20 at a time, hides products already in the draft and Escape changes nothing (TC-426)', async () => {
  const user = userEvent.setup()
  const second = { ...product, id: '0197e4a0-0000-7000-8000-000000001002', sku: 'SECOND-P' }
  const { calls } = setup('/production-lines/new', ['Operator'], undefined, () => json({ items: [product, second], total: 2, page: 1, pageSize: 20 }))
  await addProduct(user)
  const opener = screen.getByRole('button', { name: '製品を追加' })
  await user.click(opener)
  const dialog = await screen.findByRole('dialog', { name: '製品を追加' })
  expect(await within(dialog).findByRole('radio', { name: /SECOND-P/ })).toBeInTheDocument()
  expect(within(dialog).queryByRole('radio', { name: /TEST-P/ })).not.toBeInTheDocument()
  expect(within(dialog).getByRole('region', { name: '製品の候補' })).toBeInTheDocument()
  expect(within(dialog).queryByText(/再登録できません/)).not.toBeInTheDocument()
  expect(calls.filter(call => call.url.includes('/product-choices?')).every(call => call.url.includes('pageSize=20'))).toBe(true)
  fireEvent(dialog, new Event('cancel', { cancelable: true }))
  await waitFor(() => expect(opener).toHaveFocus())
  expect(screen.getAllByRole('textbox', { name: /の製造時間/ })).toHaveLength(1)
})

it('requires 「現在の単位で確認」 before saving changed minutes on a stale pair, then sends confirmUnit true (TC-427)', async () => {
  const user = userEvent.setup()
  const stale = pair({ ...product, unit: 'm', unitRevision: '9007199254740995' }, { confirmedUnit: 'kg', requiresUnitConfirmation: true })
  const { calls } = setup(`/production-lines/${line.id}/edit`, ['Operator'], undefined, (url, method) => method === 'PUT' ? json(line)
    : json(url.includes('product-choices') ? { items: [], total: 0, page: 1, pageSize: 20 } : { ...line, pairs: { items: [stale], total: 1, page: 1, pageSize: 20 } }))
  const minutes = await screen.findByRole('textbox', { name: timing('TEST-P', 'm') })
  expect(screen.getByText('単位の再確認が必要です。')).toBeInTheDocument()
  expect(screen.getByText(/確認済み：kg\s*現在：m/)).toBeInTheDocument()
  await user.clear(minutes); await user.type(minutes, '2')
  await user.click(screen.getByRole('button', { name: '保存' }))
  expect(await screen.findAllByText(/現在の単位で確認してください。/)).not.toHaveLength(0)
  expect(calls.filter(call => call.method === 'PUT')).toHaveLength(0)
  await user.click(screen.getByRole('button', { name: '現在の単位で確認' }))
  expect(screen.getByText('m で確認済み（保存後に反映）')).toBeInTheDocument()
  await user.click(screen.getByRole('button', { name: '保存' }))
  await waitFor(() => expect(calls.some(call => call.method === 'PUT')).toBe(true))
  expect(calls.find(call => call.method === 'PUT')?.body).toMatchObject({ productChanges: [{ action: 'setTiming', minutesPerUnit: '2', expectedUnit: 'm', confirmUnit: true }] })
})

it('clears a reconfirmation when the minutes change again, including a change back', async () => {
  const user = userEvent.setup()
  const stale = pair({ ...product, unit: 'm', unitRevision: '9007199254740995' }, { confirmedUnit: 'kg', requiresUnitConfirmation: true })
  setup(`/production-lines/${line.id}/edit`, ['Operator'], undefined, url => json(url.includes('product-choices') ? { items: [], total: 0, page: 1, pageSize: 20 } : { ...line, pairs: { items: [stale], total: 1, page: 1, pageSize: 20 } }))
  const minutes = await screen.findByRole('textbox', { name: timing('TEST-P', 'm') })
  await user.click(screen.getByRole('button', { name: '現在の単位で確認' }))
  await user.clear(minutes); await user.type(minutes, '0.250')
  expect(screen.getByRole('button', { name: '現在の単位で確認' })).toBeInTheDocument()
  await user.click(screen.getByRole('button', { name: '現在の単位で確認' })); await user.clear(minutes); await user.type(minutes, '1')
  expect(screen.getByRole('button', { name: '現在の単位で確認' })).toBeInTheDocument()
})

it('stages a pair retirement, can undo it, and sends it only on Save (TC-428)', async () => {
  const user = userEvent.setup()
  const { calls } = setup(`/production-lines/${line.id}/edit`, ['Operator'], undefined, (url, method) => method === 'PUT' ? json(line)
    : json(url.includes('product-choices') ? { items: [], total: 0, page: 1, pageSize: 20 } : { ...line, pairs: { items: [pair(product)], total: 1, page: 1, pageSize: 20 } }))
  await user.click(await screen.findByRole('button', { name: 'TEST-Pを使用停止' }))
  const dialog = screen.getByRole('dialog', { name: 'この製品を使用停止にしますか？' })
  expect(dialog).toHaveTextContent('TEST-P ／ 製品')
  expect(dialog).toHaveTextContent('保存後は、この製品をこのラインに再登録できません。')
  await user.click(within(dialog).getByRole('button', { name: '使用停止' }))
  expect(screen.getByText('使用停止予定（保存後に反映）')).toBeInTheDocument()
  expect(calls.filter(call => call.method !== 'GET')).toHaveLength(0)
  await user.click(screen.getByRole('button', { name: '取り消す' }))
  expect(screen.queryByText('使用停止予定（保存後に反映）')).not.toBeInTheDocument()
  expect(screen.getByRole('button', { name: '保存' })).toBeDisabled()
  await user.click(screen.getByRole('button', { name: 'TEST-Pを使用停止' }))
  await user.click(within(screen.getByRole('dialog', { name: 'この製品を使用停止にしますか？' })).getByRole('button', { name: '使用停止' }))
  await user.click(screen.getByRole('button', { name: '保存' }))
  await waitFor(() => expect(calls.find(call => call.method === 'PUT')?.body).toMatchObject({ productChanges: [{ action: 'retire', productId: product.id }] }))
})

it('explains that a saved retired pair cannot be registered again, on the row and in the add dialog (TC-435)', async () => {
  const user = userEvent.setup()
  const retired = pair(product, { isActive: false })
  const { container } = setup(`/production-lines/${line.id}/edit`, ['Operator'], undefined, url => json(url.includes('product-choices') ? { items: [], total: 0, page: 1, pageSize: 20 } : { ...line, pairs: { items: [retired], total: 1, page: 1, pageSize: 20 } }))
  expect(await screen.findByText('再登録できません')).toBeInTheDocument()
  expect(screen.queryByRole('button', { name: 'TEST-Pを使用停止' })).not.toBeInTheDocument()
  expect(screen.queryByRole('textbox', { name: timing() })).not.toBeInTheDocument()
  await user.click(screen.getByRole('button', { name: '製品を追加' }))
  const dialog = await screen.findByRole('dialog', { name: '製品を追加' })
  expect(await within(dialog).findByText('選択できる製品はありません。')).toBeInTheDocument()
  expect(dialog).toHaveAccessibleDescription(/使用停止にした製品は再登録できません。/)
  expect(await axe(container)).toHaveNoViolations()
})

it.each(['stale', 'lost'] as const)('retains edit values and blocks replay after %s until explicit verification/discard', async failure => {
  const user = userEvent.setup(); const { calls } = setup(`/production-lines/${line.id}/edit`, ['Operator'], failure)
  const name = await screen.findByLabelText('ライン名'); await user.clear(name); await user.type(name, '保持する入力')
  expect(screen.getByLabelText('ラインコード')).toHaveAttribute('readonly')
  await user.click(screen.getByRole('button', { name: '保存' }))
  await screen.findByText(failure === 'stale' ? '変更が競合しています。' : '保存結果を確認できません。再送信せず、現在の状態を確認してください。')
  if (failure === 'stale') expect(screen.getByText('他の利用者が先に更新しました。入力内容は残っています。')).toBeInTheDocument()
  expect(name).toHaveValue('保持する入力'); expect(screen.getByRole('button', { name: '保存' })).toBeDisabled()
  expect(calls.filter(call => call.method === 'PUT')).toHaveLength(1)
  await user.click(screen.getByRole('button', { name: '再読み込み' }))
  const dialog = screen.getByRole('dialog', { name: '変更を破棄しますか？' })
  await user.click(within(dialog).getByRole('button', { name: '編集を続ける' }))
  expect(name).toHaveValue('保持する入力')
})

it('reads pairs 20 at a time, preserves off-page timing intents and reveals server indexed errors by stable product id (TC-429)', async () => {
  const user = userEvent.setup()
  const second = { ...product, id: '0197e4a0-0000-7000-8000-000000001002', sku: 'SECOND-P' }
  const { calls } = setup(`/production-lines/${line.id}/edit`, ['Operator'], undefined, (url, method) => {
    if (method === 'PUT') return json({ code: 'VALIDATION', errors: { 'productChanges[0].minutesPerUnit': ['VALIDATION'] } }, 400)
    return json(url.includes('product-choices') ? { items: [], total: 0, page: 1, pageSize: 20 }
      : { ...line, pairs: { items: [pair(url.includes('pairsPage=2') ? second : product)], total: 21, page: url.includes('pairsPage=2') ? 2 : 1, pageSize: 20 } })
  })
  const firstInput = await screen.findByRole('textbox', { name: timing() })
  expect(calls.filter(call => call.url.includes('pairsPage=')).every(call => call.url.includes('pairsPageSize=20'))).toBe(true)
  await user.clear(firstInput); await user.type(firstInput, '0.125')
  const paging = screen.getByRole('navigation', { name: '製品のページ切替' })
  expect(within(paging).getByText('1 / 2')).toBeInTheDocument()
  await user.click(within(paging).getByRole('button', { name: '次へ' }))
  await screen.findByText('SECOND-P')
  await user.click(screen.getByRole('button', { name: '保存' }))
  await waitFor(() => expect(screen.getByRole('textbox', { name: timing() })).toHaveFocus())
  const revealed = screen.getByRole('textbox', { name: timing() })
  expect(revealed).toHaveValue('0.125'); expect(revealed).toHaveAttribute('aria-invalid', 'true')
  expect(calls.find(call => call.method === 'PUT')?.body).toMatchObject({ productChanges: [{ action: 'setTiming', productId: product.id, minutesPerUnit: '0.125', confirmUnit: false }] })
})

it('refreshes unit observations on revisiting a page and asks for reconfirmation without losing timing', async () => {
  const user = userEvent.setup(); let firstReads = 0
  setup(`/production-lines/${line.id}/edit`, ['Operator'], undefined, (url) => {
    let value: unknown = { items: [], total: 0, page: 1, pageSize: 20 }
    if (!url.includes('product-choices')) {
      const page = url.includes('pairsPage=2') ? 2 : 1
      if (page === 1) firstReads++
      value = { ...line, pairs: { items: page === 2 ? [] : [pair(firstReads > 1 ? { ...product, unit: 'm', unitRevision: '9007199254740994' } : product, { confirmedUnit: 'kg', confirmedUnitRevision: product.unitRevision, requiresUnitConfirmation: firstReads > 1 })], total: 21, page, pageSize: 20 } }
    }
    return json(value)
  })
  const minutes = await screen.findByRole('textbox', { name: timing() })
  await user.clear(minutes); await user.type(minutes, '0.125')
  const paging = screen.getByRole('navigation', { name: '製品のページ切替' })
  await user.click(within(paging).getByRole('button', { name: '次へ' }))
  await waitFor(() => expect(screen.queryByRole('textbox', { name: /の製造時間/ })).not.toBeInTheDocument())
  await user.click(within(paging).getByRole('button', { name: '前へ' }))
  expect(await screen.findByRole('textbox', { name: timing('TEST-P', 'm') })).toHaveValue('0.125')
  expect(screen.getByRole('button', { name: '現在の単位で確認' })).toBeInTheDocument()
})

it('blocks a changed parent version on a new pair page without merging or losing the draft', async () => {
  const user = userEvent.setup()
  setup(`/production-lines/${line.id}/edit`, ['Operator'], undefined, url => json(url.includes('product-choices') ? { items: [], total: 0, page: 1, pageSize: 20 } : { ...line, version: url.includes('pairsPage=2') ? '12' : line.version, pairs: { items: [], total: 21, page: url.includes('pairsPage=2') ? 2 : 1, pageSize: 20 } }))
  const name = await screen.findByLabelText('ライン名'); await user.clear(name); await user.type(name, 'keep draft')
  await user.click(within(screen.getByRole('navigation', { name: '製品のページ切替' })).getByRole('button', { name: '次へ' }))
  await screen.findByText('変更が競合しています。')
  expect(name).toHaveValue('keep draft'); expect(screen.getByRole('button', { name: '保存' })).toBeDisabled()
})

it('removes an unsaved pair locally and does not send a retirement request', async () => {
  const user = userEvent.setup(); const { calls } = setup('/production-lines/new')
  await addProduct(user)
  await user.click(screen.getByRole('button', { name: '削除' }))
  expect(screen.queryByRole('textbox', { name: /の製造時間/ })).not.toBeInTheDocument()
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
  expect(screen.getAllByText(/ライン名は200文字以内で入力してください。/)).not.toHaveLength(0)
  expect(screen.getByRole('alert')).toHaveTextContent('入力内容を確認してください。')
  expect(calls.every(call => call.method === 'GET')).toBe(true)
})

it('ignores a late list response after applying a new filter and shows read errors without fabricated empty rows', async () => {
  const user = userEvent.setup(); let release: ((value: Response) => void) | undefined
  const view = setup('/production-lines', ['Operator'], undefined, url => {
    if (!url.includes('q=fresh')) return new Promise<Response>(resolve => { release = resolve })
    return json({ items: [{ ...line, code: 'FRESH' }], total: 1, page: 1, pageSize: 20 })
  })
  await waitFor(() => expect(release).toBeDefined())
  await user.type(screen.getByLabelText('コード・名称で検索'), 'fresh'); await user.click(screen.getByRole('button', { name: '検索' }))
  await screen.findByRole('rowheader', { name: 'FRESH' })
  release?.(json({ items: [line], total: 1, page: 1, pageSize: 20 }))
  await waitFor(() => expect(screen.queryByRole('rowheader', { name: 'LINE-1' })).not.toBeInTheDocument())
  view.unmount()
  setup('/production-lines', ['Operator'], undefined, () => new Response(null, { status: 500 }))
  await screen.findByText('読み込みに失敗しました。')
  expect(screen.queryByText('該当する生産ラインはありません。')).not.toBeInTheDocument()
  expect(screen.queryByText('生産ラインはまだ登録されていません。')).not.toBeInTheDocument()
})

it('prevents repeat submit and navigation while a mutation is pending', async () => {
  const user = userEvent.setup(); let release: ((value: Response) => void) | undefined
  const { calls } = setup('/production-lines/new', ['Operator'], undefined, (url, method) => method === 'POST' ? new Promise<Response>(resolve => { release = resolve }) : json({ items: [], total: 0, page: 1, pageSize: 20 }))
  await user.type(screen.getByLabelText('ラインコード'), 'pending'); await user.type(screen.getByLabelText('ライン名'), 'fixture'); await user.type(screen.getByLabelText('稼働時間／日'), '8')
  await user.click(screen.getByRole('button', { name: '保存' }))
  expect(screen.getByRole('button', { name: '保存中…' })).toBeDisabled()
  const form = screen.getByRole('button', { name: '保存中…' }).closest('form')
  if (!form) throw new Error('Pending form missing')
  fireEvent.submit(form)
  await user.click(screen.getByRole('link', { name: 'キャンセル' }))
  expect(screen.getByLabelText('ラインコード')).toHaveValue('pending')
  expect(calls.filter(call => call.method === 'POST')).toHaveLength(1)
  release?.(json(line, 201))
  await screen.findByRole('heading', { name: '生産ライン・工程' })
  expect(await screen.findByText('生産ラインを保存しました。')).toBeInTheDocument()
})
