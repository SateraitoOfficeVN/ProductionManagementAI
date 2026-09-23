import { fireEvent, screen, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { Route, Routes } from 'react-router-dom'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { AppHeader } from '../../../src/components/AppHeader'
import { NavigationGuardProvider } from '../../../src/components/NavigationGuardProvider'
import { ProductionOrderPage } from '../../../src/features/production-orders/ProductionOrderPage'
import { renderWithAuth, stubFetch } from './harness'

afterEach(() => vi.unstubAllGlobals())

describe('AppNavbar (003_BD H-1–H-5, TC-222)', () => {
  it.each([
    ['/', 'ダッシュボード'],
    ['/production-orders', '製造指示一覧'],
    ['/production-orders/o1', '製造指示一覧'],
    ['/production-orders/new', '新規製造指示'],
  ])('on %s marks %s as the current page', (path, current) => {
    renderWithAuth(<AppHeader />, path)

    const nav = screen.getByRole('navigation', { name: 'メインメニュー' })
    const links = within(nav).getAllByRole('link')
    expect(links.map((l) => l.textContent)).toEqual(['ダッシュボード', '製造指示一覧', '新規製造指示'])
    expect(within(nav).getByRole('link', { name: current })).toHaveAttribute('aria-current', 'page')
    expect(links.filter((l) => l.getAttribute('aria-current') === 'page')).toHaveLength(1)
  })

  it('opens the SP menu, closes it on Escape and returns focus to Menu', async () => {
    const user = userEvent.setup()
    renderWithAuth(<AppHeader />, '/')
    const menu = screen.getByRole('button', { name: 'メニュー' })
    expect(menu).toHaveAttribute('aria-expanded', 'false')

    await user.click(menu)
    expect(screen.getByRole('button', { name: '閉じる' })).toHaveAttribute('aria-expanded', 'true')
    const panel = document.getElementById('app-menu')!
    expect(within(panel).getByRole('button', { name: 'ログアウト' })).toBeInTheDocument()

    fireEvent.keyDown(within(panel).getByRole('link', { name: 'ダッシュボード' }), { key: 'Escape' })
    expect(document.getElementById('app-menu')).toBeNull()
    expect(screen.getByRole('button', { name: 'メニュー' })).toHaveFocus()
  })

  it('navigates from a navbar link and closes the SP menu', async () => {
    const user = userEvent.setup()
    renderWithAuth(<AppHeader />, '/')
    await user.click(screen.getByRole('button', { name: 'メニュー' }))

    await user.click(within(document.getElementById('app-menu')!).getByRole('link', { name: '製造指示一覧' }))

    expect(screen.getByTestId('location')).toHaveTextContent('/production-orders')
    expect(document.getElementById('app-menu')).toBeNull()
  })
})

describe('Navigation guard on an edited Screen A form (DEC-022, TC-223)', () => {
  const products = [{ id: 'p1', sku: 'P-1001', name: 'ブレーキキャリパー' }]

  function renderCreateForm() {
    stubFetch({ 'GET /api/products': { status: 200, body: products } })
    return renderWithAuth(
      <NavigationGuardProvider>
        {/* ProductionOrderPage renders the shared header itself. */}
        <Routes>
          <Route path="/" element={<div>Dashboard page</div>} />
          <Route path="/production-orders" element={<div>List page</div>} />
          <Route path="/production-orders/new" element={<ProductionOrderPage />} />
        </Routes>
      </NavigationGuardProvider>,
      '/production-orders/new',
    )
  }

  const nav = () => screen.getByRole('navigation', { name: 'メインメニュー' })

  it('leaves at once when nothing was edited', async () => {
    const user = userEvent.setup()
    renderCreateForm()
    await screen.findByRole('button', { name: '保存' })

    await user.click(within(nav()).getByRole('link', { name: 'ダッシュボード' }))

    expect(screen.getByTestId('location')).toHaveTextContent(/^\/$/)
  })

  it('asks first; Keep editing stays with the values; Discard goes to the clicked link', async () => {
    const user = userEvent.setup()
    renderCreateForm()
    await user.type(await screen.findByLabelText(/数量/), '12')

    await user.click(within(nav()).getByRole('link', { name: 'ダッシュボード' }))
    let dialog = screen.getByRole('dialog', { name: '変更を破棄しますか？' })
    await user.click(within(dialog).getByRole('button', { name: '編集を続ける' }))
    expect(screen.getByTestId('location')).toHaveTextContent('/production-orders/new')
    expect(screen.getByLabelText(/数量/)).toHaveValue('12')

    await user.click(within(nav()).getByRole('link', { name: '製造指示一覧' }))
    dialog = screen.getByRole('dialog', { name: '変更を破棄しますか？' })
    await user.click(within(dialog).getByRole('button', { name: '破棄' }))
    expect(screen.getByTestId('location')).toHaveTextContent(/^\/production-orders$/)
  })

  it('resets the form when the clicked link is the page itself', async () => {
    const user = userEvent.setup()
    renderCreateForm()
    await user.type(await screen.findByLabelText(/数量/), '12')

    await user.click(within(nav()).getByRole('link', { name: '新規製造指示' }))
    await user.click(within(screen.getByRole('dialog')).getByRole('button', { name: '破棄' }))

    expect(screen.getByTestId('location')).toHaveTextContent('/production-orders/new')
    expect(screen.getByLabelText(/数量/)).toHaveValue('')
  })

  it('never intercepts a modified click (new tab)', async () => {
    const user = userEvent.setup()
    renderCreateForm()
    await user.type(await screen.findByLabelText(/数量/), '12')

    fireEvent.click(within(nav()).getByRole('link', { name: 'ダッシュボード' }), { ctrlKey: true })

    expect(screen.queryByRole('dialog')).not.toBeInTheDocument()
  })
})
