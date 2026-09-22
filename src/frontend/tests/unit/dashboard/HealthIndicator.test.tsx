import { act, render, screen } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { HealthIndicator } from '../../../src/features/dashboard/HealthIndicator'
import { POLL_MS } from '../../../src/features/dashboard/useSystemHealth'
import { calls, stubFetch, type Reply } from './harness'

const ok: Reply = { status: 200, body: { database: 'ok', checkedAt: '2026-09-22T05:05:30Z' } }
const down: Reply = { status: 200, body: { database: 'unavailable', checkedAt: '2026-09-22T05:06:00Z' } }

let visibility: DocumentVisibilityState = 'visible'

beforeEach(() => {
  vi.useFakeTimers({ shouldAdvanceTime: false })
  visibility = 'visible'
  vi.spyOn(document, 'visibilityState', 'get').mockImplementation(() => visibility)
})

afterEach(() => {
  vi.useRealTimers()
  vi.restoreAllMocks()
  vi.unstubAllGlobals()
})

const healthCalls = () => calls.filter((c) => c === 'GET /api/system/health').length
const flush = () => act(async () => { await vi.advanceTimersByTimeAsync(0) })

describe('HealthIndicator (BD-003 HS-01–HS-05, TC-226)', () => {
  it('checks at once, then shows the statuses and the check time in plant time', async () => {
    stubFetch({ 'GET /api/system/health': [ok] })
    render(<HealthIndicator timeZone="Asia/Tokyo" />)
    expect(screen.getByRole('status')).toHaveTextContent('Checking…')

    await flush()

    expect(screen.getByRole('status')).toHaveTextContent('Server: OK')
    expect(screen.getByRole('status')).toHaveTextContent('Database: OK')
    expect(screen.getByText('Checked 14:05:30')).toBeInTheDocument()
  })

  it('reports a database the server cannot reach', async () => {
    stubFetch({ 'GET /api/system/health': [down] })
    render(<HealthIndicator timeZone="Asia/Tokyo" />)
    await flush()

    expect(screen.getByRole('status')).toHaveTextContent('Server: OK')
    expect(screen.getByRole('status')).toHaveTextContent('Database: Unavailable')
  })

  it('reports an unreachable server with the database unknown, never its last value', async () => {
    stubFetch({ 'GET /api/system/health': [ok, { status: 503 }] })
    render(<HealthIndicator timeZone="Asia/Tokyo" />)
    await flush()
    expect(screen.getByRole('status')).toHaveTextContent('Database: OK')

    await act(async () => { await vi.advanceTimersByTimeAsync(POLL_MS) })

    expect(screen.getByRole('status')).toHaveTextContent('Server: Unreachable')
    expect(screen.getByRole('status')).toHaveTextContent('Database: Unknown')
    expect(screen.getByText('Last answered 14:05:30')).toBeInTheDocument()
  })

  it('polls every 30 s, chained so checks never overlap', async () => {
    let release: (() => void) | undefined
    stubFetch({
      'GET /api/system/health': () =>
        new Promise<Reply>((resolve) => {
          release = () => resolve(ok)
        }),
    })
    render(<HealthIndicator timeZone="Asia/Tokyo" />)
    await flush()
    expect(healthCalls()).toBe(1)

    // The first check is still in flight: no second one starts, however long it takes.
    await act(async () => { await vi.advanceTimersByTimeAsync(POLL_MS * 3) })
    expect(healthCalls()).toBe(1)

    await act(async () => { release?.(); await vi.advanceTimersByTimeAsync(0) })
    await act(async () => { await vi.advanceTimersByTimeAsync(POLL_MS - 1) })
    expect(healthCalls()).toBe(1)
    await act(async () => { await vi.advanceTimersByTimeAsync(1) })
    expect(healthCalls()).toBe(2)
  })

  it('pauses while the tab is hidden and checks at once when it is visible again', async () => {
    stubFetch({ 'GET /api/system/health': [ok] })
    render(<HealthIndicator timeZone="Asia/Tokyo" />)
    await flush()

    visibility = 'hidden'
    await act(async () => { document.dispatchEvent(new Event('visibilitychange')) })
    await act(async () => { await vi.advanceTimersByTimeAsync(POLL_MS * 4) })
    expect(healthCalls()).toBe(1)

    visibility = 'visible'
    await act(async () => { document.dispatchEvent(new Event('visibilitychange')); await vi.advanceTimersByTimeAsync(0) })
    expect(healthCalls()).toBe(2)
  })

  it('stops polling after unmount', async () => {
    stubFetch({ 'GET /api/system/health': [ok] })
    const { unmount } = render(<HealthIndicator timeZone="Asia/Tokyo" />)
    await flush()
    unmount()

    await act(async () => { await vi.advanceTimersByTimeAsync(POLL_MS * 3) })
    expect(healthCalls()).toBe(1)
  })

  it('leaves out the time until the plant zone is known, and keeps it out of the live region', async () => {
    stubFetch({ 'GET /api/system/health': [ok] })
    const { rerender } = render(<HealthIndicator timeZone={null} />)
    await flush()
    expect(screen.queryByText(/Checked/)).not.toBeInTheDocument()

    rerender(<HealthIndicator timeZone="Asia/Tokyo" />)
    expect(screen.getByText('Checked 14:05:30')).toBeInTheDocument()
    expect(screen.getByRole('status')).not.toHaveTextContent('Checked')
  })
})
