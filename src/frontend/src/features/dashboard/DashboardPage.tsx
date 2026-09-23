import { useEffect, useState } from 'react'
import { AppHeader } from '../../components/AppHeader'
import { ErrorIcon, ForbiddenIcon, iconProps, RetryIcon, TrendIcon, WorkloadIcon } from '../../components/icons'
import { ApiError } from '../../lib/apiClient'
import { useAuth } from '../auth/useAuth'
import { labels, message } from '../production-orders/messages'
import { AttentionList } from './AttentionList'
import { BarChart, type Bar } from './BarChart'
import { getDashboard } from './dashboardApi'
import { formatDate } from '../../lib/format'
import { bucketLabel, formatInZone, formatNumber, monthDay, weekRange } from './dashboardFormat'
import { HealthIndicator } from './HealthIndicator'
import { DeliveryTiles, StatusTiles } from './Tiles'
import { TopProducts } from './TopProducts'
import type { DashboardSnapshot } from './types'

type State =
  | { kind: 'loading' }
  | { kind: 'ready'; snapshot: DashboardSnapshot }
  | { kind: 'error' }
  | { kind: 'forbidden' }


// SCR-003 at "/" (003_DD module 1; 003_DD-SPD §1–§2). One request produces one snapshot, and every widget renders
// from it; no widget fetches on its own, so the figures cannot disagree (DEC-011).
export function DashboardPage() {
  const { user } = useAuth()
  const allowed = !!user && (user.roles.includes('Admin') || user.roles.includes('Operator'))
  const [state, setState] = useState<State>(allowed ? { kind: 'loading' } : { kind: 'forbidden' })
  const [attempt, setAttempt] = useState(0)

  useEffect(() => {
    document.title = labels.app.title(labels.dashboard.heading)
  }, [])

  useEffect(() => {
    if (!allowed) return
    const controller = new AbortController()
    getDashboard(controller.signal)
      .then((snapshot) => setState({ kind: 'ready', snapshot }))
      .catch((error: unknown) => {
        if (controller.signal.aborted) return
        setState(error instanceof ApiError && error.status === 403 ? { kind: 'forbidden' } : { kind: 'error' })
      })
    return () => controller.abort()
  }, [allowed, attempt])

  const snapshot = state.kind === 'ready' ? state.snapshot : null

  return (
    <div className="min-h-screen bg-white">
      <AppHeader />
      <main className="mx-auto grid max-w-6xl gap-4 px-4 pt-5 pb-10 sm:px-6">
        <div className="flex flex-col gap-2 sm:flex-row sm:items-start sm:justify-between">
          <div className="grid gap-0.5">
            <h1 className="text-xl font-medium text-gray-900">{labels.dashboard.heading}</h1>
            <p role="status" className="text-xs text-gray-500 tabular-nums">
              {snapshot
                ? labels.dashboard.asOf(formatInZone(snapshot.asOf, snapshot.timeZone, 'dateTime'), snapshot.timeZone)
                : state.kind === 'loading'
                  ? labels.dashboard.loading
                  : ''}
            </p>
          </div>
          {state.kind !== 'forbidden' && <HealthIndicator timeZone={snapshot?.timeZone ?? null} />}
        </div>

        {state.kind === 'error' && (
          <div role="alert" className="flex flex-wrap items-center justify-between gap-3 rounded-md border border-red-200 bg-red-50 px-3 py-2.5 text-sm text-red-800">
            <span className="inline-flex items-center gap-1.5">
              <ErrorIcon {...iconProps} />
              {message('MSG-E013')}
            </span>
            <button
              type="button"
              onClick={() => {
                setState({ kind: 'loading' })
                setAttempt((n) => n + 1)
              }}
              className="inline-flex items-center gap-1.5 rounded border border-gray-300 bg-white px-2.5 py-1 text-gray-900 focus-visible:ring-2 focus-visible:ring-gray-900 focus-visible:ring-offset-2 focus-visible:outline-none"
            >
              <RetryIcon {...iconProps} />
              {labels.common.retry}
            </button>
          </div>
        )}

        {state.kind === 'forbidden' && (
          <div className="grid justify-items-center gap-3 rounded-lg border border-gray-200 px-6 py-8 text-center">
            <ForbiddenIcon {...iconProps} size={28} className="text-gray-500" />
            <p className="text-gray-700">{message('MSG-E021')}</p>
          </div>
        )}

        {state.kind === 'loading' && <Skeleton />}

        {snapshot && <Widgets snapshot={snapshot} />}
      </main>
    </div>
  )
}

function Widgets({ snapshot }: { snapshot: DashboardSnapshot }) {
  const workloadBars: Bar[] = snapshot.workload.map((b, i) => ({
    label: bucketLabel(b.kind, b.weekStart, i),
    value: b.orderCount,
    emphasis: i <= 1,
  }))
  const trendBars: Bar[] = snapshot.completionTrend.map((w, i, all) => ({
    label: i === all.length - 1 ? labels.dashboard.thisWeek : monthDay(w.weekStart),
    value: w.orderCount,
    emphasis: i === all.length - 1,
  }))

  return (
    <>
      <h2 className="mt-1 text-sm font-semibold text-gray-700">{labels.dashboard.currentState}</h2>
      <StatusTiles counts={snapshot.statusCounts} />
      <h2 className="mt-1 text-sm font-semibold text-gray-700">{labels.dashboard.delivery}</h2>
      <DeliveryTiles snapshot={snapshot} />

      <div className="grid items-start gap-3.5 lg:grid-cols-[1.5fr_1fr]">
        <AttentionList overdue={snapshot.overdue} dueSoon={snapshot.dueSoon} />
        <TopProducts products={snapshot.topProducts} />
      </div>

      <div className="grid items-start gap-3.5 lg:grid-cols-2">
        <BarChart
          title={labels.dashboard.workload}
          Icon={WorkloadIcon}
          bars={workloadBars}
          summary={labels.dashboard.workloadSummary(workloadBars.map((b) => `${b.label} ${b.value}件`).join('、'))}
          caption={labels.dashboard.workloadCaption(formatNumber(workloadBars.reduce((n, b) => n + b.value, 0)))}
          emptyMessage={message('MSG-I007')}
          tableColumns={[...labels.dashboard.workloadColumns]}
          tableRows={snapshot.workload.map((b, i) => [
            workloadBars[i].label,
            b.kind === 'overdue'
              ? labels.dashboard.before(formatDate(snapshot.today))
              : b.kind === 'later'
                ? labels.dashboard.after(formatDate(snapshot.workload[i - 1]?.weekEnd ?? ''))
                : weekRange(b.weekStart ?? '', b.weekEnd ?? ''),
            formatNumber(b.orderCount),
            formatNumber(b.quantity),
          ])}
        />
        <BarChart
          title={labels.dashboard.trend}
          Icon={TrendIcon}
          bars={trendBars}
          summary={labels.dashboard.trendSummary(trendBars.map((b) => `${b.label} ${b.value}件`).join('、'))}
          caption={labels.dashboard.trendCaption}
          tableColumns={[...labels.dashboard.trendColumns]}
          tableRows={snapshot.completionTrend.map((w, i) => [trendBars[i].label, weekRange(w.weekStart, w.weekEnd), formatNumber(w.orderCount)])}
        />
      </div>
    </>
  )
}

function Skeleton() {
  const bone = 'block rounded bg-gray-100'
  return (
    <div aria-busy="true" aria-label={labels.dashboard.loadingRegion} className="grid gap-4">
      {[5, 4].map((n) => (
        <div key={n} className={`grid grid-cols-2 gap-2.5 ${n === 5 ? 'sm:grid-cols-5' : 'sm:grid-cols-4'}`}>
          {Array.from({ length: n }, (_, i) => (
            <div key={i} className="grid gap-2 rounded-lg border border-gray-200 p-3">
              <span className={`${bone} h-3 w-2/5`} />
              <span className={`${bone} h-6 w-3/5`} />
            </div>
          ))}
        </div>
      ))}
      <div className="grid gap-3.5 lg:grid-cols-2">
        {[0, 1].map((i) => (
          <div key={i} className="grid gap-2 rounded-lg border border-gray-200 p-3">
            <span className={`${bone} h-3 w-2/5`} />
            <span className={`${bone} h-32 w-full`} />
          </div>
        ))}
      </div>
    </div>
  )
}
