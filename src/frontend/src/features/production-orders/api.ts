import { getJson, sendRawJson } from '../../lib/apiClient'
import { toSearchParams } from './listViewState'
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

/** Missing assignment metadata is an invalid response, never a fabricated unassigned history. */
function requireLineProjection<T extends { line: ProductionOrder['line'] }>(value: T): T {
  if (!Object.prototype.hasOwnProperty.call(value, 'line')) throw new Error('INVALID_ORDER_LINE_PROJECTION')
  const line = value.line
  if (line !== null && (typeof line !== 'object' || !line || typeof line.id !== 'string' || typeof line.code !== 'string' || typeof line.name !== 'string' || typeof line.isActive !== 'boolean')) throw new Error('INVALID_ORDER_LINE_PROJECTION')
  return value
}
