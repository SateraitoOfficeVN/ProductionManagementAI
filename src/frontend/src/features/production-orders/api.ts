import { getFile, getJson, sendRawJson } from '../../lib/apiClient'
import { toSearchParams } from './listViewState'
import { labels } from './messages'
import type {
  CreateProductionOrderRequest,
  ListViewState,
  PagedResult,
  Product,
  ProductionOrder,
  ProductionOrderListItem,
  UpdateProductionOrderRequest,
} from './types'

export const listProducts = () => getJson<Product[]>('/api/products')

export const getOrder = (id: string) => getJson<ProductionOrder>(`/api/production-orders/${encodeURIComponent(id)}`).then(requireLineProjection)

export const createOrder = (body: CreateProductionOrderRequest) =>
  sendRawJson<ProductionOrder>('POST', '/api/production-orders', orderJson(body)).then(requireLineProjection)

export const updateOrder = (id: string, body: UpdateProductionOrderRequest) =>
  sendRawJson<ProductionOrder>('PUT', `/api/production-orders/${encodeURIComponent(id)}`, orderJson(body)).then(requireLineProjection)

/** Keep the validated decimal text as the JSON number token; JSON.stringify would convert it through Number. */
function orderJson(body: CreateProductionOrderRequest | UpdateProductionOrderRequest): string {
  if (!/^(?:0|[1-9]\d*)(?:\.\d{1,3})?$/.test(body.quantity)) throw new Error('QUANTITY_UNIT_INVALID')
  const { quantity, ...fields } = body
  return `${JSON.stringify(fields).slice(0, -1)},"quantity":${quantity}}`
}

/**
 * 002_DD-API §1. The query string is built by the same serializer the URL uses, so what the user sees and what is
 * queried cannot diverge. The signal cancels a query the user has already superseded.
 */
export function listOrders(view: ListViewState, signal?: AbortSignal) {
  const query = toSearchParams(view).toString()
  return getJson<PagedResult<ProductionOrderListItem>>(
    query === '' ? '/api/production-orders' : `/api/production-orders?${query}`,
    signal,
  ).then(result => { result.items.forEach(requireLineProjection); return result })
}

export interface ExportedFile {
  blob: Blob
  fileName: string
  /** X-Total-Count: the rows actually written, read from the same snapshot as the file (002_DD-API-CSV). */
  count: number
}

/**
 * 002_DD-CSV module 3 / 002_DD-SPD-CSV P-18. The query comes from the same serializer as the URL and the list, minus
 * paging: the export always covers every page (WI-016 DEC-002). A missing or malformed count is a failed export —
 * the screen must never announce a wrong number.
 */
export async function exportOrdersCsv(view: ListViewState, signal?: AbortSignal): Promise<ExportedFile> {
  const params = toSearchParams(view)
  params.delete('page')
  params.delete('pageSize')
  const query = params.toString()
  const response = await getFile(
    query === '' ? '/api/production-orders/export' : `/api/production-orders/export?${query}`,
    signal,
  )
  const countHeader = response.headers.get('X-Total-Count') ?? ''
  if (!/^\d+$/.test(countHeader)) throw new Error('EXPORT_COUNT_MISSING')
  const blob = await response.blob()
  return { blob, fileName: exportFileName(response.headers.get('Content-Disposition')), count: Number(countHeader) }
}

/**
 * The file name from Content-Disposition: filename* (RFC 8187) first, then filename, else a name built from the
 * browser clock. Path separators and control characters are removed either way (002_DD-SPD-CSV P-18 step 6).
 */
export function exportFileName(disposition: string | null, now: Date = new Date()): string {
  const star = disposition?.match(/filename\*\s*=\s*UTF-8''([^;]+)/i)?.[1]
  const plain = disposition?.match(/filename\s*=\s*"?([^";]+)"?/i)?.[1]
  let name: string | undefined
  try {
    name = star ? decodeURIComponent(star.trim()) : plain?.trim()
  } catch {
    name = plain?.trim()
  }
  // eslint-disable-next-line no-control-regex
  const safe = name?.replace(/[\\/\u0000-\u001f\u007f]/g, '').trim()
  if (safe) return safe
  const pad = (n: number) => String(n).padStart(2, '0')
  const stamp = `${now.getFullYear()}${pad(now.getMonth() + 1)}${pad(now.getDate())}-${pad(now.getHours())}${pad(now.getMinutes())}`
  return labels.list.exportFileName(stamp)
}

/** Missing assignment metadata is an invalid response, never a fabricated unassigned history. */
function requireLineProjection<T extends { line: ProductionOrder['line'] }>(value: T): T {
  if (!Object.prototype.hasOwnProperty.call(value, 'line')) throw new Error('INVALID_ORDER_LINE_PROJECTION')
  const line = value.line
  if (line !== null && (typeof line !== 'object' || !line || typeof line.id !== 'string' || typeof line.code !== 'string' || typeof line.name !== 'string' || typeof line.isActive !== 'boolean')) throw new Error('INVALID_ORDER_LINE_PROJECTION')
  return value
}
