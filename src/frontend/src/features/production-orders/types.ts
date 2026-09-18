// Matches DD-001-API (ProductionOrderResponse / ProductResponse).

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
