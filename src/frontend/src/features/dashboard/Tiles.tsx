import type { ComponentType, ReactNode } from 'react'
import {
  CompletedMonthIcon,
  CompletedWeekIcon,
  iconProps,
  LeadTimeIcon,
  OnTimeIcon,
  StatusCancelledIcon,
  StatusCompletedIcon,
  StatusDraftIcon,
  StatusInProgressIcon,
  StatusTotalIcon,
} from '../../components/icons'
import { formatDate } from '../../lib/format'
import { labels, message, statusLabels } from '../production-orders/messages'
import { formatLeadTime, formatNumber, formatOrders, formatRate, formatUnits } from './dashboardFormat'
import type { DashboardSnapshot } from './types'

type IconType = ComponentType<typeof iconProps>

function Tile({ Icon, label, window, value, caption, total }: {
  Icon: IconType
  label: string
  window?: string
  value: string
  caption?: ReactNode
  total?: boolean
}) {
  // Label before value in reading order, so a screen reader hears "Draft, 35" rather than a bare number.
  return (
    <li className={`grid content-start gap-0.5 rounded-lg border border-gray-200 px-3.5 py-2.5 ${total ? 'bg-gray-50' : 'bg-white'}`}>
      <span className="inline-flex items-center gap-1.5 text-xs text-gray-600">
        <Icon {...iconProps} />
        {label}
      </span>
      {window && <span className="text-xs text-gray-500">{window}</span>}
      <span className="text-2xl leading-tight font-medium text-gray-900 tabular-nums">{value}</span>
      {caption && <span className="text-xs text-gray-500 tabular-nums">{caption}</span>}
    </li>
  )
}

/** Items 7–11 (003_BD M-11). */
export function StatusTiles({ counts }: { counts: DashboardSnapshot['statusCounts'] }) {
  return (
    <ul className="grid grid-cols-2 gap-2.5 sm:grid-cols-5" aria-label={labels.dashboard.byStatus}>
      <Tile Icon={StatusTotalIcon} label={labels.dashboard.total} value={formatNumber(counts.total)} total />
      <Tile Icon={StatusDraftIcon} label={statusLabels.Draft} value={formatNumber(counts.draft)} />
      <Tile Icon={StatusInProgressIcon} label={statusLabels.InProgress} value={formatNumber(counts.inProgress)} />
      <Tile Icon={StatusCompletedIcon} label={statusLabels.Completed} value={formatNumber(counts.completed)} />
      <Tile Icon={StatusCancelledIcon} label={statusLabels.Cancelled} value={formatNumber(counts.cancelled)} />
    </ul>
  )
}

/** Items 12–15 (003_BD D-05–D-07, D-09, M-13, M-14, M-18). */
export function DeliveryTiles({ snapshot }: { snapshot: DashboardSnapshot }) {
  const { completedThisWeek: week, completedThisMonth: month, onTime, leadTime } = snapshot
  const none = message('MSG-I008')
  return (
    <ul className="grid grid-cols-2 gap-2.5 sm:grid-cols-4" aria-label={labels.dashboard.delivery}>
      <Tile Icon={CompletedWeekIcon} label={labels.dashboard.completedThisWeek} window={labels.dashboard.sinceMonday(formatDate(week.from))}
        value={formatOrders(week.orderCount)} caption={formatUnits(week.quantity)} />
      <Tile Icon={CompletedMonthIcon} label={labels.dashboard.completedThisMonth} window={labels.dashboard.since(formatDate(month.from))}
        value={formatOrders(month.orderCount)} caption={formatUnits(month.quantity)} />
      <Tile Icon={OnTimeIcon} label={labels.dashboard.onTime} window={labels.dashboard.last30Days}
        value={formatRate(onTime.onTimeCount, onTime.completedCount)}
        caption={onTime.completedCount === 0 ? none : labels.dashboard.onTimeCaption(onTime.onTimeCount, onTime.completedCount)} />
      <Tile Icon={LeadTimeIcon} label={labels.dashboard.leadTime} window={labels.dashboard.last30Days}
        value={formatLeadTime(leadTime.averageDays)}
        caption={leadTime.averageDays === null ? none : labels.dashboard.basedOn(formatOrders(leadTime.orderCount))} />
    </ul>
  )
}
