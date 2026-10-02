/** Exact calendar values; date-only strings never use browser timezone conversions. */
export const weekdays = ['Mon','Tue','Wed','Thu','Fri','Sat','Sun'] as const
export type Weekday = typeof weekdays[number]
export function dateValid(v: unknown): v is string {
  if (typeof v !== 'string' || !/^\d{4}-\d{2}-\d{2}$/.test(v)) return false
  const [y,m,d] = v.split('-').map(Number)
  if (!y || m < 1 || m > 12 || d < 1) return false
  return d <= ([31,y % 4 === 0 && (y % 100 !== 0 || y % 400 === 0) ? 29 : 28,31,30,31,30,31,31,30,31,30,31][m-1] ?? 0)
}
export const monthValid = (v: unknown): v is string => typeof v === 'string' && /^\d{4}-\d{2}$/.test(v) && dateValid(v+'-01')
export const uuidValid = (v: unknown): v is string => typeof v === 'string' && /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(v) && v !== '00000000-0000-0000-0000-000000000000'
export const versionValid = (v: unknown,zero=false): v is string => typeof v === 'string' && (zero ? /^(0|[1-9][0-9]*)$/ : /^[1-9][0-9]*$/).test(v) && v.length <= 19 && BigInt(v) <= 9223372036854775807n
/** .NET Unicode whitespace includes U+0085 and excludes U+FEFF. */
export const trimText = (v: string) => v.replace(/^\p{White_Space}+|\p{White_Space}+$/gu,'')
export const wellFormed = (v: string) => !/[\uD800-\uDBFF](?![\uDC00-\uDFFF])|(?<![\uD800-\uDBFF])[\uDC00-\uDFFF]/u.test(v)
export function normalizeText(v: string,max: number): string|null|undefined {
  if (!wellFormed(v)) return undefined
  const text = trimText(v)
  return Array.from(text).length > max ? undefined : text || null
}
export const decimalValid = (v: unknown): v is string => typeof v === 'string' && /^(0|[1-9][0-9]*)(\.[0-9]{1,3})?$/.test(v) && v.length <= 32
export function hoursValid(v: string): boolean {
  v = trimText(v)
  if (!decimalValid(v) || v.length > 8) return false
  const [whole,fraction=''] = v.split('.')
  const scaled = BigInt(whole)*1000n + BigInt(fraction.padEnd(3,'0'))
  return scaled > 0n && scaled <= 24000n
}
export const bodyWithinLimit = (v: unknown) => new TextEncoder().encode(JSON.stringify(v)).length <= 8192
export function formatExact(v: string): string {
  if (!decimalValid(v)) throw new Error('CALENDAR_DECIMAL_INVALID')
  const [whole,fraction] = v.split('.')
  return whole.replace(/\B(?=(\d{3})+(?!\d))/g,',') + (fraction === undefined ? '' : '.'+fraction)
}
export interface CalendarUrl { month?: string; date?: string; lineId?: string; productId?: string; mode: 'month'|'day'|'weekly'|'history' }
export function parseCalendarUrl(search: string): CalendarUrl|null {
  const q = new URLSearchParams(search), allowed = new Set(['month','date','lineId','productId','mode'])
  if ([...q.keys()].some(k => !allowed.has(k) || q.getAll(k).length !== 1)) return null
  const month=q.get('month') ?? undefined,date=q.get('date') ?? undefined,lineId=q.get('lineId') ?? undefined,productId=q.get('productId') ?? undefined,mode=q.get('mode') ?? 'month'
  if (month !== undefined && !monthValid(month) || date !== undefined && !dateValid(date) || lineId !== undefined && !uuidValid(lineId) || productId !== undefined && (!uuidValid(productId) || !lineId) || month && date && !date.startsWith(month+'-') || mode === 'day' && !date) return null
  if (mode !== 'month' && mode !== 'day' && mode !== 'weekly' && mode !== 'history') return null
  return {month:month ?? date?.slice(0,7),date,lineId:lineId?.toLowerCase(),productId:productId?.toLowerCase(),mode}
}
