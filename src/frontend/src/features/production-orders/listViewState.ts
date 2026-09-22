import { useCallback, useMemo } from 'react'
import { useSearchParams } from 'react-router-dom'
import {
  pageSizes,
  sortKeys,
  type ListViewState,
  type PageSize,
  type ProductionOrderSort,
  type ProductionOrderStatus,
} from './types'

// DD-002 module 6 / DD-002-SPD §2. One place converts between the URL query string and the typed view state, so the
// page never reads location.search itself and the API query is built from the same values that are displayed.

const statusValues: ProductionOrderStatus[] = ['Draft', 'InProgress', 'Completed', 'Cancelled']
const uuidPattern = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i
const datePattern = /^\d{4}-\d{2}-\d{2}$/

export const maxOrderNumberFilterLength = 20

export const defaultViewState: ListViewState = {
  statuses: [],
  productId: null,
  dueFrom: null,
  dueTo: null,
  orderNumber: null,
  sort: 'dueDate',
  dir: 'asc',
  page: 1,
  pageSize: 20,
}

/**
 * Parsing is total: it never throws and never yields an invalid state. An unreadable or out-of-range value falls
 * back to that parameter's default so a hand-edited URL still renders the screen (REQ-027), rather than an error page.
 */
export function parseViewState(params: URLSearchParams): ListViewState {
  const statuses = params
    .getAll('status')
    .filter((value): value is ProductionOrderStatus => statusValues.includes(value as ProductionOrderStatus))

  const productId = params.get('productId')
  const sort = params.get('sort')
  const dir = params.get('dir')
  const page = Number.parseInt(params.get('page') ?? '', 10)
  const pageSize = Number.parseInt(params.get('pageSize') ?? '', 10)
  const orderNumber = params.get('orderNumber')?.trim() ?? ''

  return {
    statuses: [...new Set(statuses)],
    productId: productId && uuidPattern.test(productId) ? productId : null,
    // A malformed date is dropped, but an inverted range is kept: both ends are visible in the panel, so silently
    // discarding one would hide the very thing the user has to fix (DD-002-SPD §2 step 3).
    dueFrom: parseDate(params.get('dueFrom')),
    dueTo: parseDate(params.get('dueTo')),
    orderNumber: orderNumber === '' ? null : orderNumber.slice(0, maxOrderNumberFilterLength),
    sort: isSort(sort) ? sort : defaultViewState.sort,
    dir: dir === 'desc' || dir === 'asc' ? dir : defaultViewState.dir,
    page: Number.isInteger(page) && page >= 1 ? page : defaultViewState.page,
    pageSize: isPageSize(pageSize) ? pageSize : defaultViewState.pageSize,
  }
}

/** Serializes the view state, omitting every parameter that equals its default so a shared link stays readable. */
export function toSearchParams(view: ListViewState): URLSearchParams {
  const params = new URLSearchParams()
  for (const status of view.statuses) {
    params.append('status', status)
  }
  if (view.productId) {
    params.set('productId', view.productId)
  }
  if (view.dueFrom) {
    params.set('dueFrom', view.dueFrom)
  }
  if (view.dueTo) {
    params.set('dueTo', view.dueTo)
  }
  if (view.orderNumber) {
    params.set('orderNumber', view.orderNumber)
  }
  if (view.sort !== defaultViewState.sort) {
    params.set('sort', view.sort)
  }
  if (view.dir !== defaultViewState.dir) {
    params.set('dir', view.dir)
  }
  if (view.page !== defaultViewState.page) {
    params.set('page', String(view.page))
  }
  if (view.pageSize !== defaultViewState.pageSize) {
    params.set('pageSize', String(view.pageSize))
  }
  return params
}

export function hasFilters(view: Pick<ListViewState, 'statuses' | 'productId' | 'dueFrom' | 'dueTo' | 'orderNumber'>) {
  return (
    view.statuses.length > 0 ||
    view.productId !== null ||
    view.dueFrom !== null ||
    view.dueTo !== null ||
    view.orderNumber !== null
  )
}

export function activeFilterCount(
  view: Pick<ListViewState, 'statuses' | 'productId' | 'dueFrom' | 'dueTo' | 'orderNumber'>,
) {
  return (
    (view.statuses.length > 0 ? 1 : 0) +
    (view.productId ? 1 : 0) +
    (view.dueFrom ? 1 : 0) +
    (view.dueTo ? 1 : 0) +
    (view.orderNumber ? 1 : 0)
  )
}

export function useListViewState(): [ListViewState, (next: ListViewState, options?: { replace?: boolean }) => void] {
  const [params, setParams] = useSearchParams()
  const view = useMemo(() => parseViewState(params), [params])

  const setView = useCallback(
    (next: ListViewState, options?: { replace?: boolean }) => {
      // Default is a new history entry, so Back returns to the previous view (E-19).
      setParams(toSearchParams(next), { replace: options?.replace ?? false })
    },
    [setParams],
  )

  return [view, setView]
}

function parseDate(value: string | null): string | null {
  if (!value || !datePattern.test(value)) {
    return null
  }
  const parsed = new Date(`${value}T00:00:00Z`)
  return Number.isNaN(parsed.getTime()) ? null : value
}

function isSort(value: string | null): value is ProductionOrderSort {
  return value !== null && (sortKeys as readonly string[]).includes(value)
}

function isPageSize(value: number): value is PageSize {
  return (pageSizes as readonly number[]).includes(value)
}
