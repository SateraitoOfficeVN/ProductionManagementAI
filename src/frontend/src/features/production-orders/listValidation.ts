import { maxOrderNumberFilterLength } from './listViewState'
import type { ListFilters } from './types'

// 002_DD module 5, client half. Only V-09 and V-10 live here — the two rules a user can type wrong. V-11 (status),
// V-12 (product) and V-13 (sort/paging) cannot be violated through the UI, since those values come from the screen's
// own controls, so the client doesn't re-implement them. The server still enforces all five and is authoritative.

export type FilterField = 'dueFrom' | 'dueTo' | 'orderNumber'

export type FilterErrors = Partial<Record<FilterField, string>>

/** Field order used to move focus to the first invalid field (E-11). */
export const filterFieldOrder: FilterField[] = ['dueFrom', 'dueTo', 'orderNumber']

const datePattern = /^\d{4}-\d{2}-\d{2}$/

export function validateFilters(filters: ListFilters): FilterErrors {
  const errors: FilterErrors = {}

  const from = validateDate(filters.dueFrom)
  if (from === 'invalid') {
    errors.dueFrom = 'MSG-E016'
  }

  const to = validateDate(filters.dueTo)
  if (to === 'invalid') {
    errors.dueTo = 'MSG-E016'
  }

  if (from === 'valid' && to === 'valid' && filters.dueFrom! > filters.dueTo!) {
    // ISO dates compare correctly as strings, so no Date object is needed here.
    errors.dueFrom = 'MSG-E017'
  }

  if ((filters.orderNumber?.trim().length ?? 0) > maxOrderNumberFilterLength) {
    errors.orderNumber = 'MSG-E015'
  }

  return errors
}

function validateDate(value: string | null): 'empty' | 'valid' | 'invalid' {
  if (value === null || value.trim() === '') {
    return 'empty'
  }
  if (!datePattern.test(value)) {
    return 'invalid'
  }
  const parsed = new Date(`${value}T00:00:00Z`)
  return Number.isNaN(parsed.getTime()) ? 'invalid' : 'valid'
}
