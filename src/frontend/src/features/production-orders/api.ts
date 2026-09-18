import { getJson, sendJson } from '../../lib/apiClient'
import type {
  CreateProductionOrderRequest,
  Product,
  ProductionOrder,
  UpdateProductionOrderRequest,
} from './types'

export const listProducts = () => getJson<Product[]>('/api/products')

export const getOrder = (id: string) => getJson<ProductionOrder>(`/api/production-orders/${encodeURIComponent(id)}`)

export const createOrder = (body: CreateProductionOrderRequest) =>
  sendJson<ProductionOrder>('POST', '/api/production-orders', body)

export const updateOrder = (id: string, body: UpdateProductionOrderRequest) =>
  sendJson<ProductionOrder>('PUT', `/api/production-orders/${encodeURIComponent(id)}`, body)
