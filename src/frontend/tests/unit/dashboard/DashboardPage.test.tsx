import { screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { axe } from 'vitest-axe'
import { DashboardPage } from '../../../src/features/dashboard/DashboardPage'
import { emptySnapshot, snapshot } from './fixtures'
import { calls, renderWithAuth, stubFetch } from './harness'

const healthy = { status: 200, body: { database: 'ok', checkedAt: '2026-09-22T05:05:30Z' } }

afterEach(() => vi.unstubAllGlobals())

function renderDashboard(dashboard: Parameters<typeof stubFetch>[0][string] = { status: 200, body: snapshot() }, roles?: string[]) {
  stubFetch({ 'GET /api/dashboard': dashboard, 'GET /api/system/health': healthy })
  return renderWithAuth(<DashboardPage />, '/', roles)
}

describe('DashboardPage (SCR-003)', () => {
  it('renders every widget from one snapshot, in plant time, with no axe violations (TC-201, TC-217)', async () => {
    const { container } = renderDashboard()

    expect(await screen.findByText('As of 2026-09-22 14:05 (Asia/Tokyo)')).toBeInTheDocument()
    expect(document.title).toBe('Dashboard — ProductionManagementAI')
    const status = screen.getByRole('list', { name: 'Orders by status' })
    expect(within(status).getByText('124')).toBeInTheDocument()
    expect(within(status).getByText('In progress')).toBeInTheDocument()
    expect(screen.getByText('77%')).toBeInTheDocument()
    expect(screen.getByText('23 of 30 on time')).toBeInTheDocument()
    expect(screen.getByText('13.7 days')).toBeInTheDocument()
    expect(screen.getByRole('heading', { name: 'Overdue (15)' })).toBeInTheDocument()
    expect(screen.getByText('Showing 10 of 15')).toBeInTheDocument()
    expect(screen.getByRole('heading', { name: 'Due in the next 7 days (2)' })).toBeInTheDocument()
    expect(screen.getByRole('img', { name: /^Open workload by due week: Overdue 15, This week 7/ })).toBeInTheDocument()
    expect(screen.getByRole('img', { name: /^Orders completed per week, last 12 weeks: .*This week 3$/ })).toBeInTheDocument()
    expect(await axe(container)).toHaveNoViolations()
  })

  it('is read-only: no widget is a link, and it asks only for the snapshot and health (TC-202)', async () => {
    renderDashboard()
    await screen.findByText('77%')

    expect(within(screen.getByRole('main')).queryAllByRole('link')).toHaveLength(0)
    await waitFor(() => expect(calls).toContain('GET /api/system/health'))
    expect(new Set(calls)).toEqual(new Set(['GET /api/dashboard', 'GET /api/system/health']))
  })

  it('shows zeros, "none" messages and — for an empty system, without an error (TC-215)', async () => {
    renderDashboard({ status: 200, body: emptySnapshot() })

    expect(await screen.findByText('No overdue orders.')).toBeInTheDocument()
    expect(screen.getByText('No orders due in the next 7 days.')).toBeInTheDocument()
    expect(screen.getAllByText('No open orders.')).toHaveLength(2) // top products and the workload chart
    expect(screen.getAllByText('—')).toHaveLength(2)
    expect(screen.getAllByText('No orders completed in the last 30 days.')).toHaveLength(2)
    expect(screen.queryByRole('alert')).not.toBeInTheDocument()
  })

  it('shows the error banner with Retry, no figures, and Retry reloads (TC-216)', async () => {
    const user = userEvent.setup()
    renderDashboard([{ status: 500 }, { status: 200, body: snapshot() }])

    const alert = await screen.findByRole('alert')
    expect(alert).toHaveTextContent('Something went wrong. Try again.')
    expect(screen.queryByText('77%')).not.toBeInTheDocument()

    await user.click(within(alert).getByRole('button', { name: 'Retry' }))

    expect(await screen.findByText('77%')).toBeInTheDocument()
    expect(calls.filter((c) => c === 'GET /api/dashboard')).toHaveLength(2)
  })

  it('shows the permission panel on 403, and nothing without a role (TC-213)', async () => {
    renderDashboard({ status: 403 })
    expect(await screen.findByText("You don't have permission to view the dashboard.")).toBeInTheDocument()
    vi.unstubAllGlobals()
  })

  it('never calls the API for a signed-in user without Admin or Operator (TC-213)', async () => {
    renderDashboard(undefined, [])
    expect(screen.getByText("You don't have permission to view the dashboard.")).toBeInTheDocument()
    expect(calls).toHaveLength(0)
  })

  it('toggles each chart table, and maximizes a chart in a dialog that restores focus (TC-217, TC-227)', async () => {
    const user = userEvent.setup()
    const { container } = renderDashboard()
    await screen.findByText('77%')
    const before = calls.length

    const toggle = screen.getAllByRole('button', { name: 'View as table' })[0]
    await user.click(toggle)
    expect(toggle).toHaveAttribute('aria-expanded', 'true')
    expect(screen.getByRole('table', { name: 'Open workload by due week' })).toBeInTheDocument()
    expect(screen.getByRole('cell', { name: 'before 2026-09-22' })).toBeInTheDocument()

    const expand = screen.getByRole('button', { name: 'Expand Completed per week, last 12 weeks' })
    await user.click(expand)
    const dialog = screen.getByRole('dialog', { name: 'Completed per week, last 12 weeks' })
    const restore = within(dialog).getByRole('button', { name: 'Restore' })
    expect(restore).toHaveFocus()
    expect(within(dialog).getByRole('table')).toBeInTheDocument()
    expect(await axe(container)).toHaveNoViolations()

    await user.click(restore)
    expect(screen.queryByRole('dialog')).not.toBeInTheDocument()
    expect(expand).toHaveFocus()
    expect(calls.slice(before).filter((c) => c === 'GET /api/dashboard')).toHaveLength(0)
  })

  it('keeps every icon decorative, and names every icon-only control (TC-228)', async () => {
    const { container } = renderDashboard()
    await screen.findByText('77%')

    const icons = container.querySelectorAll('svg.lucide')
    expect(icons.length).toBeGreaterThan(15)
    icons.forEach((icon) => expect(icon).toHaveAttribute('aria-hidden', 'true'))
    screen.getAllByRole('button').forEach((button) => expect(button).toHaveAccessibleName())
  })
})
