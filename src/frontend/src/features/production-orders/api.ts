import { getJson, sendJson } from '../../lib/apiClient'
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
  sendJson<ProductionOrder>('POST', '/api/production-orders', body)

export const updateOrder = (id: string, body: UpdateProductionOrderRequest) =>
  sendJson<ProductionOrder>('PUT', `/api/production-orders/${encodeURIComponent(id)}`, body)

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
