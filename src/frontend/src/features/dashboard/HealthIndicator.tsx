import { DatabaseIcon, iconProps, ServerIcon } from '../../components/icons'
import { formatInZone } from './dashboardFormat'
import { useSystemHealth, type DatabaseStatus, type ServerStatus } from './useSystemHealth'
import { labels } from '../production-orders/messages'

const h = labels.dashboard.health
const serverText: Record<ServerStatus, string> = { ok: h.ok, unreachable: h.unreachable }
const databaseText: Record<DatabaseStatus, string> = { ok: h.ok, unavailable: h.unavailable, unknown: h.unknown }

/** Shape and colour both carry the state (M-20): filled for OK, a cross for a failure, hollow for unknown. */
function Dot({ state }: { state: 'ok' | 'bad' | 'unknown' }) {
  const glyph = state === 'ok' ? '●' : state === 'bad' ? '✕' : '○'
  const colour = state === 'ok' ? 'text-green-700' : state === 'bad' ? 'text-red-700' : 'text-gray-500'
  return <span aria-hidden="true" className={`text-[11px] ${colour}`}>{glyph}</span>
}

// Item 26 (003_BD HS-01–HS-05, M-20; 003_DD module 12). The status text is a polite live region, so a change is
// announced once; the check time sits outside it, so an unchanged repeat is silent.
export function HealthIndicator({ timeZone }: { timeZone: string | null }) {
  const health = useSystemHealth()
  if (health.phase === 'forbidden') return null

  const serverState = health.server === 'ok' ? 'ok' : 'bad'
  const databaseState = health.database === 'ok' ? 'ok' : health.database === 'unavailable' ? 'bad' : 'unknown'
  // Plant time needs the plant zone, which arrives with the snapshot; until then the time is left out.
  const checked = health.lastAnswered && timeZone ? formatInZone(health.lastAnswered, timeZone, 'time') : null

  return (
    <div className="flex flex-wrap items-center gap-x-3.5 gap-y-1 rounded-lg border border-gray-200 bg-white px-3 py-1 text-xs text-gray-700 sm:rounded-full">
      <div role="status" className="flex flex-wrap items-center gap-x-3.5 gap-y-1">
        {health.phase === 'checking' ? (
          <span>{h.checking}</span>
        ) : (
          <>
            <span className="inline-flex items-center gap-1">
              <ServerIcon {...iconProps} size={14} className="text-gray-500" />
              <Dot state={serverState} /> {h.server}：{serverText[health.server]}
            </span>
            <span className="inline-flex items-center gap-1">
              <DatabaseIcon {...iconProps} size={14} className="text-gray-500" />
              <Dot state={databaseState} /> {h.database}：{databaseText[health.database]}
            </span>
          </>
        )}
      </div>
      {checked && (
        <span className="text-gray-500 tabular-nums">
          {health.server === 'ok' ? h.checked(checked) : h.lastAnswered(checked)}
        </span>
      )}
    </div>
  )
}
