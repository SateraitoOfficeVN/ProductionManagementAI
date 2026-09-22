// Pure formatters for BD-003 M-13, M-14, M-16–M-20 (DD-003 module 6). No date arithmetic: every date printed here
// came from the server, so the browser's own timezone cannot shift a figure.

const numberFormat = new Intl.NumberFormat('en-US')

export const formatNumber = (n: number) => numberFormat.format(n)

export const formatOrders = (n: number) => `${formatNumber(n)} ${n === 1 ? 'order' : 'orders'}`

export const formatUnits = (n: number) => `${formatNumber(n)} ${n === 1 ? 'unit' : 'units'}`

/** M-13: whole percent, rounded half up; "—" when nothing completed. */
export function formatRate(onTime: number, completed: number): string {
  return completed === 0 ? '—' : `${Math.floor((100 * onTime) / completed + 0.5)}%`
}

/** M-14: one decimal (the server already rounded); "—" when nothing completed. */
export function formatLeadTime(averageDays: number | null): string {
  if (averageDays === null) return '—'
  return `${averageDays.toFixed(1)} ${averageDays === 1 ? 'day' : 'days'}`
}

/** "2026-09-28" → "09-28". */
export const monthDay = (isoDate: string) => isoDate.slice(5)

/** M-16: workload bucket label. */
export function bucketLabel(kind: 'overdue' | 'week' | 'later', weekStart: string | null, index: number): string {
  if (kind === 'overdue') return 'Overdue'
  if (kind === 'later') return 'Later'
  return index === 1 ? 'This week' : monthDay(weekStart ?? '')
}

/** M-16: table-equivalent date range. */
export const weekRange = (start: string, end: string) => `${start} – ${end}`

/** M-17 and M-20: a UTC instant shown in the plant's zone, as "YYYY-MM-DD HH:mm" or "HH:mm:ss". */
export function formatInZone(utc: string, timeZone: string, part: 'dateTime' | 'time'): string {
  const parts = new Intl.DateTimeFormat('en-CA', {
    timeZone,
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit',
    hourCycle: 'h23',
  }).formatToParts(new Date(utc))
  const get = (type: Intl.DateTimeFormatPartTypes) => parts.find((p) => p.type === type)?.value ?? ''
  return part === 'time'
    ? `${get('hour')}:${get('minute')}:${get('second')}`
    : `${get('year')}-${get('month')}-${get('day')} ${get('hour')}:${get('minute')}`
}

/** M-19. */
export function shownNote(shown: number, total: number): string | null {
  return total > shown ? `Showing ${shown} of ${total}` : null
}
