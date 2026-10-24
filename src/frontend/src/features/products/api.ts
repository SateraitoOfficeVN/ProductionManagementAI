import { getJson, sendJson } from '../../lib/apiClient'

export const productUnits = ['個', '本', '枚', '台', 'セット', 'kg', 'm'] as const
export type ProductUnit = (typeof productUnits)[number]

/** Product type codes in display order (004_DD-TYPE). */
export const productTypes = ['FINISHED', 'SEMI_FINISHED', 'PART', 'RAW_MATERIAL'] as const
export type ProductType = (typeof productTypes)[number]

export const isProductType = (value: string | null): value is ProductType =>
  (productTypes as readonly (string | null)[]).includes(value)

export interface ProductMasterItem {
  id: string
  sku: string
  name: string
  type: ProductType
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
  type: ProductType | ''
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

/** The type filter is sent only when one is chosen (004_DD-SPD-TYPE SPD-01 step 2). */
export const listMaster = (q: string, state: string, type: ProductType | '', page: number, pageSize: number, signal?: AbortSignal) =>
  getJson<ProductPage>(`/api/product-master?${new URLSearchParams({ q, state, ...(type && { type }), page: String(page), pageSize: String(pageSize) })}`, signal)

export const getMaster = (id: string, signal?: AbortSignal) => getJson<ProductMasterItem>(`/api/product-master/${encodeURIComponent(id)}`, signal)

export const createMaster = (draft: ProductDraft) =>
  sendJson<ProductMasterItem>('POST', '/api/product-master', draft)

export const updateMaster = (id: string, draft: ProductDraft, version: number) =>
  sendJson<ProductMasterItem>('PUT', `/api/product-master/${encodeURIComponent(id)}`, { ...draft, version })

export const retireMaster = (id: string, version: number) =>
  sendJson<ProductMasterItem>('POST', `/api/product-master/${encodeURIComponent(id)}/retire`, { version })
