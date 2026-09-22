import { fireEvent, screen, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { Route, Routes } from 'react-router-dom'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { AppHeader } from '../../../src/components/AppHeader'
import { NavigationGuardProvider } from '../../../src/components/NavigationGuardProvider'
import { ProductionOrderPage } from '../../../src/features/production-orders/ProductionOrderPage'
import { renderWithAuth, stubFetch } from './harness'

afterEach(() => vi.unstubAllGlobals())

describe('AppNavbar (BD-003 H-1–H-5, TC-222)', () => {
  it.each([
    ['/', 'Dashboard'],
    ['/production-orders', 'Production orders'],
    ['/production-orders/o1', 'Production orders'],
    ['/production-orders/new', 'New production order'],
  ])('on %s marks %s as the current page', (path, current) => {
    renderWithAuth(<AppHeader />, path)

    const nav = screen.getByRole('navigation', { name: 'Main' })
    const links = within(nav).getAllByRole('link')
    expect(links.map((l) => l.textContent)).toEqual(['Dashboard', 'Production orders', 'New production order'])
    expect(within(nav).getByRole('link', { name: current })).toHaveAttribute('aria-current', 'page')
    expect(links.filter((l) => l.getAttribute('aria-current') === 'page')).toHaveLength(1)
  })

  it('opens the SP menu, closes it on Escape and returns focus to Menu', async () => {
    const user = userEvent.setup()
    renderWithAuth(<AppHeader />, '/')
    const menu = screen.getByRole('button', { name: 'Menu' })
    expect(menu).toHaveAttribute('aria-expanded', 'false')

    await user.click(menu)
    expect(screen.getByRole('button', { name: 'Close' })).toHaveAttribute('aria-expanded', 'true')
    const panel = document.getElementById('app-menu')!
    expect(within(panel).getByRole('button', { name: 'Sign out' })).toBeInTheDocument()

    fireEvent.keyDown(within(panel).getByRole('link', { name: 'Dashboard' }), { key: 'Escape' })
    expect(document.getElementById('app-menu')).toBeNull()
    expect(screen.getByRole('button', { name: 'Menu' })).toHaveFocus()
  })

  it('navigates from a navbar link and closes the SP menu', async () => {
    const user = userEvent.setup()
    renderWithAuth(<AppHeader />, '/')
    await user.click(screen.getByRole('button', { name: 'Menu' }))

    await user.click(within(document.getElementById('app-menu')!).getByRole('link', { name: 'Production orders' }))

    expect(screen.getByTestId('location')).toHaveTextContent('/production-orders')
    expect(document.getElementById('app-menu')).toBeNull()
  })
})

describe('Navigation guard on an edited Screen A form (DEC-022, TC-223)', () => {
  const products = [{ id: 'p1', sku: 'P-1001', name: 'Steel bracket' }]

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

  const nav = () => screen.getByRole('navigation', { name: 'Main' })

  it('leaves at once when nothing was edited', async () => {
    const user = userEvent.setup()
    renderCreateForm()
    await screen.findByRole('button', { name: 'Save' })

    await user.click(within(nav()).getByRole('link', { name: 'Dashboard' }))

    expect(screen.getByTestId('location')).toHaveTextContent(/^\/$/)
  })

  it('asks first; Keep editing stays with the values; Discard goes to the clicked link', async () => {
    const user = userEvent.setup()
    renderCreateForm()
    await user.type(await screen.findByLabelText(/Quantity/), '12')

    await user.click(within(nav()).getByRole('link', { name: 'Dashboard' }))
    let dialog = screen.getByRole('dialog', { name: 'Discard your changes?' })
    await user.click(within(dialog).getByRole('button', { name: 'Keep editing' }))
    expect(screen.getByTestId('location')).toHaveTextContent('/production-orders/new')
    expect(screen.getByLabelText(/Quantity/)).toHaveValue('12')

    await user.click(within(nav()).getByRole('link', { name: 'Production orders' }))
    dialog = screen.getByRole('dialog', { name: 'Discard your changes?' })
    await user.click(within(dialog).getByRole('button', { name: 'Discard' }))
    expect(screen.getByTestId('location')).toHaveTextContent(/^\/production-orders$/)
  })

  it('resets the form when the clicked link is the page itself', async () => {
    const user = userEvent.setup()
    renderCreateForm()
    await user.type(await screen.findByLabelText(/Quantity/), '12')

    await user.click(within(nav()).getByRole('link', { name: 'New production order' }))
    await user.click(within(screen.getByRole('dialog')).getByRole('button', { name: 'Discard' }))

    expect(screen.getByTestId('location')).toHaveTextContent('/production-orders/new')
    expect(screen.getByLabelText(/Quantity/)).toHaveValue('')
  })

  it('never intercepts a modified click (new tab)', async () => {
    const user = userEvent.setup()
    renderCreateForm()
    await user.type(await screen.findByLabelText(/Quantity/), '12')

    fireEvent.click(within(nav()).getByRole('link', { name: 'Dashboard' }), { ctrlKey: true })

    expect(screen.queryByRole('dialog')).not.toBeInTheDocument()
  })
})
