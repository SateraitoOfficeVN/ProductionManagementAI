// Client-side checks (001_BD V-01–V-05). Advisory only: the server repeats every check and is authoritative.
// Each returns a 001_DD message ID, or null when valid.

export const MAX_QUANTITY = 999_999_999
export const MAX_NOTES = 500

/** Unicode code points, matching the server and PostgreSQL varchar(500) (an emoji counts once). */
export function codePointLength(value: string): number {
  return [...value].length
}

/** The browser-local date as YYYY-MM-DD. The server uses the plant date (Asia/Tokyo) and decides (DEC-021). */
export function localToday(now: Date = new Date()): string {
  const y = now.getFullYear()
  const m = String(now.getMonth() + 1).padStart(2, '0')
  const d = String(now.getDate()).padStart(2, '0')
  return `${y}-${m}-${d}`
}

export function validateProduct(productId: string): string | null {
  return productId === '' ? 'MSG-E001' : null
}

export function validateQuantity(quantity: string): string | null {
  const trimmed = quantity.trim()
  if (!/^\d+$/.test(trimmed) || Number(trimmed) < 1) {
    return 'MSG-E003'
  }
  return Number(trimmed) > MAX_QUANTITY ? 'MSG-E010' : null
}

/**
 * "Today or later" applies on create, and on edit only when the date was changed (DEC-009), so an overdue
 * order can still be saved.
 */
export function validateDueDate(dueDate: string, initialDueDate: string | null, today: string): string | null {
  if (dueDate === '') {
    return 'MSG-E004'
  }
  const changed = initialDueDate === null || dueDate !== initialDueDate
  return changed && dueDate < today ? 'MSG-E005' : null
}

export function validateNotes(notes: string): string | null {
  return codePointLength(notes.trim()) > MAX_NOTES ? 'MSG-E006' : null
}
