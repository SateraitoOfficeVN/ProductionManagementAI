// Japanese display formats (WI-005 DEC-006). Only the presentation changes: which timezone a value is shown in is
// decided by each screen's design, as before.

const numberFormat = new Intl.NumberFormat('ja-JP')

export const formatNumber = (n: number) => numberFormat.format(n)

/** A plant-local date "2026-09-30" → "2026/09/30"; anything unparseable is returned unchanged. */
export const formatDate = (isoDate: string) =>
  /^\d{4}-\d{2}-\d{2}$/.test(isoDate) ? isoDate.replaceAll('-', '/') : isoDate

const timestampFormat = new Intl.DateTimeFormat('ja-JP', {
  year: 'numeric',
  month: '2-digit',
  day: '2-digit',
  hour: '2-digit',
  minute: '2-digit',
  hourCycle: 'h23',
})

/** A UTC instant in the browser's timezone (001_BD M-04, 002_BD M-10) as "2026/09/18 10:02". */
export function formatTimestamp(utc: string): string {
  const parsed = new Date(utc)
  return Number.isNaN(parsed.getTime()) ? utc : timestampFormat.format(parsed)
}
