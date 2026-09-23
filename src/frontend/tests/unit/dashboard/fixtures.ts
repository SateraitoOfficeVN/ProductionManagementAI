import type { DashboardSnapshot, DashboardOrder } from '../../../src/features/dashboard/types'

const order = (n: number, dueDate: string, status: DashboardOrder['status'] = 'InProgress'): DashboardOrder => ({
  id: `o${n}`,
  orderNumber: `PO-2026-${String(n).padStart(5, '0')}`,
  product: { id: `p${n}`, sku: `P-${1000 + n}`, name: `Product ${n}` },
  quantity: 100 + n,
  dueDate,
  status,
})

/** Shaped like 003_DB's run-day figures (003_DD-API example), reduced to what the tests assert. */
export function snapshot(overrides: Partial<DashboardSnapshot> = {}): DashboardSnapshot {
  return {
    asOf: '2026-09-22T05:05:12Z',
    today: '2026-09-22',
    timeZone: 'Asia/Tokyo',
    statusCounts: { total: 124, draft: 35, inProgress: 25, completed: 56, cancelled: 8 },
    overdue: { total: 15, orders: Array.from({ length: 10 }, (_, i) => order(i + 1, `2026-09-${String(8 + i).padStart(2, '0')}`)) },
    dueSoon: { total: 2, orders: [order(21, '2026-09-22', 'Draft'), order(22, '2026-09-24')] },
    workload: [
      { kind: 'overdue', weekStart: null, weekEnd: null, orderCount: 15, quantity: 7410 },
      { kind: 'week', weekStart: '2026-09-22', weekEnd: '2026-09-27', orderCount: 7, quantity: 3643 },
      ...[28, 5, 12, 19, 26].map((d, i) => ({
        kind: 'week' as const,
        weekStart: i === 0 ? `2026-09-${d}` : `2026-10-${String(d).padStart(2, '0')}`,
        weekEnd: '2026-10-31',
        orderCount: 6,
        quantity: 100,
      })),
      { kind: 'week', weekStart: '2026-11-02', weekEnd: '2026-11-08', orderCount: 1, quantity: 950 },
      { kind: 'week', weekStart: '2026-11-09', weekEnd: '2026-11-15', orderCount: 1, quantity: 600 },
      { kind: 'later', weekStart: null, weekEnd: null, orderCount: 6, quantity: 2250 },
    ],
    topProducts: [
      { product: { id: 'p29', sku: 'P-1029', name: 'ドアヒンジ' }, openQuantity: 5430, activeOrderCount: 3 },
      { product: { id: 'p15', sku: 'P-1015', name: 'エンジンワイヤーハーネス' }, openQuantity: 5408, activeOrderCount: 1 },
    ],
    completedThisWeek: { orderCount: 3, quantity: 3097, from: '2026-09-21' },
    completedThisMonth: { orderCount: 24, quantity: 17922, from: '2026-09-01' },
    onTime: { onTimeCount: 23, completedCount: 30, windowStart: '2026-08-24' },
    leadTime: { averageDays: 13.7, orderCount: 30, windowStart: '2026-08-24' },
    completionTrend: [3, 3, 3, 2, 3, 3, 4, 6, 5, 7, 9, 3].map((n, i) => ({
      weekStart: `2026-W${i}`,
      weekEnd: `2026-W${i}e`,
      orderCount: n,
    })),
    ...overrides,
  }
}

export function emptySnapshot(): DashboardSnapshot {
  const base = snapshot()
  return {
    ...base,
    statusCounts: { total: 0, draft: 0, inProgress: 0, completed: 0, cancelled: 0 },
    overdue: { total: 0, orders: [] },
    dueSoon: { total: 0, orders: [] },
    workload: base.workload.map((b) => ({ ...b, orderCount: 0, quantity: 0 })),
    topProducts: [],
    completedThisWeek: { ...base.completedThisWeek, orderCount: 0, quantity: 0 },
    completedThisMonth: { ...base.completedThisMonth, orderCount: 0, quantity: 0 },
    onTime: { ...base.onTime, onTimeCount: 0, completedCount: 0 },
    leadTime: { ...base.leadTime, averageDays: null, orderCount: 0 },
    completionTrend: base.completionTrend.map((w) => ({ ...w, orderCount: 0 })),
  }
}
