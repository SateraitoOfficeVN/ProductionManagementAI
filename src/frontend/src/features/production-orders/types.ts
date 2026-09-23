// Matches 001_DD-API (ProductionOrderResponse / ProductResponse) and 002_DD-API (the list contract).

export type ProductionOrderStatus = 'Draft' | 'InProgress' | 'Completed' | 'Cancelled'

export interface Product {
  id: string
  sku: string
  name: string
}

export interface ProductionOrder {
  id: string
  orderNumber: string
  productId: string
  quantity: number
  dueDate: string
  status: ProductionOrderStatus
  allowedNextStatuses: ProductionOrderStatus[]
  isProductQuantityEditable: boolean
  notes: string | null
  createdAt: string
  updatedAt: string
  version: number
}

export interface CreateProductionOrderRequest {
  productId: string
  quantity: number
  dueDate: string
  notes: string | null
}

export interface UpdateProductionOrderRequest extends CreateProductionOrderRequest {
  status: ProductionOrderStatus
  version: number
}

// ---- Screen B: 002_DD-API. The list row is deliberately narrower than ProductionOrder: no notes, no version,
// no allowedNextStatuses — the list neither shows nor writes them.

export interface ProductionOrderListItem {
  id: string
  orderNumber: string
  product: Product
  quantity: number
  dueDate: string
  status: ProductionOrderStatus
  isOverdue: boolean
  updatedAt: string
}

export interface PagedResult<T> {
  items: T[]
  total: number
  page: number
  pageSize: number
  sort: ProductionOrderSort
  dir: SortDirection
}

export const sortKeys = ['dueDate', 'orderNumber', 'product', 'quantity', 'status', 'updatedAt'] as const
export type ProductionOrderSort = (typeof sortKeys)[number]

export type SortDirection = 'asc' | 'desc'

export const pageSizes = [10, 20, 50, 100] as const
export type PageSize = (typeof pageSizes)[number]

/** The whole view state of SCR-002, and exactly what the URL carries (002_BD 0-3, REQ-027). */
export interface ListViewState {
  statuses: ProductionOrderStatus[]
  productId: string | null
  dueFrom: string | null
  dueTo: string | null
  orderNumber: string | null
  sort: ProductionOrderSort
  dir: SortDirection
  page: number
  pageSize: PageSize
}

/** Just the filter half, which the filter panel edits as a draft before publishing it (DEC-008). */
export type ListFilters = Pick<ListViewState, 'statuses' | 'productId' | 'dueFrom' | 'dueTo' | 'orderNumber'>
