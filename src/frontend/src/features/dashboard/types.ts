import type { ProductionOrderStatus } from '../production-orders/types'

// Mirrors 003_DD-API §1 field for field.

export interface ProductSummary {
  id: string
  sku: string
  name: string
}

export interface DashboardOrder {
  id: string
  orderNumber: string
  product: ProductSummary
  quantity: number
  dueDate: string
  status: ProductionOrderStatus
}

export interface OrderGroup {
  total: number
  orders: DashboardOrder[]
}

export interface WorkloadBucket {
  kind: 'overdue' | 'week' | 'later'
  weekStart: string | null
  weekEnd: string | null
  orderCount: number
  quantity: number
}

export interface DashboardSnapshot {
  asOf: string
  today: string
  timeZone: string
  statusCounts: { total: number; draft: number; inProgress: number; completed: number; cancelled: number }
  overdue: OrderGroup
  dueSoon: OrderGroup
  workload: WorkloadBucket[]
  topProducts: { product: ProductSummary; openQuantity: number; activeOrderCount: number }[]
  completedThisWeek: { orderCount: number; quantity: number; from: string }
  completedThisMonth: { orderCount: number; quantity: number; from: string }
  onTime: { onTimeCount: number; completedCount: number; windowStart: string }
  leadTime: { averageDays: number | null; orderCount: number; windowStart: string }
  completionTrend: { weekStart: string; weekEnd: string; orderCount: number }[]
}

/** 003_DD-API §2. */
export interface SystemHealth {
  database: 'ok' | 'unavailable'
  checkedAt: string
}
