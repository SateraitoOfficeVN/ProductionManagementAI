import { getJson, sendJson } from '../../lib/apiClient'

export interface LineSummary { id: string; code: string; name: string; workingHoursPerDay: string; isActive: boolean; updatedAt: string; version: string }
export interface ProductChoice { id: string; sku: string; name: string; unit: string; unitRevision: string; isActive: boolean }
export interface PairDetail { product: ProductChoice; minutesPerUnit: string; confirmedUnit: string; confirmedUnitRevision: string; isActive: boolean; requiresUnitConfirmation: boolean; updatedAt: string }
export interface Page<T> { items: T[]; total: number; page: number; pageSize: number }
export interface LineDetail extends LineSummary { pairs: Page<PairDetail> }
export interface EligibleLine extends LineSummary { minutesPerUnit: string; unit: string }
export interface EligiblePage extends Page<EligibleLine> { product: ProductChoice }
export interface TimingInput { productId: string; minutesPerUnit: string; expectedUnit: string; expectedUnitRevision: string; confirmUnit: boolean }
export type ProductChange = ({ action: 'add' | 'setTiming' } & TimingInput) | { action: 'retire'; productId: string }
export interface CreateLine { code: string; name: string; workingHoursPerDay: string; products: TimingInput[] }
export interface UpdateLine { name: string; workingHoursPerDay: string; version: string; productChanges: ProductChange[] }

const units = new Set(['個', '本', '枚', '台', 'セット', 'kg', 'm'])
const record = (value: unknown): value is Record<string, unknown> => typeof value === 'object' && value !== null && !Array.isArray(value)
const text = (value: unknown): value is string => typeof value === 'string'
const uuid = (value: unknown): value is string => text(value) && /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(value) && value !== '00000000-0000-0000-0000-000000000000'
const utc = (value: unknown): value is string => text(value) && /^\d{4}-\d{2}-\d{2}T.*Z$/.test(value) && Number.isFinite(Date.parse(value))
const token = (value: unknown, max: bigint): value is string => text(value) && /^(0|[1-9][0-9]*)$/.test(value) && value.length <= 19 && BigInt(value) <= max
export const decimalValid = (value: string, max: string): boolean => {
  if (!/^(0|[1-9][0-9]*)(\.[0-9]{1,3})?$/.test(value) || value.length > 32) return false
  const scaled = (input: string) => { const [whole, fraction = ''] = input.split('.'); return BigInt(whole) * 1000n + BigInt(fraction.padEnd(3, '0')) }
  return scaled(value) > 0n && scaled(value) <= scaled(max)
}
const product = (v: unknown): v is ProductChoice => record(v) && uuid(v.id) && text(v.sku) && text(v.name) && text(v.unit) && units.has(v.unit) && token(v.unitRevision, 9223372036854775807n) && typeof v.isActive === 'boolean'
const summary = (v: unknown): v is LineSummary => record(v) && uuid(v.id) && text(v.code) && text(v.name) && text(v.workingHoursPerDay) && decimalValid(v.workingHoursPerDay, '24') && typeof v.isActive === 'boolean' && utc(v.updatedAt) && token(v.version, 4294967295n)
const pair = (v: unknown): v is PairDetail => record(v) && product(v.product) && text(v.minutesPerUnit) && decimalValid(v.minutesPerUnit, '999999999.999') && text(v.confirmedUnit) && units.has(v.confirmedUnit) && token(v.confirmedUnitRevision, 9223372036854775807n) && typeof v.isActive === 'boolean' && typeof v.requiresUnitConfirmation === 'boolean' && utc(v.updatedAt)
const page = <T,>(v: unknown, item: (value: unknown) => value is T): v is Page<T> => record(v) && Array.isArray(v.items) && v.items.length <= 50 && v.items.every(item) && Number.isSafeInteger(v.total) && Number(v.total) >= 0 && Number.isInteger(v.page) && Number(v.page) >= 1 && Number(v.page) <= 10000 && v.pageSize === 50
const detail = (v: unknown): v is LineDetail => summary(v) && record(v) && page(v.pairs, pair)
const eligible = (v: unknown): v is EligibleLine => summary(v) && record(v) && text(v.minutesPerUnit) && decimalValid(v.minutesPerUnit, '999999999.999') && text(v.unit) && units.has(v.unit)
function decode<T>(value: unknown, guard: (input: unknown) => input is T): T { if (!guard(value)) throw new Error('Invalid production-line response'); return value }
const signal = (caller?: AbortSignal) => caller ? AbortSignal.any([caller, AbortSignal.timeout(20000)]) : AbortSignal.timeout(20000)
const query = (values: Record<string, string | undefined>) => new URLSearchParams(Object.entries(values).filter((entry): entry is [string, string] => entry[1] !== undefined)).toString()
const base = '/api/production-lines'
export const listLines = async (q: string, state: string, currentPage: number, caller?: AbortSignal) => decode(await getJson<unknown>(`${base}?${query({ q, state, page: String(currentPage) })}`, signal(caller)), (v): v is Page<LineSummary> => page(v, summary))
export const getLine = async (id: string, currentPage = 1, caller?: AbortSignal) => decode(await getJson<unknown>(`${base}/${encodeURIComponent(id)}?pairsPage=${currentPage}`, signal(caller)), detail)
export const productChoices = async (q: string, currentPage: number, lineId?: string, caller?: AbortSignal) => decode(await getJson<unknown>(`${base}/product-choices?${query({ q, page: String(currentPage), lineId })}`, signal(caller)), (v): v is Page<ProductChoice> => page(v, product))
export const eligibleLines = async (productId: string, q: string, currentPage: number, caller?: AbortSignal) => decode(await getJson<unknown>(`${base}/eligible?${query({ productId, q, page: String(currentPage) })}`, signal(caller)), (v): v is EligiblePage => record(v) && product(v.product) && page(v, eligible))
export const createLine = async (body: CreateLine) => decode(await sendJson<unknown>('POST', base, body, signal()), detail)
export const updateLine = async (id: string, body: UpdateLine) => decode(await sendJson<unknown>('PUT', `${base}/${encodeURIComponent(id)}`, body, signal()), detail)
export const retireLine = async (id: string, version: string) => decode(await sendJson<unknown>('POST', `${base}/${encodeURIComponent(id)}/retire`, { version }, signal()), summary)

/** Checks aggregate action and UTF-8 body limits before a mutation is sent. */
export const mutationWithinLimits = (body: unknown, actions: number): boolean => actions <= 1000 && new TextEncoder().encode(JSON.stringify(body)).length <= 256 * 1024
