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

export const getOrder = (id: string) => getJson<ProductionOrder>(`/api/production-orders/${encodeURIComponent(id)}`)

export const createOrder = (body: CreateProductionOrderRequest) =>
  sendRawJson<ProductionOrder>('POST', '/api/production-orders', orderJson(body))

export const updateOrder = (id: string, body: UpdateProductionOrderRequest) =>
  sendRawJson<ProductionOrder>('PUT', `/api/production-orders/${encodeURIComponent(id)}`, orderJson(body))

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
  )
}
