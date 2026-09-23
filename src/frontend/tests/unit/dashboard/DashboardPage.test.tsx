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

    expect(await screen.findByText('2026/09/22 14:05 時点（Asia/Tokyo）')).toBeInTheDocument()
    expect(document.title).toBe('ダッシュボード — ProductionManagementAI')
    const status = screen.getByRole('list', { name: 'ステータス別の製造指示' })
    expect(within(status).getByText('124')).toBeInTheDocument()
    expect(within(status).getByText('進行中')).toBeInTheDocument()
    expect(screen.getByText('77%')).toBeInTheDocument()
    expect(screen.getByText('30件中 23件が期限内')).toBeInTheDocument()
    expect(screen.getByText('13.7日')).toBeInTheDocument()
    expect(screen.getByRole('heading', { name: '納期遅れ（15件）' })).toBeInTheDocument()
    expect(screen.getByText('15件中 10件を表示')).toBeInTheDocument()
    expect(screen.getByRole('heading', { name: '7日以内に納期（2件）' })).toBeInTheDocument()
    expect(screen.getByRole('img', { name: /^納期週別の未完了作業量：納期遅れ 15件、今週 7件/ })).toBeInTheDocument()
    expect(screen.getByRole('img', { name: /^週別の完了件数（直近12週）：.*今週 3件$/ })).toBeInTheDocument()
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

    expect(await screen.findByText('納期遅れの製造指示はありません。')).toBeInTheDocument()
    expect(screen.getByText('7日以内に納期の製造指示はありません。')).toBeInTheDocument()
    expect(screen.getAllByText('未完了の製造指示はありません。')).toHaveLength(2) // top products and the workload chart
    expect(screen.getAllByText('—')).toHaveLength(2)
    expect(screen.getAllByText('直近30日に完了した製造指示はありません。')).toHaveLength(2)
    expect(screen.queryByRole('alert')).not.toBeInTheDocument()
  })

  it('shows the error banner with Retry, no figures, and Retry reloads (TC-216)', async () => {
    const user = userEvent.setup()
    renderDashboard([{ status: 500 }, { status: 200, body: snapshot() }])

    const alert = await screen.findByRole('alert')
    expect(alert).toHaveTextContent('問題が発生しました。もう一度お試しください。')
    expect(screen.queryByText('77%')).not.toBeInTheDocument()

    await user.click(within(alert).getByRole('button', { name: '再試行' }))

    expect(await screen.findByText('77%')).toBeInTheDocument()
    expect(calls.filter((c) => c === 'GET /api/dashboard')).toHaveLength(2)
  })

  it('shows the permission panel on 403, and nothing without a role (TC-213)', async () => {
    renderDashboard({ status: 403 })
    expect(await screen.findByText('ダッシュボードを閲覧する権限がありません。')).toBeInTheDocument()
    vi.unstubAllGlobals()
  })

  it('never calls the API for a signed-in user without Admin or Operator (TC-213)', async () => {
    renderDashboard(undefined, [])
    expect(screen.getByText('ダッシュボードを閲覧する権限がありません。')).toBeInTheDocument()
    expect(calls).toHaveLength(0)
  })

  it('toggles each chart table, and maximizes a chart in a dialog that restores focus (TC-217, TC-227)', async () => {
    const user = userEvent.setup()
    const { container } = renderDashboard()
    await screen.findByText('77%')
    const before = calls.length

    const toggle = screen.getAllByRole('button', { name: '表で表示' })[0]
    await user.click(toggle)
    expect(toggle).toHaveAttribute('aria-expanded', 'true')
    expect(screen.getByRole('table', { name: '納期週別の未完了作業量' })).toBeInTheDocument()
    expect(screen.getByRole('cell', { name: '2026/09/22より前' })).toBeInTheDocument()

    const expand = screen.getByRole('button', { name: '週別の完了件数（直近12週）を拡大' })
    await user.click(expand)
    const dialog = screen.getByRole('dialog', { name: '週別の完了件数（直近12週）' })
    const restore = within(dialog).getByRole('button', { name: '元に戻す' })
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
