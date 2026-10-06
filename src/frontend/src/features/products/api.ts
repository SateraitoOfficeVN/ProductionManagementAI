import { getJson, sendJson } from '../../lib/apiClient'

export const productUnits = ['個', '本', '枚', '台', 'セット', 'kg', 'm'] as const
export type ProductUnit = (typeof productUnits)[number]

export interface ProductMasterItem {
  id: string
  sku: string
  name: string
  unit: ProductUnit
  drawingNumber: string | null
  isActive: boolean
  updatedAt: string
  version: number
  unitLocked: boolean
}

export interface ProductDraft {
  sku: string
  name: string
  unit: string
  drawingNumber: string
}

export interface ProductPage {
  items: ProductMasterItem[]
  total: number
  page: number
  pageSize: number
  sort: 'sku'
  dir: 'asc'
}

export const listMaster = (q: string, state: string, page: number, pageSize: number, signal?: AbortSignal) =>
  getJson<ProductPage>(`/api/product-master?${new URLSearchParams({ q, state, page: String(page), pageSize: String(pageSize) })}`, signal)

export const getMaster = (id: string, signal?: AbortSignal) => getJson<ProductMasterItem>(`/api/product-master/${encodeURIComponent(id)}`, signal)

export const createMaster = (draft: ProductDraft) =>
  sendJson<ProductMasterItem>('POST', '/api/product-master', draft)

export const updateMaster = (id: string, draft: ProductDraft, version: number) =>
  sendJson<ProductMasterItem>('PUT', `/api/product-master/${encodeURIComponent(id)}`, { ...draft, version })

export const retireMaster = (id: string, version: number) =>
  sendJson<ProductMasterItem>('POST', `/api/product-master/${encodeURIComponent(id)}/retire`, { version })
